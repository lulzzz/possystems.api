using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neodynamic.SDK.Web;
using Newtonsoft.Json;
using POSSystems.Core;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Infrastructure;
using POSSystems.Web.Infrastructure.Services;
using System;

namespace POSSystems.Web.Controllers
{
    [Route("api/common")]
    public class CommonController : BaseController<CommonController>
    {
        private readonly bool _printOnlyRx;
        private readonly int _printCopy;
        private readonly IWebHostEnvironment _env;
        public CommonController(IUnitOfWork unitOfWork,
            ILogger<CommonController> logger,
            IUrlHelper urlHelper,
            IWebHostEnvironment env,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
            _env = env;
            _printOnlyRx = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("printOnlyRx", "False"));
            _printCopy = int.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("printCopy", "2"));
        }

        [HttpGet]
        public IActionResult PrintReceipt(string invoiceNo)
        {
            if (string.IsNullOrEmpty(invoiceNo)) return BadRequest("Invoice no empty not accepted.");

            #region V4 Neodynamic

            WebClientPrint.LicenseOwner = "DAA Enterprises Inc. - 1 WebApp Lic - 1 WebServer Lic";
            WebClientPrint.LicenseKey = "7C2AD722096CF4FBBD7069BE6129C8F0ED809968";

            #endregion V4 Neodynamic

            #region V3 Neodynamic

            //WebClientPrint.LicenseOwner = "DAA Enterprises Inc. - 1 WebApp Lic - 1 WebServer Lic";
            //WebClientPrint.LicenseKey = "DF537DBAC1C8657D7937C79E6BBF448387BB21C1";

            #endregion V3 Neodynamic

            _logger.LogInformation($"PrintReceipt: Get Invoice Information {invoiceNo}.");
            var salesMaster = _unitOfWork.SalesMasterRepository.GetByInvoiceNo(invoiceNo);
            var company = _unitOfWork.CompanyRepository.FirstOrDefault(c => c.Status == Statuses.Active.Humanize());

            var invoiceDTO = JsonConvert.DeserializeObject<InvoiceDTO>(salesMaster?.InvoiceReceipt);

            invoiceDTO.CompanyName = company?.Name;
            invoiceDTO.CompanyAddress = company?.Address;
            invoiceDTO.CompanyAddress2 = company?.Address2;
            invoiceDTO.CompanyEmail = company?.Email;
            invoiceDTO.CompanyPhone = company?.Phone;
            invoiceDTO.CompanyWebsite = company?.Website;
            invoiceDTO.CreatedBy = salesMaster?.CreatedBy;
            invoiceDTO.CreatedDate = salesMaster.CreatedDate;

            try
            {
                var posPrintHelper = new POSPrintHelper(_env, _printOnlyRx);
                posPrintHelper.Notes = company.Notes;

                string cmds = "";
                _logger.LogInformation($"PrintReceipt: Generating receipt for Invoice No-> {invoiceNo}.");
                cmds = posPrintHelper.PrintReceipt(invoiceDTO, 
                    salesMaster.PaymentStatus.DehumanizeTo<PaymentStatus>() == PaymentStatus.CompletelyReturned || 
                    salesMaster.PaymentStatus.DehumanizeTo<PaymentStatus>() == PaymentStatus.PartiallyReturned ? "Sales Return" : "Sales"
                    , _printCopy);

                ClientPrintJob cpj = new ClientPrintJob
                {
                    PrinterCommands = cmds,
                    FormatHexValues = true,
                    ClientPrinter = new DefaultPrinter()
                };

                _logger.LogInformation($"PrintReceipt: Receipt generated for Invoice No-> {invoiceNo}.");
                _logger.LogInformation($"-------------");
                _logger.LogInformation($"{cmds}");
                _logger.LogInformation($"-------------");
                _logger.LogInformation($"Printing for Invoice No-> {invoiceNo}.");
                return File(cpj.GetContent(), "application/octet-stream");
            }
            catch (Exception exe)
            {
                _logger.LogError($"Error:{exe.Message}");
                return BadRequest($"Error Printing for invoice no-> {invoiceNo}");
            }
        }
    }
}
