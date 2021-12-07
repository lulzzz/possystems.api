using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Transaction;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System.Collections.Generic;

namespace POSSystems.Web.Controllers
{
    [Route("")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.SalesHistory })]
    public class TransactionController : BaseController<TransactionController>
    {
        public TransactionController(IUnitOfWork unitOfWork,
            ILogger<TransactionController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet("api/transactions", Name = "GetTransactions")]
        public IActionResult GetTransactions(GridPageResourceParams pageResourceParams)
        {
            if (!_unitOfWork.TransactionRepository.Exists(p => p.SalesId == pageResourceParams.Id))
            {
                return NotFound();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<TransactionDto, Transaction>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var transactionsBeforePaging = _unitOfWork.TransactionRepository
                .GetAllDeferred(p => p.SalesId == pageResourceParams.Id)
                .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<TransactionDto, Transaction>());

            var transactionWithPaging = PagedList<Transaction>.Create(transactionsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(transactionWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"transactions:{paginationMetadata.Response}");

            var salesDetailDtos = _mapper.Map<List<TransactionDto>>(transactionWithPaging);

            return Ok(salesDetailDtos);
        }
    }
}