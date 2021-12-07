using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.UserRole;
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
    [Route("api/userroles")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.User })]
    public class UserRoleController : BaseController<UserRoleController>
    {
        public UserRoleController(IUnitOfWork unitOfWork,
            ILogger<UserRoleController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetUserRoles")]
        public IActionResult GetUserRoles(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<UserRoleDto, UserRole>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var userRolesBeforePaging = _unitOfWork.UserRoleRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<UserRoleDto, UserRole>());

            var userRolesWithPaging = PagedList<UserRole>.Create(userRolesBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(userRolesWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"userroles:{paginationMetadata.Response}");

            var userRoleDtos = _mapper.Map<List<UserRoleDto>>(userRolesWithPaging);

            return Ok(userRoleDtos);
        }

        [HttpGet("{id}", Name = "GetUserRole")]
        public IActionResult GetUserRole(string id)
        {
            var userRoles = _unitOfWork.UserRoleRepository.Get(int.Parse(id));

            if (userRoles == null)
                return NotFound();

            var userRoleDto = _mapper.Map<UserRoleDto>(userRoles);

            return Ok(userRoleDto);
        }

        [HttpPost]
        public IActionResult CreateUserRole([FromBody] CreateUserRoleDto createUserRoleDto)
        {
            if (createUserRoleDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            CheckDuplicate(createUserRoleDto.UserId, createUserRoleDto.RoleId);
            var userRole = _mapper.Map<UserRole>(createUserRoleDto);

            _unitOfWork.UserRoleRepository.Add(userRole);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating user role {createUserRoleDto.Id} failed on saving.");

            var userRoleDto = _mapper.Map<UserRoleDto>(userRole);

            return CreatedAtRoute("GetUserRole", new { id = userRoleDto.Id }, userRoleDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUserRole(string id, [FromBody] UpdateUserRoleDto editUserRoleDto)
        {
            if (editUserRoleDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var userRole = _unitOfWork.UserRoleRepository.SingleOrDefault(x => x.UserRoleId == int.Parse(id));
            if (userRole == null)
            {
                return NotFound();
            }

            CheckDuplicate(editUserRoleDto.UserId, editUserRoleDto.RoleId);
            _mapper.Map(editUserRoleDto, userRole);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating user role {editUserRoleDto.Id} failed on saving.");

            return Ok(editUserRoleDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUserRole(string id)
        {
            var userRole = _unitOfWork.UserRoleRepository.SingleOrDefault(x => x.UserRoleId == int.Parse(id));
            if (userRole == null)
            {
                return NotFound();
            }

            userRole.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting role {userRole.UserRoleId} failed on saving.");

            _logger.LogInformation(100, $"UserInfo {userRole.UserId}-{userRole.UserRoleId}  was deleted.");

            return NoContent();
        }

        private void CheckDuplicate(int userId, int roleId)
        {
            var userRole = _unitOfWork.UserRoleRepository.GetAllDeferred(x => x.UserId == userId && x.RoleId == roleId).ToList();

            if (userRole.Count > 0)
                throw new Exception("This role has already been assigned to this user.");
        }
    }
}