using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Session;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Web.Controllers
{
    [Route("api/sessions")]
    public class SessionController : BaseController<SessionController>
    {
        public SessionController(IUnitOfWork unitOfWork,
            ILogger<SessionController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetSessions")]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Timesheet })]
        public IActionResult Getsessions(TimesheetResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<SessionDto, Session>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var sessionsBeforePaging = _unitOfWork.SessionRepository
                .GetAllDeferred(
                    s =>
                    (pageResourceParams.Q == null || s.UserId.ToString() == pageResourceParams.Q) &&
                    (!pageResourceParams.StartTime.HasValue || s.StartTime >= pageResourceParams.StartTime) &&
                    (!pageResourceParams.EndTime.HasValue || s.EndTime <= pageResourceParams.EndTime)
                ).ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<SessionDto, Session>())
                .Where(s => s.Status == pageResourceParams.Status);

            var sessionsWithPaging = PagedList<Session>.Create(sessionsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(sessionsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"sessions:{paginationMetadata.Response}");

            var sessionDtos = _mapper.Map<List<SessionDto>>(sessionsWithPaging);

            return Ok(sessionDtos);
        }

        [HttpGet("{id}", Name = "GetSession")]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Timesheet })]
        public IActionResult GetSession(int id)
        {
            var session = _unitOfWork.SessionRepository.Get(id);

            if (session == null)
                return NotFound();

            var sessionDto = _mapper.Map<SessionDto>(session);

            return Ok(sessionDto);
        }

        [HttpGet("status", Name = "GetSessionStatus")]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Session })]
        public IActionResult GetSessionStatus()
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var session = _unitOfWork.SessionRepository.GetLastSessionByUsername(username);

            var punchDto = _mapper.Map<PunchDto>(session);

            return Ok(punchDto);
        }

        [HttpPost("punch", Name = "PunchInOut")]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Session })]
        public IActionResult PunchInOut(string barcode)
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var user = _unitOfWork.UserRepository.GetByUsername(username);

            if (string.IsNullOrEmpty(barcode) || barcode != user?.Barcode)
            {
                _logger.LogWarning($"The scanned barcode - {barcode} do not match with the user - {username} logged in with.");
                return StatusCode(406, "The scanned barcode is unacceptable.");
            }

            var session = _unitOfWork.SessionRepository.GetLastSessionByUsername(username);
            if (session == null)
            {
                session = new Session { UserId = user.UserId, StartTime = DateTime.UtcNow };
                _unitOfWork.SessionRepository.Add(session);
            }
            else if (session.EndTime == null)
                session.EndTime = DateTime.UtcNow;
            else
            {
                session = new Session { UserId = user.UserId, StartTime = DateTime.UtcNow };
                _unitOfWork.SessionRepository.Add(session);
            }

            if (!_unitOfWork.Save())
                throw new Exception($"Creating punchin/out user - {username} failed on saving.");

            var punchDto = _mapper.Map<PunchDto>(session);

            return StatusCode(202, punchDto);
        }

        [HttpPost]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Timesheet })]
        public IActionResult CreateSession([FromBody] CreateSessionDto createSessionDto)
        {
            if (createSessionDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var session = _mapper.Map<Session>(createSessionDto);

            _unitOfWork.SessionRepository.Add(session);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating session for user - {createSessionDto.UserId} failed on saving.");

            var sessionDto = _mapper.Map<SessionDto>(session);

            return CreatedAtRoute("GetSession", new { id = sessionDto.Id }, sessionDto);
        }

        [HttpPut("{id}")]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Timesheet })]
        public IActionResult UpdateSession(int id, [FromBody] UpdateSessionDto updateSessionDto)
        {
            if (updateSessionDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var session = _unitOfWork.SessionRepository.Get(id);
            if (session == null)
            {
                return NotFound();
            }

            _mapper.Map(updateSessionDto, session);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating session - {id} failed on saving.");

            return Ok(updateSessionDto);
        }

        [HttpDelete("{id}")]
        [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Timesheet })]
        public IActionResult DeleteSession(int id)
        {
            var session = _unitOfWork.SessionRepository.Get(id);
            if (session == null)
            {
                return NotFound();
            }

            session.Status = Statuses.Inactive.Humanize();
            if (!_unitOfWork.Save())
                throw new Exception($"Deleting session - {session.SessionId} failed on saving.");

            _logger.LogInformation(100, $"Session - {session.SessionId} was deleted.");

            return CreatedAtRoute("GetSessions", new TimesheetResourceParams());
        }
    }
}