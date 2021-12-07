using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;
using POSSystems.Core.Dtos.Product;
using POSSystems.Web.Infrastructure.Services;
using POSSystems.Web.Infrastructure.TranCloud;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSSystems.Web.Controllers
{
    [Route("")]
    //[TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Dashboard })]
    public class DashboardController : BaseController<DashboardController>
    {
        private readonly TranCloudConfig _tranCloudOptions;
        private readonly ApplicationData _applicationData;

        public DashboardController(IUnitOfWork unitOfWork,
            IOptions<TranCloudConfig> tranCloudOptionsAccessor,
            ILogger<DashboardController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper,
            ApplicationData applicationData)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
            _tranCloudOptions = tranCloudOptionsAccessor.Value;
            _applicationData = applicationData;
        }

        [HttpGet("api/reorderpending", Name = "GetReorderPending")]
        public IActionResult GetReorderPending()
        {
            var reorderPendingProducts = _unitOfWork.ProductRepository.GetReorderPendingProducts();

            var reorderPendingProductDtos = _mapper.Map<List<ReorderPendingProductDto>>(reorderPendingProducts);
            Response.Headers.Add("X-Total-Count", reorderPendingProductDtos.Count.ToString());

            return Ok(reorderPendingProductDtos);
        }

        [HttpGet("api/paramdownload", Name = "GetParamDownload")]
        public async Task<IActionResult> GetParamDownload()
        {
            var paramDownloadjson = TranCloudParamDownload.GetTranCloudParamDownload(_tranCloudOptions);
            var sresponse = await TranCloudWebRequest.DoTranCloudRequest(paramDownloadjson, _tranCloudOptions, _logger);
            _logger.LogInformation(sresponse);
            var paramDownloadResponse = ParamDownloadResponse.MapTranCloudResponse(sresponse);

            return Ok();
        }

        [HttpGet("api/deliverypending", Name = "GetDeliveryPending")]
        public IActionResult GetDeliveryPending()
        {
            var deliveryPendingPurchases = _unitOfWork.PurchaseMasterRepository.GetDeliveryPendingPurchases();

            var deliveryPendingPurchaseDtos = _mapper.Map<List<DeliveryPendingProductDto>>(deliveryPendingPurchases);
            Response.Headers.Add("X-Total-Count", deliveryPendingPurchaseDtos.Count.ToString());

            return Ok(deliveryPendingPurchaseDtos);
        }

        [HttpGet("api/jobsrunning", Name = "GetJobsRunning")]
        public IActionResult GetJobsRunning()
        {
            return Ok(_applicationData.IsJobRunning);
        }
    }
}