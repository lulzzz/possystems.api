using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PurchaseMaster;
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
    [Route("api/purchases")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Purchase })]
    public class PurchaseMasterController : BaseController<PurchaseMasterController>
    {
        public PurchaseMasterController(IUnitOfWork unitOfWork,
            ILogger<PurchaseMasterController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetPurchaseMasters")]
        public IActionResult GetPurchaseMasters(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<PurchaseMasterDto, PurchaseMaster>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var purchaseMastersBeforePaging = _unitOfWork.PurchaseMasterRepository
                .GetAllDeferred(pm => (pageResourceParams.Q == null || pm.PurchaseId.ToString().StartsWith(pageResourceParams.Q)))
                .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<PurchaseMasterDto, PurchaseMaster>());

            var purchaseMastersWithPaging = PagedList<PurchaseMaster>.Create(purchaseMastersBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(purchaseMastersWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"purchases:{paginationMetadata.Response}");

            var purchaseMasterDtos = _mapper.Map<List<PurchaseMasterDto>>(purchaseMastersWithPaging);

            return Ok(purchaseMasterDtos);
        }

        [HttpGet("{id}", Name = "GetPurchaseMaster")]
        public IActionResult GetPurchaseMaster(int id)
        {
            var purchaseMaster = _unitOfWork.PurchaseMasterRepository.Get(id);

            if (purchaseMaster == null)
                return NotFound();

            var purchaseMasterDto = _mapper.Map<PurchaseMasterDto>(purchaseMaster);

            return Ok(purchaseMasterDto);
        }

        [HttpPost]
        public IActionResult CreatePurchaseMaster([FromBody] CreatePurchaseMasterDto createPurchaseMasterDto)
        {
            if (createPurchaseMasterDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var purchaseMaster = _mapper.Map<PurchaseMaster>(createPurchaseMasterDto);

            purchaseMaster.DeliveryStatus = DeliveryStatus.Pending.Humanize();

            _unitOfWork.PurchaseMasterRepository.Add(purchaseMaster);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating purchase failed on saving.");

            var purchaseMasterDto = _mapper.Map<PurchaseMasterDto>(purchaseMaster);

            return CreatedAtRoute("GetPurchaseMaster", new { id = purchaseMasterDto.Id }, purchaseMasterDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePurchaseMaster(int id, [FromBody] UpdatePurchaseMasterDto updatePurchaseMasterDto)
        {
            if (updatePurchaseMasterDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var purchaseMaster = _unitOfWork.PurchaseMasterRepository.Get(id);
            if (purchaseMaster == null)
            {
                return NotFound();
            }

            _mapper.Map(updatePurchaseMasterDto, purchaseMaster);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating purchase {id} failed on saving.");

            return Ok(updatePurchaseMasterDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePurchaseMaster(int id)
        {
            var purchaseMaster = _unitOfWork.PurchaseMasterRepository.Get(id);
            if (purchaseMaster == null)
            {
                return NotFound();
            }

            purchaseMaster.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting purchase {id} failed on saving.");

            _logger.LogInformation(100, $"Purchase {purchaseMaster.PurchaseId}  was deleted.");

            return CreatedAtRoute("GetPurchaseMasters", new MasterPageResourceParams());
        }

        [HttpPut("{id}/delivered")]
        public IActionResult ChangeDeliveryStatus(int id)
        {
            var purchaseMaster = _unitOfWork.PurchaseMasterRepository.Get(id);
            if (purchaseMaster == null)
            {
                return NotFound();
            }
            // get purchase details
            var purchaseDetails = _unitOfWork.PurchaseDetailRepository.GetAllDeferred(s => s.PurchaseId == id);
            if (purchaseDetails != null)
            {
                foreach (var item in purchaseDetails)
                {// update product details
                    var productDetails = _unitOfWork.ProductRepository.Get(item.ProductId);
                    if (item.DeliveryStatus == Delivery855Status.Accepted.Humanize() || item.DeliveryStatus == Delivery855Status.Backordered.Humanize())
                    {
                        productDetails.Quantity += item.DeliveredQuantity.Value;
                    }
                }
                //set status
                purchaseMaster.DeliveryStatus = DeliveryStatus.Delivered.Humanize();
            }

            if (!_unitOfWork.Save())
                throw new Exception($"Changing delivery status of purchase {id} failed on saving.");

            return Ok();
        }
    }
}