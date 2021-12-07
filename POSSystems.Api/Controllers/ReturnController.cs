using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neodynamic.SDK.Web;
using POSSystems.Core;
using POSSystems.Core.Dtos.Return;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Core.Dtos.SalesMaster;
using POSSystems.Core.Models;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Services;
using POSSystems.Web.Infrastructure.TranCloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POSSystems.Web.Infrastructure;

namespace POSSystems.Web.Controllers
{
    public class ReturnController : BaseController<ReturnController>
    {
        private readonly TranCloudConfig _tranCloudOptions;
        private readonly PrintingInfo _printingInfo;
        private readonly ApplicationData _applicationData;

        private bool _vssIntegrated;
        private bool _loyaltyEnabled;
        private bool _creditCardLikeCash;
        private bool _trancloudEnabled;

        public IConfigurationRoot Configuration { get; set; }

        public ReturnController(IUnitOfWork unitOfWork,
            ILogger<ReturnController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IOptions<TranCloudConfig> tranCloudOptionsAccessor,
            IOptions<PrintingInfo> printingInfoAccessor,
            ApplicationData applicationData,
            IWebHostEnvironment env,
            IMapper mapper
            )
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
            _tranCloudOptions = tranCloudOptionsAccessor?.Value;
            _printingInfo = printingInfoAccessor?.Value;
            _applicationData = applicationData;

