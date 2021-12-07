using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.RoleClaim;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Web.Controllers
{
    [Route("api/claims")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.User })]
    public class ClaimController : BaseController<ClaimController>
    {
        public ClaimController(IUnitOfWork unitOfWork,
            ILogger<ClaimController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetClaims")]
        public IActionResult GetClaims()
        {
            var claimsDtos = _mapper.Map<List<ClaimDto>>(Enum.GetNames(typeof(Permission)).Where(p => !p.Equals("Company")));

            var claimsWithPaging = PagedList<ClaimDto>.Create(claimsDtos);

            var paginationMetadata = GeneratePaginationMetadata(claimsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"claims:{paginationMetadata.Response}");

            return Ok(claimsDtos);
        }
    }
}