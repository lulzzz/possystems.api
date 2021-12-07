using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neodynamic.SDK.Web;
using POSSystems.Core;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Web.Infrastructure.Services;
using POSSystems.Web.Infrastructure.Token;

namespace POSSystems.Web.Controllers
{
    public class HomeController : BaseController<HomeController>
    {
        private readonly JwtIssuerOptions _jwtIssuerOptions;
        private readonly ApplicationData _applicationData;
        private readonly PrintingInfo _printingInfo;

        public HomeController(IUnitOfWork unitOfWork,
            ILogger<HomeController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IOptions<JwtIssuerOptions> jwtIssuerOptions,
            ApplicationData applicationData,
            IOptions<PrintingInfo> printingInfo)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, null)
        {
            _jwtIssuerOptions = jwtIssuerOptions?.Value;
            _applicationData = applicationData;
            _printingInfo = printingInfo?.Value;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewData["ApiUrl"] = _jwtIssuerOptions.Audience.TrimEnd('/');
            ViewData["WCPScript"] = WebClientPrint.CreateScript(Url.Action("ProcessRequest", "WebClientPrintAPI", null, Url.ActionContext.HttpContext.Request.Scheme),
                Url.Action("PrintReceipt", "Common", new CreatePrintingInvoiceDto(), Url.ActionContext.HttpContext.Request.Scheme), Url.ActionContext.HttpContext.Session.Id);

            ViewData["Version"] = _applicationData.Version;

            ViewData["CompanyName"] = _printingInfo.CompanyName;
            ViewData["CompanyAddress"] = _printingInfo.CompanyAddress;
            ViewData["CompanyAddress2"] = _printingInfo.CompanyAddress2;
            ViewData["CompanyPhone"] = _printingInfo.CompanyPhone;
            ViewData["CompanyEmail"] = _printingInfo.CompanyEmail;
            ViewData["CompanyWebsite"] = _printingInfo.CompanyWebsite;

            return View("Dashboard");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}