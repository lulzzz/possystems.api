using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Models;
using POSSystems.Dtos.User;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using POSSystems.Web.Infrastructure.Token;
using Microsoft.Extensions.Options;

namespace POSSystems.Web.Controllers
{
    [Route("api/users")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.User })]
    public class UserController : BaseController<UserController>
    {
        private readonly JwtIssuerOptions _jwtOptions;
        
        public UserController(IOptions<JwtIssuerOptions> jwtOptions, IUnitOfWork unitOfWork,
            ILogger<UserController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet(Name = "GetUsers")]
        public IActionResult GetUsers(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<UserDto, User>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var usersBeforePaging = _unitOfWork.UserRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<UserDto, User>())
                .Where(s => s.Status == pageResourceParams.Status);

            var usersWithPaging = PagedList<User>.Create(usersBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(usersWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"users:{paginationMetadata.Response}");

            var userDtos = _mapper.Map<List<UserDto>>(usersWithPaging);

            return Ok(userDtos);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetUser(int id)
        {
            var user = _unitOfWork.UserRepository.SingleOrDefault(x => x.UserId == id);

            if (user == null)
                return NotFound();

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            if (createUserDto.UserName.Equals(_jwtOptions.DevUser, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest();

            var user = _mapper.Map<User>(createUserDto);

            user.Password = SecurityHelper.HashPassword(user.Password);

            if (_unitOfWork.UserRepository.Exists(ui => ui.UserName == createUserDto.UserName))
                return StatusCode(500, new { Message = "resources.users.unique" });

            _unitOfWork.UserRepository.Add(user);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating user {createUserDto.UserName} failed on saving.");

            var userInfoDto = _mapper.Map<UserDto>(user);

            return CreatedAtRoute("GetRole", new { id = userInfoDto.Id }, userInfoDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto editUserDto)
        {
            if (editUserDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var user = _unitOfWork.UserRepository.SingleOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            var existingPass = user.Password;

            _mapper.Map(editUserDto, user);

            user.Password = editUserDto.Password != existingPass ? SecurityHelper.HashPassword(editUserDto.Password) : existingPass;

            if (!_unitOfWork.Save())
                throw new Exception($"Updating user {user.UserName} failed on saving.");

            return Ok(editUserDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _unitOfWork.UserRepository.SingleOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            //_unitOfWork.UserRepository.Remove(user);
            user.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting user {user.UserName} failed on saving.");

            _logger.LogInformation(100, $"UserInfo {user.UserId}-{user.UserName}  was deleted.");

            return NoContent();
        }

        [HttpGet("current", Name = "GetLoggedUser")]
        public IActionResult GetLoggedUser()
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var user = _unitOfWork.UserRepository.GetByUsername(username);

            if (user == null)
                return NotFound();

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }
    }
}