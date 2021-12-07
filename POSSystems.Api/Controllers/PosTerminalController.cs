using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.PosTerminal;
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
    [Route("api/posterminals")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Config })]
    public class PosTerminalController : BaseController<PosTerminalController>
    {
        public PosTerminalController(IUnitOfWork unitOfWork,
            ILogger<PosTerminalController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetPosTerminals")]
        public IActionResult GetPosTerminals(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<PosTerminalDto, PosTerminal>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var posTerminalsBeforePaging = _unitOfWork.PosTerminalRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<PosTerminalDto, PosTerminal>())
                .Where(s => s.Status == pageResourceParams.Status);

            var posTerminalsWithPaging = PagedList<PosTerminal>.Create(posTerminalsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(posTerminalsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"posterminals:{paginationMetadata.Response}");

            var posTerminalDtos = _mapper.Map<List<PosTerminalDto>>(posTerminalsWithPaging);

            return Ok(posTerminalDtos);
        }

        [HttpGet("{id}", Name = "GetPosTerminal")]
        public IActionResult GetPosTerminal(int id)
        {
            var posTerminal = _unitOfWork.PosTerminalRepository.Get(id);

            if (posTerminal == null)
                return NotFound();

            var posterminalDto = _mapper.Map<PosTerminalDto>(posTerminal);

            return Ok(posterminalDto);
        }

        [HttpPost]
        public IActionResult CreatePosTerminal([FromBody] CreatePosTerminalDto createPosTerminalDto)
        {
            if (createPosTerminalDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var posTerminal = _mapper.Map<PosTerminal>(createPosTerminalDto);

            _unitOfWork.PosTerminalRepository.Add(posTerminal);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating PosTerminal {createPosTerminalDto.TerminalName} failed on saving.");

            var posTerminalDto = _mapper.Map<PosTerminalDto>(posTerminal);

            return CreatedAtRoute("GetPosTerminals", new { id = posTerminalDto.Id }, posTerminalDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePosTerminal(int id, [FromBody] UpdatePosTerminalDto updatePosTerminalDto)
        {
            if (updatePosTerminalDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var posTerminal = _unitOfWork.PosTerminalRepository.Get(id);
            if (posTerminal == null)
            {
                return NotFound();
            }

            _mapper.Map(updatePosTerminalDto, posTerminal);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating PosTerminal {updatePosTerminalDto.TerminalName} failed on saving.");

            return Ok(updatePosTerminalDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePosTerminal(int id)
        {
            var posTerminal = _unitOfWork.PosTerminalRepository.Get(id);
            if (posTerminal == null)
            {
                return NotFound();
            }

            posTerminal.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting PosTerminal {posTerminal.TerminalName} failed on saving.");

            _logger.LogInformation(100, $"PosTerminal {posTerminal.TerminalId}-{posTerminal.TerminalName}  was deleted.");

            return CreatedAtRoute("GetPosTerminals", new MasterPageResourceParams());
        }
    }
}