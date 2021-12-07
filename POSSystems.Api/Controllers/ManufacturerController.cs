using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Manufacturer;
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
    [Route("api/manufacturers")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Product })]
    public class ManufacturerController : BaseController<ManufacturerController>
    {
        public ManufacturerController(IUnitOfWork unitOfWork,
            ILogger<ManufacturerController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetManufacturers")]
        public IActionResult GetManufacturers(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<ManufacturerDto, Manufacturer>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var manufacturersBeforePaging = _unitOfWork.ManufacturerRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<ManufacturerDto, Manufacturer>())
                .Where(s => s.Status == pageResourceParams.Status);

            var manufacturersWithPaging = PagedList<Manufacturer>.Create(manufacturersBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(manufacturersWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"manufacturers:{paginationMetadata.Response}");

            var manufacturerDtos = _mapper.Map<List<ManufacturerDto>>(manufacturersWithPaging);

            return Ok(manufacturerDtos);
        }

        [HttpGet("{id}", Name = "GetManufacturer")]
        public IActionResult GetManufacturer(int id)
        {
            var manufacturer = _unitOfWork.ManufacturerRepository.Get(id);

            if (manufacturer == null)
                return NotFound();

            var manufacturerDto = _mapper.Map<ManufacturerDto>(manufacturer);

            return Ok(manufacturerDto);
        }

        [HttpPost]
        public IActionResult CreateManufacturer([FromBody] CreateManufacturerDto createManufacturerDto)
        {
            if (createManufacturerDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var manufacturer = _mapper.Map<Manufacturer>(createManufacturerDto);

            _unitOfWork.ManufacturerRepository.Add(manufacturer);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating manufacturer {createManufacturerDto.Name} failed on saving.");

            var manufacturerDto = _mapper.Map<ManufacturerDto>(manufacturer);

            return CreatedAtRoute("GetManufacturer", new { id = manufacturerDto.Id }, manufacturerDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateManufacturer(int id, [FromBody] UpdateManufacturerDto updateManufacturerDto)
        {
            if (updateManufacturerDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var manufacturer = _unitOfWork.ManufacturerRepository.Get(id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            _mapper.Map(updateManufacturerDto, manufacturer);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating manufacturer {updateManufacturerDto.Name} failed on saving.");

            return Ok(updateManufacturerDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteManufacturer(int id)
        {
            var manufacturer = _unitOfWork.ManufacturerRepository.Get(id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            //_unitOfWork.ManufacturerRepository.Remove(Manufacturer);
            manufacturer.Status = Statuses.Inactive.Humanize();
            //MapEntity(manufacturer, manufacturer);

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting manufacturer - {manufacturer.Name} failed on saving.");

            _logger.LogInformation(100, $"Manufacturer - {manufacturer.ManufacturerId}-{manufacturer.Name}  was deleted.");

            return CreatedAtRoute("GetManufacturers", new MasterPageResourceParams());
        }
    }
}