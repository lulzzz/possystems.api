using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.MeasurementUnit;
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
    [Route("api/measurementunits")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Product })]
    public class MeasurementUnitController : BaseController<MeasurementUnitController>
    {
        public MeasurementUnitController(IUnitOfWork unitOfWork,
            ILogger<MeasurementUnitController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetMeasurementUnits")]
        public IActionResult GetMeasurementUnits(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<MeasurementUnitDto, MeasurementUnit>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var measurementUnitsBeforePaging = _unitOfWork.MeasurementUnitRepository
                .GetAllDeferred(ep => (pageResourceParams.Q == null || ep.MeasurementName.StartsWith(pageResourceParams.Q)))
                .Where(s => s.Status == pageResourceParams.Status)
                .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<MeasurementUnitDto, MeasurementUnit>());

            var measurementUnitsWithPaging = PagedList<MeasurementUnit>.Create(measurementUnitsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(measurementUnitsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"measurementunits:{paginationMetadata.Response}");

            var measurementUnitDtos = _mapper.Map<List<MeasurementUnitDto>>(measurementUnitsWithPaging);

            return Ok(measurementUnitDtos);
        }

        [HttpGet("{id}", Name = "GetMeasurementUnit")]
        public IActionResult GetMeasurementUnit(int id)
        {
            var measurementUnit = _unitOfWork.MeasurementUnitRepository.Get(id);

            if (measurementUnit == null)
                return NotFound();

            var measurementUnitDto = _mapper.Map<MeasurementUnitDto>(measurementUnit);

            return Ok(measurementUnitDto);
        }

        [HttpPost]
        public IActionResult CreateMeasurementUnit([FromBody] CreateMeasurementUnitDto createMeasurementUnitDto)
        {
            if (createMeasurementUnitDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var measurementUnit = _mapper.Map<MeasurementUnit>(createMeasurementUnitDto);

            _unitOfWork.MeasurementUnitRepository.Add(measurementUnit);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating measurement unit {createMeasurementUnitDto.MeasurementName} failed on saving.");

            var measurementUnitDto = _mapper.Map<MeasurementUnitDto>(measurementUnit);

            return CreatedAtRoute("GetMeasurementUnit", new { id = measurementUnitDto.Id }, measurementUnitDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMeasurementUnit(int id, [FromBody] UpdateMeasurementUnitDto updateMeasurementUnitDto)
        {
            if (updateMeasurementUnitDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var measurementUnit = _unitOfWork.MeasurementUnitRepository.Get(id);
            if (measurementUnit == null)
            {
                return NotFound();
            }

            _mapper.Map(updateMeasurementUnitDto, measurementUnit);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating measurement unit {updateMeasurementUnitDto.MeasurementName} failed on saving.");

            return Ok(updateMeasurementUnitDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMeasurementUnit(int id)
        {
            var measurementUnit = _unitOfWork.MeasurementUnitRepository.Get(id);
            if (measurementUnit == null)
            {
                return NotFound();
            }

            //_unitOfWork.MeasurementUnitRepository.Remove(MeasurementUnit);
            measurementUnit.Status = Statuses.Inactive.Humanize();
            //MapEntity(measurementUnit, measurementUnit);

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting measurement unit {measurementUnit.MeasurementName} failed on saving.");

            _logger.LogInformation(100, $"Measurement unit {measurementUnit.MeasurementId}-{measurementUnit.MeasurementName}  was deleted.");

            return CreatedAtRoute("GetMeasurementUnits", new MasterPageResourceParams());
        }
    }
}