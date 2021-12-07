using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Supplier;
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
    [Route("api/suppliers")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Product })]
    public class SupplierController : BaseController<SupplierController>
    {
        public SupplierController(IUnitOfWork unitOfWork,
            ILogger<SupplierController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetSuppliers")]
        public IActionResult GetSuppliers(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<SupplierDto, Supplier>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var suppliersBeforePaging = _unitOfWork.SupplierRepository
                .GetAllDeferred(ep => (pageResourceParams.Q == null || ep.SupplierName.StartsWith(pageResourceParams.Q)))
                .Where(s => s.Status == pageResourceParams.Status)
                .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<SupplierDto, Supplier>());

            var suppliersWithPaging = PagedList<Supplier>.Create(suppliersBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(suppliersWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"suppliers:{paginationMetadata.Response}");

            var supplierDtos = _mapper.Map<List<SupplierDto>>(suppliersWithPaging);

            return Ok(supplierDtos);
        }

        [HttpGet("{id}", Name = "GetSupplier")]
        public IActionResult GetSupplier(int id)
        {
            var supplier = _unitOfWork.SupplierRepository.Get(id);

            if (supplier == null)
                return NotFound();

            var productCategorieDto = _mapper.Map<SupplierDto>(supplier);

            return Ok(productCategorieDto);
        }

        [HttpPost]
        public IActionResult CreateSupplier([FromBody] CreateSupplierDto createSupplierDto)
        {
            if (createSupplierDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var supplier = _mapper.Map<Supplier>(createSupplierDto);

            _unitOfWork.SupplierRepository.Add(supplier);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating supplier {createSupplierDto.SupplierName} failed on saving.");

            var supplierDto = _mapper.Map<SupplierDto>(supplier);

            return CreatedAtRoute("GetSupplier", new { id = supplierDto.Id }, supplierDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSupplier(int id, [FromBody] UpdateSupplierDto updateSupplierDto)
        {
            if (updateSupplierDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var supplier = _unitOfWork.SupplierRepository.Get(id);
            if (supplier == null)
            {
                return NotFound();
            }

            _mapper.Map(updateSupplierDto, supplier);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating supplier {updateSupplierDto.SupplierName} failed on saving.");

            return Ok(updateSupplierDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSupplier(int id)
        {
            var supplier = _unitOfWork.SupplierRepository.Get(id);
            if (supplier == null)
            {
                return NotFound();
            }

            supplier.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting supplier {supplier.SupplierName} failed on saving.");

            _logger.LogInformation(100, $"Supplier {supplier.SupplierId}-{supplier.SupplierName}  was deleted.");

            return CreatedAtRoute("GetSuppliers", new MasterPageResourceParams());
        }
    }
}