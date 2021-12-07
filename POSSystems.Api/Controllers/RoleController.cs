using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Role;
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
    [Route("api/roles")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.User })]
    public class RoleController : BaseController<RoleController>
    {
        public RoleController(IUnitOfWork unitOfWork,
            ILogger<RoleController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetRoles")]
        public IActionResult GetRoles(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<RoleDto, Role>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var rolesBeforePaging = _unitOfWork.RoleRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<RoleDto, Role>())
                .Where(s => s.Status == pageResourceParams.Status);

            var rolesWithPaging = PagedList<Role>.Create(rolesBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(rolesWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"Roles:{paginationMetadata.Response}");

            var roleDtos = _mapper.Map<List<RoleDto>>(rolesWithPaging);

            return Ok(roleDtos);
        }

        [HttpGet("{id}", Name = "GetRole")]
        public IActionResult GetRole(int id)
        {
            var role = _unitOfWork.RoleRepository.SingleOrDefault(x => x.RoleId == id);

            if (role == null)
                return NotFound();

            var roleDto = _mapper.Map<RoleDto>(role);

            return Ok(roleDto);
        }

        [HttpPost]
        public IActionResult CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (createRoleDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var existingRole = _unitOfWork.RoleRepository.SingleOrDefault(x => x.RoleName == createRoleDto.RoleName);
            if (existingRole != null)
                throw new Exception("This role is already exist. Please choose different one.");

            var role = _mapper.Map<Role>(createRoleDto);

            _unitOfWork.RoleRepository.Add(role);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating role {createRoleDto.RoleName} failed on saving.");

            var RoleDto = _mapper.Map<RoleDto>(role);

            return CreatedAtRoute("GetRole", new { id = RoleDto.Id }, RoleDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            if (updateRoleDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var role = _unitOfWork.RoleRepository.SingleOrDefault(x => x.RoleId == id);
            if (role == null)
            {
                return NotFound();
            }

            _mapper.Map(updateRoleDto, role);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating role {updateRoleDto.RoleName} failed on saving.");

            return Ok(updateRoleDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRole(int id)
        {
            var role = _unitOfWork.RoleRepository.SingleOrDefault(x => x.RoleId == id);
            if (role == null)
            {
                return NotFound();
            }

            role.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting role {role.RoleName} failed on saving.");

            _logger.LogInformation(100, $"Role {role.RoleId}-{role.RoleName}  was deleted.");

            return CreatedAtRoute("GetRoles", new MasterPageResourceParams());
        }
    }
}