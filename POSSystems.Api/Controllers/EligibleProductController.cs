using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;
using POSSystems.Core.Dtos.EligibleProduct;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Jobs;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;

namespace POSSystems.Web.Controllers
{
    [Route("api/eligibleproducts")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Product })]
    public class EligibleProductController : BaseController<EligibleProductController>
    {
        public EligibleProductController(IUnitOfWork unitOfWork,
            ILogger<EligibleProductController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetEligibleProducts")]
        public IActionResult GetEligibleProducts(MasterPageResourceParams eligiblePageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<EligibleProductDto, EligibleProduct>
                (eligiblePageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var eligibleProductsBeforePaging = _unitOfWork.EligibleProductRepository
                .GetAllDeferred(ep => (eligiblePageResourceParams.Q == null || ep.UPC.StartsWith(eligiblePageResourceParams.Q)) &&
                (eligiblePageResourceParams.SearchName == null || ep.Description.StartsWith(eligiblePageResourceParams.SearchName)))
                //.Where(s => s.Status == eligiblePageResourceParams.Status)
                .ApplySort(eligiblePageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<EligibleProductDto, EligibleProduct>());

            var eligibleProductsWithPaging = PagedList<EligibleProduct>.Create(eligibleProductsBeforePaging
                , eligiblePageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(eligibleProductsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"eligibleproducts:{paginationMetadata.Response}");

            var eligibleProductsDtos = _mapper.Map<List<EligibleProductDto>>(eligibleProductsWithPaging);

            return Ok(eligibleProductsDtos);
        }

        [HttpGet("{id}", Name = "GetEligibleProduct")]
        public IActionResult GetEligibleProduct(string id)
        {
            var eligibleProduct = _unitOfWork.EligibleProductRepository.Get(id);

            if (eligibleProduct == null)
                return NotFound();

            var eligibleProductDto = _mapper.Map<EligibleProductDto>(eligibleProduct);

            return Ok(eligibleProductDto);
        }

        [HttpGet("import", Name = "ImportEligibleProduct")]
        public IActionResult ImportEligibleProduct()
        {
            //var host = new WebHostBuilder().UseStartup<Startup>().Build();

            //var sigisJob = new SigisJob(host.Services.GetService(typeof(IUnitOfWork)) as IUnitOfWork, _azureConfig, _sigisLogger, _applicationData);

            //sigisJob.Execute();

            return Ok();
        }
    }
}