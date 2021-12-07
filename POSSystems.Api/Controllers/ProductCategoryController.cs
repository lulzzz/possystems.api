using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.ProductCategory;
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
    [Route("api/productcategories")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Product })]
    public class ProductCategoryController : BaseController<ProductCategoryController>
    {
        public ProductCategoryController(IUnitOfWork unitOfWork,
            ILogger<ProductCategoryController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetProductCategories")]
        public IActionResult GetProductCategories(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<ProductCategoryDto, ProductCategory>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var productCategoriesBeforePaging = _unitOfWork.ProductCategoryRepository
                .GetAllDeferred(ep => (pageResourceParams.Q == null || ep.CategoryName.StartsWith(pageResourceParams.Q)))
                .Where(s => s.Status == pageResourceParams.Status)
                .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<ProductCategoryDto, ProductCategory>());

            var productCategoriesWithPaging = PagedList<ProductCategory>.Create(productCategoriesBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(productCategoriesWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"productcategories:{paginationMetadata.Response}");

            var productCategoryDtos = _mapper.Map<List<ProductCategoryDto>>(productCategoriesWithPaging);

            return Ok(productCategoryDtos);
        }

        [HttpGet("{id}", Name = "GetProductCategory")]
        public IActionResult GetProductCategory(int id)
        {
            var productCategory = _unitOfWork.ProductCategoryRepository.Get(id);

            if (productCategory == null)
                return NotFound();

            var productCategoriesDto = _mapper.Map<ProductCategoryDto>(productCategory);

            return Ok(productCategoriesDto);
        }

        [HttpPost]
        public IActionResult CreateProductCategory([FromBody] CreateProductCategoryDto createProductCategoryDto)
        {
            if (createProductCategoryDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var productCategory = _mapper.Map<ProductCategory>(createProductCategoryDto);

            _unitOfWork.ProductCategoryRepository.Add(productCategory);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating product category {createProductCategoryDto.CategoryName} failed on saving.");

            var productCategoryDto = _mapper.Map<ProductCategoryDto>(productCategory);

            return CreatedAtRoute("GetProductCategory", new { id = productCategoryDto.Id }, productCategoryDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProductCategory(int id, [FromBody] UpdateProductCategoryDto updateProductCategoryDto)
        {
            if (updateProductCategoryDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var productCategory = _unitOfWork.ProductCategoryRepository.Get(id);
            if (productCategory == null)
            {
                return NotFound();
            }

            _mapper.Map(updateProductCategoryDto, productCategory);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating product category {updateProductCategoryDto.CategoryName} failed on saving.");

            return Ok(updateProductCategoryDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProductCategory(int id)
        {
            var productCategory = _unitOfWork.ProductCategoryRepository.Get(id);
            if (productCategory == null)
            {
                return NotFound();
            }

            productCategory.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting product category {productCategory.CategoryName} failed on saving.");

            _logger.LogInformation(100, $"Product Category {productCategory.CategoryID}-{productCategory.CategoryName}  was deleted.");

            return CreatedAtRoute("GetProductCategories", new MasterPageResourceParams());
        }
    }
}