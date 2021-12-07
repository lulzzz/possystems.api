using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PurchaseReturn;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;

namespace POSSystems.Web.Controllers
{
    [Route("api/purchasereturns")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Purchase })]
    public class PurchaseReturnController : BaseController<PurchaseReturnController>
    {
        public PurchaseReturnController(IUnitOfWork unitOfWork,
            ILogger<PurchaseReturnController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetPurchaseReturns")]
        public IActionResult GetPurchaseReturns(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<PurchaseReturnDto, PurchaseReturn>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var purchaseReturnsBeforePaging = _unitOfWork.PurchaseReturnRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<PurchaseReturnDto, PurchaseReturn>());

            var purchaseReturnsWithPaging = PagedList<PurchaseReturn>.Create(purchaseReturnsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(purchaseReturnsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"purchasereturns:{paginationMetadata.Response}");

            var purchaseReturnDtos = _mapper.Map<List<PurchaseReturnDto>>(purchaseReturnsWithPaging);

            return Ok(purchaseReturnDtos);
        }

        [HttpGet("{id}", Name = "GetPurchaseReturn")]
        public IActionResult GetPurchaseReturn(int id)
        {
            var purchaseReturn = _unitOfWork.PurchaseReturnRepository.Get(id);

            if (purchaseReturn == null)
                return NotFound();

            var purchaseReturnDto = _mapper.Map<PurchaseReturnDto>(purchaseReturn);

            return Ok(purchaseReturnDto);
        }

        [HttpPost]
        public IActionResult CreatePurchaseReturn([FromBody] CreatePurchaseReturnDto createPurchaseReturnDto)
        {
            if (createPurchaseReturnDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var purchaseReturn = _mapper.Map<PurchaseReturn>(createPurchaseReturnDto);

            var productDetail = _unitOfWork.ProductRepository.GetByUpc(purchaseReturn.UpcScanCode);

            if (productDetail == null)
                throw new Exception($"In purchase detail product not found for upc {createPurchaseReturnDto.UpcScanCode}.");

            purchaseReturn.ProductDetailId = productDetail.ProductId;
            _unitOfWork.PurchaseReturnRepository.Add(purchaseReturn);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating purchase return of the purchase {createPurchaseReturnDto.PurchaseId} failed on saving.");

            var purchaseReturnDto = _mapper.Map<PurchaseReturnDto>(purchaseReturn);

            return CreatedAtRoute("GetPurchaseReturn", new { id = purchaseReturnDto.PurchaseId }, purchaseReturnDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePurchaseReturn(int id, [FromBody] UpdatePurchaseReturnDto updatePurchaseReturnDto)
        {
            if (updatePurchaseReturnDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var purchaseReturn = _unitOfWork.PurchaseReturnRepository.Get(id);
            if (purchaseReturn == null)
            {
                return NotFound();
            }

            var productDetail = _unitOfWork.ProductRepository.GetByUpc(purchaseReturn.UpcScanCode);

            if (productDetail == null)
                throw new Exception($"In purchase detail product not found for upc {updatePurchaseReturnDto.UpcScanCode}.");

            purchaseReturn.ProductDetailId = productDetail.ProductId;

            _mapper.Map(updatePurchaseReturnDto, purchaseReturn);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating purchase return {id} failed on saving.");

            return Ok(updatePurchaseReturnDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePurchaseReturn(int id)
        {
            var purchaseReturn = _unitOfWork.PurchaseReturnRepository.Get(id);
            if (purchaseReturn == null)
            {
                return NotFound();
            }

            purchaseReturn.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting purchase return {id} failed on saving.");

            _logger.LogInformation(100, $"Purchase Return {id}  was deleted.");

            return CreatedAtRoute("GetPurchaseReturns", new MasterPageResourceParams());
        }
    }
}