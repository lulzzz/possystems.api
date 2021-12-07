using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PurchaseMaster;
using POSSystems.Core.Models;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Services;
using System;

namespace POSSystems.Web.Controllers
{
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Purchase })]

    public class PurchaseController : BaseController<PurchaseController>
    {
        public PurchaseController(IUnitOfWork unitOfWork,
            ILogger<PurchaseController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreatePurchaseMaster([FromBody] CreatePurchaseMasterDto createPurchaseMasterDto)
        {
            if (createPurchaseMasterDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var purchaseMaster = _mapper.Map<PurchaseMaster>(createPurchaseMasterDto);

            _unitOfWork.PurchaseMasterRepository.Add(purchaseMaster);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating purchase failed on saving.");

            var purchaseMasterDto = _mapper.Map<PurchaseMasterDto>(purchaseMaster);

            return CreatedAtRoute("GetPurchaseMaster", new { id = purchaseMasterDto.Id }, purchaseMasterDto);
        }
    }
}