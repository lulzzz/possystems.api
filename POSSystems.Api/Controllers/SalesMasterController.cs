using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.SalesMaster;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System.Collections.Generic;

namespace POSSystems.Web.Controllers
{
    [Route("api/salesmasters")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.SalesHistory })]
    public class SalesMasterController : BaseController<SalesMasterController>
    {
        public SalesMasterController(IUnitOfWork unitOfWork,
            ILogger<SalesMasterController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }
        
        [HttpGet(Name = "GetSalesMasters")]
        public IActionResult GetSalesMasters(SalesPageResourceParams salesPageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<SalesMasterDto, SalesMaster>
                (salesPageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var salesMastersBeforePaging = _unitOfWork.SalesMasterRepository
                .GetAllDeferred(ep => (salesPageResourceParams.Q == null || ep.InvoiceNo.Contains(salesPageResourceParams.Q)) &&
                (!salesPageResourceParams.SalesDate.HasValue || ep.SalesDate.Value.Date == salesPageResourceParams.SalesDate))
                .ApplySort(salesPageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<SalesMasterDto, SalesMaster>());

            var salesMastersWithPaging = PagedList<SalesMaster>.Create(salesMastersBeforePaging
                , salesPageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(salesMastersWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"salesmasters:{paginationMetadata.Response}");

            var salesMasterDtos = _mapper.Map<List<SalesMasterDto>>(salesMastersWithPaging);

            return Ok(salesMasterDtos);
        }

        [HttpGet("{id}", Name = "GetSalesMaster")]
        public IActionResult GetSalesMaster(int id)
        {
            var salesMaster = _unitOfWork.SalesMasterRepository.Get(id);
            salesMaster.Terminal = salesMaster.TerminalId != null ? _unitOfWork.PosTerminalRepository.Get(salesMaster.TerminalId.Value) : null;
            if (salesMaster == null)
                return NotFound();

            var salesMasterDto = _mapper.Map<SalesMasterDto>(salesMaster);

            return Ok(salesMasterDto);
        }
    }
}