            _trancloudEnabled = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("trancloudEnabled", "True"));
            _loyaltyEnabled = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("loyaltyEnabled", "False"));
            _vssIntegrated = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("vssIntegrated", "True"));
            _creditCardLikeCash = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("creditcardLikeCash", "False"));
        }

        public IActionResult Index()
        {
            ViewData["WCPScript"] = WebClientPrint.CreateScript(Url.Action("ProcessRequest", "WebClientPrintAPI", null,
                Url.ActionContext.HttpContext.Request.Scheme), Url.Action("PrintReceipt", "Common",
                null, Url.ActionContext.HttpContext.Request.Scheme), Url.ActionContext.HttpContext.Session.Id);
            ViewData["Version"] = _applicationData.Version;

            return View(new ReturnDto { EnableLoyalty = _loyaltyEnabled, CreditCardLikeCash = _creditCardLikeCash });
        }

        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Return })]
        public IActionResult Invoice(string invoiceNo)
        {
            try
            {
                var salesMaster = _unitOfWork.SalesMasterRepository.GetByInvoiceNo(invoiceNo, true);

                if (salesMaster == null)
                {
                    _logger.LogWarning($"Invoice no.:{invoiceNo} not found.");
                    return BadRequest($"Invoice no.:{invoiceNo} not found.");
                }

                if (salesMaster.PaymentStatus.DehumanizeTo<PaymentStatus>() == PaymentStatus.CompletelyReturned)
                {
                    _logger.LogWarning($"Invoice no.:{invoiceNo} already returned.");
                    return BadRequest($"Invoice no.:{invoiceNo} already returned.");
                }

                salesMaster.SalesDetails = salesMaster.SalesDetails.OrderBy(sd => sd.ItemType).ToList();

                var salesMasterDto = _mapper.Map<SalesMasterDto>(salesMaster);

                return Ok(salesMasterDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Paymentables(string invoiceNo)
        {
            try
            {
                var salesMaster = _unitOfWork.SalesMasterRepository.GetByInvoiceNo(invoiceNo, true);

                if (salesMaster == null)
                    return NotFound();

                var paymentables = salesMaster.Transactions.Where(t => t.TransactionType.DehumanizeTo<TransactionType>() == TransactionType.Sales).GroupBy(s => s.PayMethod)
                   .Select(
                       g => new PaymentableDto
                       {
                           PaymentType = g.Key,
                           Total = g.Sum(s => s.Amount)
                       })
                    .OrderBy(p => p.PaymentType.DehumanizeTo<PaymentType>());

                return Ok(paymentables);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task ResetPinpad()
        {
            try
            {
                var resetjson = TranCloudReset.GetTranCloudReset(_tranCloudOptions);
                var sresponse = await TranCloudWebRequest.DoTranCloudRequest(resetjson, _tranCloudOptions, _logger);
                var resetResponse = SignatureResponse.MapTranCloudResponse(sresponse);
            }
            catch
            {
                _logger.LogInformation("Pinpad reset not working");
            }
        }

        [HttpPost]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Sale })]
        public async Task<IActionResult> Accept(CreateReturnDto createReturnDto)
        {
            var salesMaster = _unitOfWork.SalesMasterRepository.GetByInvoiceNo(createReturnDto.InvoiceNo, true);
            if (salesMaster == null)
            {
                _logger.LogWarning($"Invoice no.:{createReturnDto.InvoiceNo} was not found.");
                return BadRequest($"Invoice no.:{createReturnDto.InvoiceNo} was not found.");
            }

            if (salesMaster.PaymentStatus.DehumanizeTo<PaymentStatus>() == PaymentStatus.CompletelyReturned)
            {
                _logger.LogWarning($"Invoice no.:{createReturnDto.InvoiceNo} already returned.");
                return BadRequest($"Invoice no.:{createReturnDto.InvoiceNo} already returned.");
            }

            if (createReturnDto.PosItems == null || createReturnDto.PosItems.Count == 0)
            {
                _logger.LogWarning($"For Invoice no.:{createReturnDto.InvoiceNo} no items selected.");
                return BadRequest($"For Invoice no.:{createReturnDto.InvoiceNo} no items selected.");
            }

            var transactionAuthCode = string.IsNullOrEmpty(salesMaster.Transactions[0].AuthCode) ? "" : salesMaster.Transactions[0].AuthCode.ToString();
            var transactionAcqRefData = string.IsNullOrEmpty(salesMaster.Transactions[0].AcqRefData) ? "" : salesMaster.Transactions[0].AcqRefData.ToString();

            Customer customer = null;
            double loyaltyAmount = 0;
            if (_loyaltyEnabled && salesMaster.LoyaltyCard != null)
            {
                customer = _unitOfWork.CustomerRepository.SingleOrDefault(c => c.LoyaltyCardNumber == salesMaster.LoyaltyCard);
                if (customer == null)
                {
                    _logger.LogError($"Customer was not found for {salesMaster.LoyaltyCard}");
                    return BadRequest($"Customer was not found for {salesMaster.LoyaltyCard}");
                }
                else
                    double.TryParse(createReturnDto.LoyaltyAmount, out loyaltyAmount);
            }

            var salesReturns = new List<SalesReturn>();
            foreach (var kvp in createReturnDto.PosItems)
            {
                var upcCode = kvp.Id;
                int? productId = null;
                if (kvp.ItemType == "PI")
                {
                    var productDetail = _unitOfWork.ProductRepository.Get(int.Parse(kvp.Id));

                    if (productDetail == null)
                    {
                        _logger.LogWarning($"Selected product id {kvp.Id} was not found.");
                        return BadRequest($"Selected product id {kvp.Id} was not found.");
                    }

                    if (!_unitOfWork.SalesDetailRepository.ItemExistsInInvoice(createReturnDto.InvoiceNo, int.Parse(kvp.Id), kvp.Quantity))
                    {
                        _logger.LogWarning($"Selected product id {kvp.Id} is not in the invoice.");
                        return BadRequest($"Selected product id {kvp.Id} is not in the invoice.");
                    }

                    upcCode = productDetail.UpcCode;
                    productId = int.Parse(kvp.Id);
                }

                var salesReturn = new SalesReturn
                {
                    SalesId = salesMaster.SalesId,
                    Quantity = kvp.Quantity,
                    ProductId = productId,
                    InvoiceNo = createReturnDto.InvoiceNo,
                    UpcCode = upcCode,
                    ReturnAmount = kvp.UnitPriceAfterTax * kvp.Quantity,
                    ItemType = kvp.ItemType
                };

                salesReturns.Add(salesReturn);
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4();

            ReturnTransaction returnTransaction = null;

            InvoiceDTO invoiceDTO = CreateInvoice(createReturnDto);

            PaymentInfoResponse paymentResponse;

            var paymentType = createReturnDto.TransactionType.DehumanizeTo<PaymentType>();

            var transactionType = salesMaster.Transactions.Where(t => t.PayMethod.DehumanizeTo<PaymentType>() == paymentType).SingleOrDefault();

            bool cardTransaction = false;
            if (paymentType == PaymentType.Manual)
                cardTransaction = salesMaster.Transactions.Any(t => t.PayMethod.DehumanizeTo<PaymentType>() == PaymentType.Card);

            if (transactionType == null && (paymentType == PaymentType.Manual && cardTransaction == false))
            {
                _logger.LogError($"Transaction of {paymentType.Humanize()} type not found for invoice no:{createReturnDto.InvoiceNo}");
                return BadRequest($"Transaction of {paymentType.Humanize()} type transaction not found for invoice no:{createReturnDto.InvoiceNo}");
            }

            var transaction = new Transaction { PayMethod = paymentType.Humanize(), TransactionType = TransactionType.SalesReturn.Humanize() };

            switch (paymentType)
            {
                case PaymentType.Cash:
                    returnTransaction = new ReturnBasicTransaction(createReturnDto.InvoiceNo);
                    break;

                case PaymentType.Bank:
                    returnTransaction = new ReturnBasicTransaction(createReturnDto.InvoiceNo);
                    break;

                case PaymentType.Manual:
                    returnTransaction = new TranCloudManualReturnTransaction(createReturnDto.InvoiceNo, _logger, _tranCloudOptions, "", "");
                    break;

                case PaymentType.Card:
                    if (_creditCardLikeCash && !_trancloudEnabled)
                    {
                        returnTransaction = new ReturnBasicTransaction(createReturnDto.InvoiceNo);
                    }
                    else
                    {
                        returnTransaction = new TranCloudCardReturnTransaction(createReturnDto.InvoiceNo, _logger, _tranCloudOptions, "", "");
                    }
                    break;

                case PaymentType.FSA:
                    returnTransaction = new TranCloudFSAReturnTransaction(createReturnDto.InvoiceNo, _logger, _tranCloudOptions, transactionAuthCode, transactionAcqRefData);
                    break;

                case PaymentType.Manualfsa:
                    returnTransaction = new TranCloudManualFSAReturnTransaction(createReturnDto.InvoiceNo, _logger, _tranCloudOptions, transactionAuthCode, transactionAcqRefData);
                    break;
            }

            paymentResponse = returnTransaction.PaymentResponse;

            if (paymentType == PaymentType.Card || paymentType == PaymentType.Manual) await ResetPinpad();

            if (paymentResponse.PaymentStatus != PaymentStatus.None)
            {
                _logger.LogInformation(201, $"Return  processed for invoice: {createReturnDto.InvoiceNo}");

                transaction.Amount = paymentResponse.Authorize;
                transaction.CardType = paymentResponse.CardType;
                _unitOfWork.SalesReturnRepository.Add(salesMaster, customer, transaction,
                    salesReturns, paymentResponse.PaymentStatus.Humanize(), paymentResponse.Authorize, loyaltyAmount);

                var returnResponse = new ReturnResponse()
                {
                    AmountAttended = salesMaster.ReturnedAmount ?? 0,
                    Authorize = paymentResponse.Authorize,
                    InvoiceTotal = salesMaster.GrandTotal
                };

                if (paymentResponse.PaymentStatus == PaymentStatus.PartiallyReturned)
                    return Ok(returnResponse);
                else
                    return View("InvoiceNew", returnResponse);
            }
            else
            {
                await ResetPinpad();

                _logger.LogError($"Return was unsuccessful due to {paymentResponse.DisplayMessage}.");
                return BadRequest($"Return was unsuccessful due to {paymentResponse.DisplayMessage}.");
            }
        }

        private InvoiceDTO CreateInvoice(CreateReturnDto createReturnDto)
        {
            var invoiceDTO = new InvoiceDTO();
            invoiceDTO.InvoiceNo = createReturnDto.InvoiceNo;
            invoiceDTO.InvoiceItemList = new List<InvoiceItemDTO>();
            return invoiceDTO;
        }
    }
}