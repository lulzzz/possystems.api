using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.SalesDetail;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System.Collections.Generic;

namespace POSSystems.Web.Controllers
{
    [Route("")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.SalesHistory })]
    public class SalesDetailController : BaseController<SalesDetailController>
    {
        public SalesDetailController(IUnitOfWork unitOfWork,
            ILogger<SalesDetailController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet("api/salesdetails", Name = "GetSalesDetails")]
        public IActionResult GetSalesDetails(GridPageResourceParams pageResourceParams)
        {
            if (!_unitOfWork.SalesMasterRepository.Exists(p => p.SalesId == pageResourceParams.Id))
            {
                return NotFound();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<SalesDetailDto, SalesDetail>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var salesDetailsBeforePaging = _unitOfWork.SalesDetailRepository
                .GetAllDeferred(p => p.SalesId == pageResourceParams.Id)
                .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<SalesDetailDto, SalesDetail>());

            var salesDetailsWithPaging = PagedList<SalesDetail>.Create(salesDetailsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(salesDetailsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"salesdetails:{paginationMetadata.Response}");

            var salesDetailDtos = _mapper.Map<List<SalesDetailDto>>(salesDetailsWithPaging);

            return Ok(salesDetailDtos);
        }
    }
}