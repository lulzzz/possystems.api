using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Product;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Edi;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace POSSystems.Web.Controllers
{
    [Route("api/products")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Product })]
    public class ProductController : BaseController<ProductController>
    {
        public ProductController(IUnitOfWork unitOfWork,
            ILogger<ProductController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetProducts")]
        public IActionResult GetProducts(ProductResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<ProductDto, Product>
                (pageResourceParams?.OrderBy))
            {
                return BadRequest();
            }

            var productsBeforePaging = _unitOfWork.ProductRepository
                 .GetAllDeferred
                 (
                     p =>
                     (pageResourceParams.Q == null || p.UpcScanCode.StartsWith(pageResourceParams.Q))
                     && (pageResourceParams.SearchCatName == null || p.Category.CategoryName.StartsWith(pageResourceParams.SearchCatName))
                     && (pageResourceParams.SearchName == null || p.ProductName.StartsWith(pageResourceParams.SearchName))
                 )
                 .Where(s => s.Status == pageResourceParams.Status)
                 .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<ProductDto, Product>());

            var productsWithPaging = PagedList<Product>.Create(productsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(productsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"details:{paginationMetadata.Response}");

            var productDtos = _mapper.Map<List<ProductDto>>(productsWithPaging);

            return Ok(productDtos);
        }

        [HttpGet("{id}", Name = "GetProduct")]
        public IActionResult GetProduct(int id)
        {
            var product = _unitOfWork.ProductRepository.GetOne(id);

            if (product == null)
                return NotFound();

            var productDto = _mapper.Map<ProductDto>(product);

            return Ok(productDto);
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var product = _mapper.Map<Product>(createProductDto);

            _unitOfWork.ProductRepository.Add(product);

            if (!_unitOfWork.Save())
                throw new Exception("Creating product failed on saving.");

            var productDto = _mapper.Map<ProductDto>(product);

            return CreatedAtRoute("GetProduct", new { id = productDto.Id }, productDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var product = _unitOfWork.ProductRepository.Get(id);
            if (product == null)
            {
                return NotFound();
            }

            _mapper.Map(updateProductDto, product);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating product {id} failed on saving.");

            return Ok(updateProductDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _unitOfWork.ProductRepository.Get(id);
            if (product == null)
            {
                return NotFound();
            }

            var deletePermanently = bool.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("deletePermanently", "False"));

            if (deletePermanently)
                _unitOfWork.ProductRepository.Remove(product);
            else
                product.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting product id failed on saving.");

            _logger.LogInformation(100, $"Product {id}  was deleted.");

            return CreatedAtRoute("GetProducts", new ProductResourceParams());
        }

        [HttpPost("import", Name = "ImportProducts")]
        public IActionResult ImportProducts()
        {
            var csvSource = _unitOfWork.SourceRepository.SingleOrDefault(s => s.Status == Statuses.Active.Humanize() && s.FileType == FileType.CSV.Humanize());

            if (csvSource == null) return StatusCode(500, "Csv Source server error.");

            try
            {
                var file = Request.Form.Files[0];
                var pathToSave = Path.Combine(csvSource.LocalPath, csvSource.SubLocalPath);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    using var stream = new FileStream(fullPath, FileMode.Create);
                    file.CopyTo(stream);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }

            var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var csvProcessor = new CompleteCsvProcessor(_unitOfWork, _logger, _mapper, username);
            csvProcessor.Process(csvSource.LocalPath, csvSource.SubLocalPath, csvSource.ProcessingPath, csvSource.SupplierId);

            return Ok();
        }
    }
}