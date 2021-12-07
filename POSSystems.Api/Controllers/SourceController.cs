using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
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
    [Route("api/sources")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Config })]
    public class SourceController : BaseController<SourceController>
    {
        public SourceController(IUnitOfWork unitOfWork,
            ILogger<SourceController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetSources")]
        public IActionResult GetSources(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<SourceDto, Source>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var sourcesBeforePaging = _unitOfWork.SourceRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<SourceDto, Source>())
                .Where(s => s.Status == pageResourceParams.Status);

            var sourcesWithPaging = PagedList<Source>.Create(sourcesBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(sourcesWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"sources:{paginationMetadata.Response}");

            var sourceDtos = _mapper.Map<List<SourceDto>>(sourcesWithPaging);

            return Ok(sourceDtos);
        }

        [HttpGet("{id}", Name = "GetSource")]
        public IActionResult GetSource(int id)
        {
            var source = _unitOfWork.SourceRepository.Get(id);

            if (source == null)
                return NotFound();

            var sourcesDto = _mapper.Map<SourceDto>(source);

            return Ok(sourcesDto);
        }

        [HttpPost]
        public IActionResult CreateSource([FromBody] CreateSourceDto createSourceDto)
        {
            if (createSourceDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var source = _mapper.Map<Source>(createSourceDto);

            _unitOfWork.SourceRepository.Add(source);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating source {createSourceDto.HostAddress} failed on saving.");

            var sourceDto = _mapper.Map<SourceDto>(source);

            return CreatedAtRoute("GetSource", new { id = sourceDto.Id }, sourceDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSource(int id, [FromBody] UpdateSourceDto updateSourceDto)
        {
            if (updateSourceDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var source = _unitOfWork.SourceRepository.Get(id);
            if (source == null)
            {
                return NotFound();
            }

            _mapper.Map(updateSourceDto, source);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating source {updateSourceDto.HostAddress} failed on saving.");

            return Ok(updateSourceDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSource(int id)
        {
            var source = _unitOfWork.SourceRepository.Get(id);
            if (source == null)
            {
                return NotFound();
            }

            source.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting source {source.HostAddress} failed on saving.");

            _logger.LogInformation(100, $"Source {source.SourceID}-{source.HostAddress} was deleted.");

            return CreatedAtRoute("GetSources", new MasterPageResourceParams());
        }
    }
}