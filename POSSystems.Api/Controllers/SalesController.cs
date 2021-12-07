using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neodynamic.SDK.Web;
using Newtonsoft.Json;
using POSSystems.Core;
using POSSystems.Core.Dtos;
using POSSystems.Core.Dtos.Customer;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Core.Exceptions;
using POSSystems.Core.Models;
using POSSystems.Helpers;
using POSSystems.Infrastructure;
using POSSystems.Web.Infrastructure;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Jobs;
using POSSystems.Web.Infrastructure.Plugins.DataProviders;
using POSSystems.Web.Infrastructure.Services;
using POSSystems.Web.Infrastructure.TranCloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POSSystems.Web.Controllers
{
    public static class CacheKeys
    {
        public static string RelationEntry => "_RelationEntry";
        public static string VerificationEntry => "_VerificationEntry";
    }

    public class SalesController : BaseController<SalesController>
    {
        private readonly JobConfig _jobConfig;
        private readonly TranCloudConfig _tranCloudConfig;
        private readonly PrintingInfo _printingInfo;
        private readonly ApplicationData _applicationData;
        private readonly DeploymentDto _deployment;
        public IConfigurationRoot Configuration { get; set; }
        private readonly IWebHostEnvironment _env;

        private readonly bool _trancloudEnabled;
        private readonly double _taxPercentage;
        private readonly bool _vssIntegrated;
        private readonly bool _loyaltyEnabled;
        private readonly bool _rxSignatureNeeded;
        private readonly bool _creditCardLikeCash;
        private readonly bool _printOnlyRx;
        private readonly int _printCopy;
        private readonly int _redeemThresholdPoint;

        private Customer _customer;
        private readonly IMemoryCache _cache;

        private readonly IDataProvider _dataProvider;

        private readonly TranCloudResetHandler _tranCloudResetHandler;

        public SalesController(IUnitOfWork unitOfWork,
            ILogger<SalesController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IOptions<TranCloudConfig> tranCloudOptionsAccessor,
            IOptions<PrintingInfo> printingInfoAccessor,
            IOptions<DeploymentDto> deploymentDto,
            ApplicationData applicationData,
            IWebHostEnvironment env,
            IMemoryCache memoryCache,
            IMapper mapper,
            IOptions<JobConfig> jobConfig,
            IDataProvider dataProvider)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
            _tranCloudConfig = tranCloudOptionsAccessor?.Value;
            _printingInfo = printingInfoAccessor?.Value;
            _env = env;
            _applicationData = applicationData;
            _deployment = deploymentDto?.Value;
            _cache = memoryCache;
            _jobConfig = jobConfig?.Value;
            _dataProvider = dataProvider;

            _trancloudEnabled = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("trancloudEnabled", "True"));
            _taxPercentage = double.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("taxPercentage", "0"));
            _vssIntegrated = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("vssIntegrated", "True"));
            _loyaltyEnabled = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("loyaltyEnabled", "False"));
            _creditCardLikeCash = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("creditcardLikeCash", "False"));
            _printOnlyRx = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("printOnlyRx", "False"));
            _printCopy = int.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("printCopy", "2"));

            if (_trancloudEnabled)
            {
                _rxSignatureNeeded = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("rxSignatureNeeded", "True"));
            }

            if (_loyaltyEnabled)
            {
                _redeemThresholdPoint = int.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("redeemThresholdPoint", "0"));
            }

            _tranCloudResetHandler = new TranCloudResetHandler(_trancloudEnabled, _logger, _tranCloudConfig);
        }

        //[TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Sales })]
        public IActionResult Index()
        {
            IEnumerable<PickerRelationDto> relationList = null;
            IEnumerable<PatientIdTypeDto> verficationIdTypeList = null;

            if (_vssIntegrated)
            {
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromHours(4));

                if (!_cache.TryGetValue(CacheKeys.RelationEntry, out relationList))
                {
                    // Key not in cache, so get data.
                    relationList = _dataProvider.GetRelationList().Result;

                    // Save data in cache.
                    _cache.Set(CacheKeys.RelationEntry, relationList, cacheEntryOptions);
                }

                if (!_cache.TryGetValue(CacheKeys.VerificationEntry, out verficationIdTypeList))
                {
                    // Key not in cache, so get data.
                    verficationIdTypeList = _dataProvider.GetVerificationIdTypeList().Result;

                    // Save data in cache.
                    _cache.Set(CacheKeys.VerificationEntry, verficationIdTypeList, cacheEntryOptions);
                }
            }

            ViewData["WCPScript"] = WebClientPrint.CreateScript(Url.Action("ProcessRequest", "WebClientPrintAPI", null,
                Url.ActionContext.HttpContext.Request.Scheme), Url.Action("PrintReceipt", "Sales",
                new CreateSalesDto(), Url.ActionContext.HttpContext.Request.Scheme), Url.ActionContext.HttpContext.Session.Id);

            ViewData["Version"] = _applicationData.Version;

            var overrideAuthorized = HttpContext.Session.GetString("OverrideAuthorized");

            return View(new SalesDto
            {
                EnableLoyalty = _loyaltyEnabled,
                TrancloudEnabled = _trancloudEnabled,
                CreditCardLikeCash = _creditCardLikeCash,
                TaxPercentage = _taxPercentage,
                VssIntegrated = _vssIntegrated,
                PickerRelations = relationList,
                PatientIdTypes = verficationIdTypeList,
                VerifyMode = _deployment.VerifyMode == "On",
                OverrideAuthorized = overrideAuthorized == "True"
            });
        }

        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Sale })]
        public IActionResult Search(string batch)
        {
            try
            {
                var salesItemDto = new SalesItemDto();
                var followMarkup = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("followMarkup"));

                var product = _unitOfWork.ProductRepository.GetReadOnly(pd => pd.UpcScanCode == batch);
                if (product != null)
                {
                    var filters = new List<string> { "RX", "C2", "C3", "C4", "C5" };

                    if (filters.Any(product.Category.CategoryName.Equals)) throw new POSException($"Item {product.ProductName} not found.");

                    if (product.Quantity <= 0) throw new POSException($"Item {product.ProductName} out of stock.");

                    if (product.PurchasePrice.HasValue && product.SalesPrice <= product.PurchasePrice)
                    {
                        if (followMarkup && product.ProductPriceRangeId != null)
                        {
                            product.SalesPrice = _unitOfWork.PriceRangeRepository.GetCalculatedPrice(product, product.SalesPrice, product.PurchasePrice.Value);
                            if (!_unitOfWork.Save())
                                throw new Exception($"Updating product detail {product.ProductId} failed on saving in sales.");
                        }
                        else if (followMarkup && product.ProductPriceRangeId != null)
                            throw new POSException($"Please choose price range for the product.");
                    }

                    var productDto = _mapper.Map<ProductDto>(product);

                    if (_jobConfig.Edi832?.DehumanizeTo<JobConfigType>() != JobConfigType.Off)
                        if (_unitOfWork.EligibleProductRepository.Exists(product.UpcCode))
                            productDto.IsFSA = true;

                    salesItemDto.ProductDto = productDto;
                    salesItemDto.ProductDto.Category = product.Category.CategoryName;

                    double taxAmount = productDto.TaxIndicator ? Math.Round((salesItemDto.ProductDto.SalesPrice * _taxPercentage) / 100, 2, MidpointRounding.AwayFromZero) : 0;

                    salesItemDto.ProductDto.TaxAmount = taxAmount;
                    salesItemDto.IsRx = false;

                    return Ok(salesItemDto);
                }
                else if (_vssIntegrated)
                {
                    var batchRx = _dataProvider.GetRxInfo(batch).Result;

                    if (batchRx?.FamilyList == null || !batchRx.FamilyList.Any())
                        throw new POSException("No Rx Found in the batch.");

                    var rxListFromBatch = new List<RxItemDto>();
                    foreach (var familyList in batchRx.FamilyList)
                        foreach (var rx in familyList.RxList)
                        {
                            rx.IsFSA = true;
                            rxListFromBatch.Add(rx);
                        }

                    HttpContext.Session.SetString("rxListFromBatch", JsonConvert.SerializeObject(rxListFromBatch));
                    salesItemDto.RxBatchDto = batchRx;
                    salesItemDto.IsRx = true;

                    return Ok(salesItemDto);
                }
                else
                    throw new POSException("Item Not found.");
            }
            catch (POSException pex)
            {
                _logger.LogError($"Barcode:{batch} message:{pex.Message}");
                return BadRequest(pex.UserMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Barcode:{batch} message:{ex.Message}");
                return BadRequest("Not Found");
            }
        }

        [HttpPost]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Sale })]
        public async Task<IActionResult> AcceptPayment(CreateSalesDto createSalesDto)
        {
            if (createSalesDto == null) throw new ArgumentException("Invalid submission of payment.");
            var paymentType = createSalesDto.PaymentType.DehumanizeTo<PaymentType>();

            var ipAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4();
            _logger.LogInformation($"AcceptPayment from Ip: {ipAddress}");

            var transcloudTerminal = _unitOfWork.PosTerminalRepository.GetAllDeferred(s => s.IpAddress == ipAddress.ToString()).FirstOrDefault();
            if (transcloudTerminal != null)
            {
                _tranCloudConfig.RefNo = transcloudTerminal.RefNo;
                _tranCloudConfig.PinPadIpPort = transcloudTerminal.PinpadIpPort;
                _tranCloudConfig.PinPadMACAddress = transcloudTerminal.PinpadMacAddress;
                _tranCloudConfig.ComPort = transcloudTerminal.ComPort;
            }
            else
            {
                _logger.LogInformation($"AcceptPayment from Ip: {ipAddress}. The terminal is not configured");
                throw new POSException("Invalid Terminal, Please configure the terminal.");
            }

            try
            {
                var salesMaster = !string.IsNullOrEmpty(createSalesDto.InvoiceNo) ? _unitOfWork.SalesMasterRepository.GetByInvoiceNo(createSalesDto.InvoiceNo) : null;

                if (salesMaster != null && salesMaster.IsSignatureRequired())
                {
                    var paymentResponse = new PaymentInfoResponse { InvoiceNo = createSalesDto.InvoiceNo };
                    await ProcessSignature(createSalesDto, salesMaster, paymentResponse, _rxSignatureNeeded, false);

                    if (paymentResponse.PaymentStatus != PaymentStatus.Signed)
                        return Ok(paymentResponse);
                    else
                    {
                        await _tranCloudResetHandler.PerformAfterTransactionReset(paymentType);
                        return View("InvoiceNew", paymentResponse);
                    }
                }
                else
                {
                    if (_loyaltyEnabled && !string.IsNullOrEmpty(createSalesDto.LoyaltyCard))
                    {
                        _customer = _unitOfWork.CustomerRepository.GetByLoyalCardNumber(createSalesDto.LoyaltyCard);
                        if (_customer == null)
                        {
                            throw new POSException("Loyalty card not found", $"The loyalty card {createSalesDto.LoyaltyCard} was not found.");
                        }
                    }

                    var products = _unitOfWork.ProductRepository.GetAll(s => createSalesDto.PosItems != null && createSalesDto.PosItems.Select(d => d.Id).Contains(s.ProductId)).ToList();
                    var rxListFromBatchStr = HttpContext.Session.GetString("rxListFromBatch");
                    var rxListFromBatch = rxListFromBatchStr == null ? default : JsonConvert.DeserializeObject<List<RxItemDto>>(rxListFromBatchStr);

                    Calculator calc = new Calculator(
                        _customer,
                        products,
                        rxListFromBatch,
                        _taxPercentage,
                        _redeemThresholdPoint
                    ).CalculateTotal(createSalesDto).CalculatePayment(salesMaster);

                    salesMaster = salesMaster ?? new SalesMaster
                    {
                        SalesDate = DateTime.Today,
                        GrandTotal = calc.InvoicedTotal,
                        InvoiceNo = GenerateToBeUsedInvoiceNo(),
                        SalesTax = calc.TaxTotal,
                        TotalDiscount = calc.DiscountTotal,
                        DiscountPercentage = createSalesDto.DiscountPercentage,
                        ContainsRx = createSalesDto.RxList != null && createSalesDto.RxList.Any(),
                        PointsEarned = calc.PointsEarned,
                        PointsRedeemed = calc.PointsRedeemed,
                        InvoiceRedeemAmount = calc.RedeemableAmount,
                        LoyaltyCard = createSalesDto.LoyaltyCard,
                        PointDollarConversionRatio = _customer?.PointDollarConversionRatio,
                        TerminalId = transcloudTerminal?.TerminalId,
                        PaymentStatus = PaymentStatus.None.Humanize()
                    };

                    POSTransaction posTransaction = null;

                    if (salesMaster.PaymentStatus.DehumanizeTo<PaymentStatus>() != PaymentStatus.Complete)
                    {
                        switch (paymentType)
                        {
                            case PaymentType.FSA:
                                posTransaction = new TranCloudFSATransaction(salesMaster.InvoiceNo, _logger, _tranCloudConfig, createSalesDto.FSATotal);
                                break;

                            case PaymentType.Card:
                                if (!_creditCardLikeCash)
                                    posTransaction = new TranCloudCardTransaction(salesMaster.InvoiceNo, _logger, _tranCloudConfig);
                                else
                                    posTransaction = new POSBasicTransaction(salesMaster.InvoiceNo);
                                break;

                            case PaymentType.Manual:
                                posTransaction = new TranCloudManualTransaction(salesMaster.InvoiceNo, _logger, _tranCloudConfig);
                                break;

                            case PaymentType.Manualfsa:
                                posTransaction = new TranCloudManualFSATransaction(salesMaster.InvoiceNo, _logger, _tranCloudConfig);
                                break;

                            default:
                                posTransaction = new POSBasicTransaction(salesMaster.InvoiceNo);
                                break;
                        }
                    }

                    var paymentResponse = await posTransaction.ProcessPayment(createSalesDto.PaidTotal, calc.ToBeProcessedTotal);
                    if (paymentResponse.PaymentStatus == PaymentStatus.None)
                    {
                        throw new POSException("Payment was unsuccessful.", $"Payment was unsuccessful due to {paymentResponse.DisplayMessage}");
                    }

                    _logger.LogInformation($"Payment processed for invoice no: {salesMaster.InvoiceNo}");
                    salesMaster.Payment += paymentResponse.Authorize;

                    var invoiceDTO = salesMaster.InvoiceReceipt == null ?
                        CreateInvoice(salesMaster.InvoiceNo, calc) : JsonConvert.DeserializeObject<InvoiceDTO>(salesMaster.InvoiceReceipt);
                    invoiceDTO.Masked_Account = invoiceDTO.Masked_Account ?? paymentResponse.MaskedAccount;
                    invoiceDTO.PaymentType += ", " + paymentType.ToString();
                    invoiceDTO.InvoicePaymentList.Add(new InvoicePaymentDTO() { PaymentType = paymentType.ToString(), PaidTotal = paymentResponse.Authorize });

                    var transaction = new Transaction
                    {
                        TransactionType = TransactionType.Sales.Humanize(),
                        PayMethod = paymentType == PaymentType.Manual ? PaymentType.Card.Humanize() : paymentType.Humanize(),
                        Amount = paymentResponse.Authorize,
                        CheckNo = createSalesDto.CheckNo,
                        MaskedAcct = paymentResponse.MaskedAccount,
                        Token = paymentResponse.Token,
                        AuthCode = paymentResponse.AuthCode,
                        AcqRefData = paymentResponse.AcqRefData,
                        CardType = paymentResponse.CardType,
                        Back = salesMaster.Due < 0 ? salesMaster.Due : 0
                    };

                    salesMaster.SignatureNeeded = paymentResponse.SignatureNeeded || (salesMaster.ContainsRx ?? false) || createSalesDto.SignatureReq;
                    salesMaster.InvoiceReceipt = JsonConvert.SerializeObject(invoiceDTO);
                    salesMaster.PaymentStatus = paymentResponse.PaymentStatus.Humanize();

                    _unitOfWork.SalesMasterRepository.Add(salesMaster, transaction, createSalesDto.PosItems, createSalesDto.RxList, _taxPercentage);

                    if (salesMaster.IsSignatureRequired())
                    {
                        await ProcessSignature(createSalesDto, salesMaster, paymentResponse, _rxSignatureNeeded);
                    }

                    var salesResponse = new SalesResponse()
                    {
                        InvoiceNo = salesMaster.InvoiceNo,
                        InvoiceTotal = calc.InvoicedTotal,
                        AmountAttended = salesMaster.Payment,
                        Authorize = _creditCardLikeCash ? salesMaster.Payment : paymentResponse.Authorize,
                        Due = salesMaster.Due ?? 0,
                        Purchase = paymentResponse.Purchase,
                        PaymentStatus = paymentResponse.PaymentStatus
                    };

                    await _tranCloudResetHandler.PerformAfterTransactionReset(paymentType);

                    if (paymentResponse.PaymentStatus == PaymentStatus.Signed || paymentResponse.PaymentStatus == PaymentStatus.Complete)
                    {
                        return View("InvoiceNew", salesResponse);
                    }
                    else
                    {
                        return Ok(paymentResponse);
                    }
                }
            }
            catch (POSException pox)
            {
                await _tranCloudResetHandler.PerformAfterTransactionReset(paymentType);

                _logger.LogError(pox.Message);
                return BadRequest(pox.UserMessage);
            }
        }

        private async Task ProcessSignature(CreateSalesDto createSalesDto,
            SalesMaster salesMaster,
            PaymentInfoResponse paymentResponse,
            bool rxSignatureNeeded = true,
            bool reset = true)
        {
            if (rxSignatureNeeded)
            {
                if (reset) await _tranCloudResetHandler.ResetPinpad();
                var tranCloudSignHandler = new TranCloudSignHandler(_trancloudEnabled, _logger, _tranCloudConfig);
                var signature = await tranCloudSignHandler.GetSignature();
                if (!string.IsNullOrEmpty(signature))
                {
                    paymentResponse.PaymentStatus = PaymentStatus.Signed;

                    salesMaster.Signature = signature;
                    if (!_unitOfWork.Save())
                    {
                        _logger.LogError($"Failed to save signature in sales id: {salesMaster.SalesId}");
                    }

                    paymentResponse.Signature = salesMaster.Signature;
                    if (salesMaster.ContainsRx ?? false)
                    {
                        var batchPickupPostDataDto = _mapper.Map<BatchPickupPostDataDto>(createSalesDto);
                        batchPickupPostDataDto.SignatureData = signature;
                        await _dataProvider.UpdateRxInfo(batchPickupPostDataDto);
                    }
                }
                else
                    paymentResponse.PaymentStatus = PaymentStatus.NotSigned;
            }
            else
            {
                paymentResponse.PaymentStatus = PaymentStatus.Signed;

                if (salesMaster.ContainsRx ?? false)
                {
                    var batchPickupPostDataDto = _mapper.Map<BatchPickupPostDataDto>(createSalesDto);
                    batchPickupPostDataDto.SignatureData = string.Empty;
                    await _dataProvider.UpdateRxInfo(batchPickupPostDataDto);
                }
            }
        }

        [HttpGet]
        public IEnumerable<PickerAutoCompleteDto> GetPickerLookupList(string term)
        {
            return _dataProvider.GetPickerLookupList(term).Result;
        }

        private InvoiceDTO CreateInvoice(string invoiceNo, Calculator calc)
        {
            var invoiceDTO = new InvoiceDTO
            {
                CompanyName = _printingInfo.CompanyName,
                CompanyAddress = _printingInfo.CompanyAddress,
                CompanyAddress2 = _printingInfo.CompanyAddress2,
                CompanyEmail = _printingInfo.CompanyEmail,
                CompanyPhone = _printingInfo.CompanyPhone,
                CompanyWebsite = _printingInfo.CompanyWebsite,
                InvoiceNo = invoiceNo,
                InvoiceItemList = new List<InvoiceItemDTO>(),
                InvoicePaymentList = new List<InvoicePaymentDTO>(),
                Subtotal = calc.Subtotal,
                TotalTax = calc.TaxTotal,
                TotalPrice = calc.InvoicedTotal,
                Discount = calc.DiscountTotal,
                LoyaltyPointEarned = _customer?.LoyaltyPointEarned.ToString()
            };

            return invoiceDTO;
        }

        private string GenerateToBeUsedInvoiceNo()
        {
            var lastInvoiceNo = _unitOfWork.SalesMasterRepository.GetLastInvoiceNo() ?? 0;
            return (lastInvoiceNo + 1).ToString().PadLeft(7, '0');
        }

        [HttpPost]
        public IActionResult GetLoyalty(CreateSalesDto createSalesDto)
        {
            try
            {
                _customer = _unitOfWork.CustomerRepository.GetByLoyalCardNumber(createSalesDto?.LoyaltyCard);
                if (_customer != null)
                {
                    var products = _unitOfWork.ProductRepository.GetAll(s => createSalesDto.PosItems != null && createSalesDto.PosItems.Select(d => d.Id).Contains(s.ProductId)).ToList();
                    var rxListFromBatchStr = HttpContext.Session.GetString("rxListFromBatch");
                    var rxListFromBatch = rxListFromBatchStr == null ? default : JsonConvert.DeserializeObject<List<RxItemDto>>(rxListFromBatchStr);

                    var calc = new Calculator(
                        _customer,
                        products,
                        rxListFromBatch,
                        _taxPercentage,
                        _redeemThresholdPoint
                    ).CalculateTotal(createSalesDto);

                    var customerDto = _mapper.Map<CustomerDto>(_customer);
                    customerDto.RedeemableAmount = calc.RedeemableAmount;
                    return Ok(customerDto.RedeemableAmount);
                }
                else
                    throw new POSException("Loyalty card not found", $"The loyalty card {createSalesDto.LoyaltyCard} was not found.");
            }
            catch (POSException ex)
            {
                _logger.LogError($"Loyaltycard:{createSalesDto.LoyaltyCard} message:{ex.Message}");
                return BadRequest(ex.UserMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Loyaltycard:{createSalesDto.LoyaltyCard} message:{ex.Message}");
                return BadRequest("Internal error needs fixing.");
            }
        }

        [HttpGet]
        public IActionResult AuthorizeOverride(string username, string password)
        {
            try
            {
                var userInfo = _unitOfWork.UserRepository.SingleOrDefault(x => x.UserName == username);
                if (userInfo != null)
                {
                    var matched = SecurityHelper.VerifyHashedPassword(userInfo.Password, password);
                    if (matched)
                    {
                        var userRole = _unitOfWork.UserRoleRepository.SingleOrDefault(x => x.UserId == userInfo.UserId);
                        if (userRole != null)
                        {
                            var roleClaims = _unitOfWork.RoleClaimRepository.SingleOrDefault(x => x.RoleId == userRole.RoleId && x.ClaimValue == Permission.Override.Humanize());
                            if (roleClaims == null)
                            {
                                throw new POSException("Not Authorized for overriding.");
                            }
                            else
                                return Ok("Authorized");
                        }
                        else
                            throw new POSException("User not assigned to any role.");
                    }
                    else
                        throw new POSException("Password didnt match.");
                }
                else
                    throw new POSException("User not found.");
            }
            catch (POSException pex)
            {
                _logger.LogError($"Overrriding price or quantity ended with error.{pex.UserMessage}", pex.Message);
                return BadRequest(pex.UserMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Overrriding price or quantity ended with error.{ex.Message}");
                return BadRequest("Overrriding price or quantity ended with error.");
            }
        }
    }
}