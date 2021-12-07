using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.SalesReturn;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Web.Controllers
{
    [Route("api/salesreturns")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Return })]

    public class SalesReturnController : BaseController<SalesReturnController>
    {
        public SalesReturnController(IUnitOfWork unitOfWork,
            ILogger<SalesReturnController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetSalesReturns")]
        public IActionResult GetSalesReturns(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<SalesReturnDto, SalesReturn>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var salesReturnsBeforePaging = _unitOfWork.SalesReturnRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<SalesReturnDto, SalesReturn>());

            var salesReturnsWithPaging = PagedList<SalesReturn>.Create(salesReturnsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(salesReturnsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"salesreturns:{paginationMetadata.Response}");

            var salesReturnsDtos = _mapper.Map<List<SalesReturnDto>>(salesReturnsWithPaging);

            return Ok(salesReturnsDtos);
        }

        [HttpGet("{id}", Name = "GetSalesReturn")]
        public IActionResult GetSalesReturn(int id)
        {
            var salesReturn = _unitOfWork.SalesReturnRepository.Get(id);

            if (salesReturn == null)
                return NotFound();

            var salesReturnDto = _mapper.Map<SalesReturnDto>(salesReturn);

            return Ok(salesReturnDto);
        }

        [HttpPost]
        public IActionResult CreateSalesReturn([FromBody] CreateSalesReturnDto createSalesReturnDto)
        {
            if (createSalesReturnDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var salesMaster = _unitOfWork.SalesMasterRepository.GetAllDeferred(sm => sm.InvoiceNo == createSalesReturnDto.InvoiceNo).SingleOrDefault();
            if (salesMaster == null)
                return BadRequest();

            var salesReturn = _mapper.Map<SalesReturn>(createSalesReturnDto);
            salesReturn.SalesId = salesMaster.SalesId;
            _unitOfWork.SalesReturnRepository.Add(salesReturn);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating sales return {createSalesReturnDto.SalesId} failed on saving.");

            var salesReturnDto = _mapper.Map<SalesReturnDto>(salesReturn);

            return CreatedAtRoute("GetSalesReturn", new { id = salesReturnDto.SalesId }, salesReturnDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSalesReturn(int id, [FromBody] UpdateSalesReturnDto updateSalesReturnDto)
        {
            if (updateSalesReturnDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var salesReturn = _unitOfWork.SalesReturnRepository.Get(id);
            if (salesReturn == null)
            {
                return NotFound();
            }

            _mapper.Map(updateSalesReturnDto, salesReturn);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating sales return {updateSalesReturnDto.SalesId} failed on saving.");

            return Ok(updateSalesReturnDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSalesReturn(int id)
        {
            var salesReturn = _unitOfWork.SalesReturnRepository.Get(id);
            if (salesReturn == null)
            {
                return NotFound();
            }

            salesReturn.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting sales return {salesReturn.SalesId} failed on saving.");

            _logger.LogInformation(100, $"Sales return {salesReturn.SalesId}  was deleted.");

            return NoContent();
        }
    }
}