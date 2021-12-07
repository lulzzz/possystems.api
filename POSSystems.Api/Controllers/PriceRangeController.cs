using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PriceRange;
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
    [Route("api/priceranges")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Config })]
    public class PriceRangesController : BaseController<PriceRangesController>
    {
        public PriceRangesController(IUnitOfWork unitOfWork,
            ILogger<PriceRangesController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetPriceRanges")]
        public IActionResult GetPriceRanges(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<PriceRangeDto, PriceRange>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var PriceRangesBeforePaging = _unitOfWork.PriceRangeRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<PriceRangeDto, PriceRange>())
                .Where(s => s.Status == pageResourceParams.Status);

            var priceRangesWithPaging = PagedList<PriceRange>.Create(PriceRangesBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(priceRangesWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"priceranges:{paginationMetadata.Response}");

            var PriceRangeDtos = _mapper.Map<List<PriceRangeDto>>(priceRangesWithPaging);

            return Ok(PriceRangeDtos);
        }

        [HttpGet("{id}", Name = "GetPriceRange")]
        public IActionResult GetPriceRange(int id)
        {
            var PriceRange = _unitOfWork.PriceRangeRepository.Get(id);

            if (PriceRange == null)
                return NotFound();

            var productCategorieDto = _mapper.Map<PriceRangeDto>(PriceRange);

            return Ok(productCategorieDto);
        }

        [HttpPost]
        public IActionResult CreatePriceRange([FromBody] CreatePriceRangeDto createPriceRangeDto)
        {
            if (createPriceRangeDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var PriceRange = _mapper.Map<PriceRange>(createPriceRangeDto);

            _unitOfWork.PriceRangeRepository.Add(PriceRange);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating PriceRange failed on saving.");

            var PriceRangeDto = _mapper.Map<PriceRangeDto>(PriceRange);

            return CreatedAtRoute("GetPriceRange", new { id = PriceRangeDto.Id }, PriceRangeDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePriceRange(int id, [FromBody] UpdatePriceRangeDto updatePriceRangeDto)
        {
            if (updatePriceRangeDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var PriceRange = _unitOfWork.PriceRangeRepository.Get(id);
            if (PriceRange == null)
            {
                return NotFound();
            }

            _mapper.Map(updatePriceRangeDto, PriceRange);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating PriceRange   failed on saving.");

            return Ok(updatePriceRangeDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePriceRange(int id)
        {
            var PriceRange = _unitOfWork.PriceRangeRepository.Get(id);
            if (PriceRange == null)
            {
                return NotFound();
            }

            PriceRange.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting PriceRange failed on saving.");

            _logger.LogInformation(100, $"PriceRange {PriceRange.PriceRangeId}  was deleted.");

            return CreatedAtRoute("GetPriceRanges", new MasterPageResourceParams());
        }
    }
}