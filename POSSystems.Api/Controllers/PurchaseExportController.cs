using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Product;
using POSSystems.Core.Dtos.PurchaseMaster;
using POSSystems.Web.Infrastructure.Edi;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Jobs;
using POSSystems.Web.Infrastructure.Services;
using System.Collections.Generic;

namespace POSSystems.Web.Controllers
{
    [Route("api/purchaseexport")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Purchase })]
    public class PurchaseExportController : BaseController<PurchaseExportController>
    {
        private readonly IWebHostEnvironment _env;
        private new ILogger<PurchaseExportController> _logger;

        private readonly ILogger<Edi855Job> _logger55;
        private readonly ILogger<Edi832Job> _logger32;

        public PurchaseExportController(IUnitOfWork unitOfWork,
            ILogger<PurchaseExportController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService, IWebHostEnvironment env,
            ILogger<Edi855Job> logger55,
            ILogger<Edi832Job> logger32)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, null)
        {
            _logger = logger;
            _env = env;

            _logger55 = logger55;
            _logger32 = logger32;
        }

        [HttpGet("{upcCode}", Name = "GetProductDetailsByUpc")]
        public IActionResult GetProductDetails(string upcCode)
        {
            var productDetail = _unitOfWork.ProductRepository.GetByUpc(upcCode);

            if (productDetail == null)
                return NotFound();

            var productPurchaseDetailDto = _mapper.Map<ProductPurchaseDetailDto>(productDetail);
            return Ok(productPurchaseDetailDto);
        }

        [HttpPost("{supplierId}", Name = "PurchaseOrder")]
        public IActionResult PurchaseOrder(int supplierId, [FromBody] List<PurchaseEdi850Model> purchasingItems)
        {
            var edi850Processor = new Edi850Processor(_unitOfWork, _env, _logger);
            edi850Processor.Process(supplierId, purchasingItems);

            return Ok();
        }
    }
}