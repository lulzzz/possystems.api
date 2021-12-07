using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PurchaseDetail;
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
    [Route("")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Purchase })]
    public class PurchaseDetailController : BaseController<PurchaseDetailController>
    {
        public PurchaseDetailController(IUnitOfWork unitOfWork,
            ILogger<PurchaseDetailController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet("api/purchasedetails", Name = "GetPurchaseDetails")]
        public IActionResult GetPurchaseDetails(MasterPageResourceParams purchaseDetailResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<PurchaseDetailDto, PurchaseDetail>
                (purchaseDetailResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var purchaseDetailsBeforePaging = _unitOfWork.PurchaseDetailRepository
                .GetAllDeferred(pd => (purchaseDetailResourceParams.Q == null || pd.UpcScanCode.ToString().StartsWith(purchaseDetailResourceParams.Q)) &&
                (purchaseDetailResourceParams.SearchName == null || pd.Product.ProductName.StartsWith(purchaseDetailResourceParams.SearchName)))
                .ApplySort(purchaseDetailResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<PurchaseDetailDto, PurchaseDetail>());

            var purchaseDetailsWithPaging = PagedList<PurchaseDetail>.Create(purchaseDetailsBeforePaging
                , purchaseDetailResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(purchaseDetailsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"purchasedetails:{paginationMetadata.Response}");

            var purchaseDetailDtos = _mapper.Map<List<PurchaseDetailDto>>(purchaseDetailsWithPaging);

            return Ok(purchaseDetailDtos);
        }

        [HttpGet("api/purchases/{purchaseid}/details", Name = "GetPurchasetDetails")]
        public IActionResult GetPurchaseDetails(int purchaseid, MasterPageResourceParams pageResourceParams)
        {
            if (!_unitOfWork.PurchaseMasterRepository.Exists(p => p.PurchaseId == purchaseid))
            {
                return NotFound();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<PurchaseDetailDto, PurchaseDetail>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var purchaseDetailsBeforePaging = _unitOfWork.PurchaseDetailRepository
                .GetAllDeferred(p => p.PurchaseId == purchaseid).ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<PurchaseDetailDto, PurchaseDetail>());

            var productDetailsWithPaging = PagedList<PurchaseDetail>.Create(purchaseDetailsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(productDetailsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"configurations:{paginationMetadata.Response}");

            var purchaseDetailsDtos = _mapper.Map<List<PurchaseDetailDto>>(productDetailsWithPaging);

            return Ok(purchaseDetailsDtos);
        }

        [HttpGet("api/purchasedetails/{id}", Name = "GetPurchaseDetail")]
        public IActionResult GetPurchaseDetail(int id)
        {
            var purchaseDetail = _unitOfWork.PurchaseDetailRepository.Get(id);

            if (purchaseDetail == null)
                return NotFound();

            var purchaseDetailsDto = _mapper.Map<PurchaseDetailDto>(purchaseDetail);

            return Ok(purchaseDetailsDto);
        }

        [HttpPost("api/purchasedetails")]
        public IActionResult CreatePurchaseDetail([FromBody] CreatePurchaseDetailDto createPurchaseDetailDto)
        {
            if (createPurchaseDetailDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var purchaseDetail = _mapper.Map<PurchaseDetail>(createPurchaseDetailDto);

            var product = _unitOfWork.ProductRepository.GetByUpc(purchaseDetail.UpcScanCode);

            if (product == null)
                throw new Exception($"In purchase detail product not found for upc {createPurchaseDetailDto.UpcScanCode}.");

            purchaseDetail.ProductId = product.ProductId;
            _unitOfWork.PurchaseDetailRepository.Add(purchaseDetail);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating purchase detail {createPurchaseDetailDto.PurchaseId} failed on saving.");

            var purchaseDetailDto = _mapper.Map<PurchaseDetailDto>(purchaseDetail);

            return CreatedAtRoute("GetPurchaseDetail", new { id = purchaseDetailDto.PurchaseId }, purchaseDetailDto);
        }

        [HttpPut("api/purchasedetails/{id}")]
        public IActionResult UpdatePurchaseDetail(int id, [FromBody] UpdatePurchaseDetailDto updatePurchaseDetailDto)
        {
            if (updatePurchaseDetailDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var purchaseDetail = _unitOfWork.PurchaseDetailRepository.Get(id);

            if (purchaseDetail == null)
            {
                return NotFound();
            }
            var product = _unitOfWork.ProductRepository.GetByUpc(purchaseDetail.UpcScanCode);

            if (product == null)
                throw new Exception($"In purchase detail product not found for upc {updatePurchaseDetailDto.UpcScanCode}.");

            purchaseDetail.ProductId = product.ProductId;

            _mapper.Map(updatePurchaseDetailDto, purchaseDetail);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating purchase detail {id} failed on saving.");

            return Ok(updatePurchaseDetailDto);
        }

        [HttpDelete("api/purchasedetails/{id}")]
        public IActionResult DeletePurchaseDetail(int id)
        {
            var purchaseDetail = _unitOfWork.PurchaseDetailRepository.Get(id);
            if (purchaseDetail == null)
            {
                return NotFound();
            }

            purchaseDetail.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting purchase detail {id} failed on saving.");

            _logger.LogInformation(100, $"Purchase detail {purchaseDetail.PurchaseId}  was deleted.");

            return CreatedAtRoute("GetPurchaseDetails", new MasterPageResourceParams());
        }
    }
}