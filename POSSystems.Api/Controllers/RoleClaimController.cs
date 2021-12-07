using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.RoleClaim;
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
    [Route("api/roleclaims")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.User })]
    public class RoleClaimController : BaseController<RoleClaimController>
    {
        public RoleClaimController(IUnitOfWork unitOfWork,
            ILogger<RoleClaimController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetRoleClaims")]
        public IActionResult GetRoleClaims(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<RoleClaimDto, RoleClaim>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var roleClaimsBeforePaging = _unitOfWork.RoleClaimRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<RoleClaimDto, RoleClaim>())
                .Where(s => s.Status == pageResourceParams.Status);

            var roleClaimsWithPaging = PagedList<RoleClaim>.Create(roleClaimsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(roleClaimsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"roleclaims:{paginationMetadata.Response}");

            var roleClaimDtos = _mapper.Map<List<RoleClaimDto>>(roleClaimsWithPaging);

            return Ok(roleClaimDtos);
        }

        [HttpGet("{id}", Name = "GetRoleClaim")]
        public IActionResult GetRoleClaim(int id)
        {
            var roleClaim = _unitOfWork.RoleClaimRepository.SingleOrDefault(x => x.Id == id);

            if (roleClaim == null)
                return NotFound();

            var roleClaimDto = _mapper.Map<RoleClaimDto>(roleClaim);

            return Ok(roleClaimDto);
        }

        [HttpPost]
        public IActionResult CreateRoleClaim([FromBody] CreateRoleClaimDto createRoleClaimDto)
        {
            if (createRoleClaimDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var exists = _unitOfWork.RoleClaimRepository.Exists(x => x.RoleId == createRoleClaimDto.RoleId && x.ClaimValue == createRoleClaimDto.ClaimValue);
            if (exists) return StatusCode(500, new { Message = "resources.roleclaims.unique" });

            var roleClaim = _mapper.Map<RoleClaim>(createRoleClaimDto);

            var role = _unitOfWork.RoleRepository.SingleOrDefault(x => x.RoleId == createRoleClaimDto.RoleId);

            roleClaim.ClaimType = role.RoleName;

            _unitOfWork.RoleClaimRepository.Add(roleClaim);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating role claim {createRoleClaimDto.ClaimType} failed on saving.");

            var roleClaimDto = _mapper.Map<RoleClaimDto>(roleClaim);

            return CreatedAtRoute("GetRoleClaim", new { id = roleClaimDto.Id }, roleClaimDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRoleClaim(int id, [FromBody] UpdateRoleClaimDto updateRoleClaimDto)
        {
            if (updateRoleClaimDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var exists = _unitOfWork.RoleClaimRepository.Exists(x => x.RoleId == updateRoleClaimDto.RoleId && x.ClaimValue == updateRoleClaimDto.ClaimValue);
            if (exists) return StatusCode(500, new { Message = "resources.roleclaims.unique" });

            var roleClaim = _unitOfWork.RoleClaimRepository.SingleOrDefault(x => x.Id == id);
            if (roleClaim == null)
            {
                return NotFound();
            }

            var role = _unitOfWork.RoleRepository.SingleOrDefault(x => x.RoleId == updateRoleClaimDto.RoleId);

            roleClaim.ClaimType = role.RoleName;

            _mapper.Map(updateRoleClaimDto, roleClaim);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating role claim {updateRoleClaimDto.Id} failed on saving.");

            return Ok(updateRoleClaimDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRoleClaim(int id)
        {
            var roleClaim = _unitOfWork.RoleClaimRepository.SingleOrDefault(x => x.Id == id);
            if (roleClaim == null)
            {
                return NotFound();
            }

            _unitOfWork.RoleClaimRepository.Remove(roleClaim);

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting role claim {roleClaim.ClaimType} failed on saving.");

            _logger.LogInformation(100, $"Role Claim {roleClaim.ClaimType} was deleted.");

            return CreatedAtRoute("GetRoleClaims", new MasterPageResourceParams());
        }

        [HttpPost("assign/{roleId}")]
        public IActionResult AssignAllClaims(int roleId)
        {
            var role = _unitOfWork.RoleRepository.SingleOrDefault(x => x.RoleId == roleId);
            if (role == null)
            {
                return NotFound();
            }

            var permissions = Enum.GetNames(typeof(Permission)).Where( p => !p.Equals("Company"));
            foreach (var permission in permissions)
            {
                if (!_unitOfWork.RoleClaimRepository.Exists(x => x.RoleId == roleId && x.ClaimValue == permission))
                {
                    var roleClaim = new RoleClaim { RoleId = roleId, ClaimType = role.RoleName, ClaimValue = permission };
                    _unitOfWork.RoleClaimRepository.Add(roleClaim);
                }
            }

            if (!_unitOfWork.Save())
                throw new Exception($"Creating roleclaim for role - {role.RoleName} failed on saving.");

            return Ok();
        }
    }
}