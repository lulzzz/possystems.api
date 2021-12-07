using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos;
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
    [Route("api/productpriceranges")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Product })]
    public class ProductPriceRangeController : BaseController<ProductPriceRangeController>
    {
        public ProductPriceRangeController(IUnitOfWork unitOfWork,
            ILogger<ProductPriceRangeController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetProductPriceRanges")]
        public IActionResult GetProductPriceRanges(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<ProductPriceRangeDto, ProductPriceRange>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var productPriceRangesBeforePaging = _unitOfWork.ProductPriceRangeRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<ProductPriceRangeDto, ProductPriceRange>())
                .Where(s => s.Status == pageResourceParams.Status);

            var productPriceRangesWithPaging = PagedList<ProductPriceRange>.Create(productPriceRangesBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(productPriceRangesWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"productpriceranges:{paginationMetadata.Response}");

            var productPriceRangeDtos = _mapper.Map<List<ProductPriceRangeDto>>(productPriceRangesWithPaging);

            return Ok(productPriceRangeDtos);
        }

        [HttpGet("{id}", Name = "GetProductPriceRange")]
        public IActionResult GetProductPriceRange(int id)
        {
            var productPriceRange = _unitOfWork.ProductPriceRangeRepository.Get(id);

            if (productPriceRange == null)
                return NotFound();

            var productPriceRangeDto = _mapper.Map<GetProductPriceRangeDto>(productPriceRange);

            return Ok(productPriceRangeDto);
        }

        [HttpPost]
        public IActionResult CreateProductPriceRange([FromBody] CreateProductPriceRangeDto createProductPriceRangeDto)
        {
            if (createProductPriceRangeDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var productPriceRange = _mapper.Map<ProductPriceRange>(createProductPriceRangeDto);

            _unitOfWork.ProductPriceRangeRepository.Add(productPriceRange);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating ProductPriceRange failed on saving.");

            var productPriceRangeDto = _mapper.Map<ProductPriceRangeDto>(productPriceRange);

            return CreatedAtRoute("GetProductPriceRange", new { id = productPriceRangeDto.Id }, productPriceRangeDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProductPriceRange(int id, [FromBody] UpdateProductPriceRangeDto updateProductPriceRangeDto)
        {
            if (updateProductPriceRangeDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var productPriceRange = _unitOfWork.ProductPriceRangeRepository.Get(id);
            if (productPriceRange == null)
            {
                return NotFound();
            }

            _mapper.Map(updateProductPriceRangeDto, productPriceRange);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating ProductPriceRange   failed on saving.");

            return Ok(updateProductPriceRangeDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProductPriceRange(int id)
        {
            var productPriceRange = _unitOfWork.ProductPriceRangeRepository.Get(id);
            if (productPriceRange == null)
            {
                return NotFound();
            }

            productPriceRange.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting ProductPriceRange failed on saving.");

            _logger.LogInformation(100, $"ProductPriceRange {productPriceRange.ProductPriceRangeId}  was deleted.");

            return CreatedAtRoute("GetProductPriceRanges", new MasterPageResourceParams());
        }
    }
}