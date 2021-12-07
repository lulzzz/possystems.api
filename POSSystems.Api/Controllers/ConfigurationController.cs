using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Configuration;
using POSSystems.Core.Models;
using POSSystems.Infrastructure;
using POSSystems.Helpers;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.Paging;
using POSSystems.Web.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace POSSystems.Web.Controllers
{
    [Route("api/configurations")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Config })]
    public class ConfigurationController : BaseController<ConfigurationController>
    {
        public ConfigurationController(IUnitOfWork unitOfWork,
            ILogger<ConfigurationController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetConfigurations")]
        public IActionResult GetConfigurations(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<ConfigurationDto, Configuration>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var configurationsBeforePaging = _unitOfWork.ConfigurationRepository
                .GetAllDeferred()
                .ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<ConfigurationDto, Configuration>());

            var configurationsWithPaging = PagedList<Configuration>.Create(configurationsBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(configurationsWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"configurations:{paginationMetadata.Response}");

            var configurationDtos = _mapper.Map<List<ConfigurationDto>>(configurationsWithPaging);

            return Ok(configurationDtos);
        }

        [HttpGet("{id}", Name = "GetConfiguration")]
        public IActionResult GetConfiguration(int id)
        {
            var configuration = _unitOfWork.ConfigurationRepository.Get(id);

            if (configuration == null)
                return NotFound();

            var productCategorieDto = _mapper.Map<ConfigurationDto>(configuration);

            return Ok(productCategorieDto);
        }

        [HttpPost]
        public IActionResult CreateConfiguration([FromBody] CreateConfigurationDto createConfigurationDto)
        {
            if (createConfigurationDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var configuration = _mapper.Map<Configuration>(createConfigurationDto);

            _unitOfWork.ConfigurationRepository.Add(configuration);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating configuration {createConfigurationDto.ConfigCode} failed on saving.");

            var configurationDto = _mapper.Map<ConfigurationDto>(configuration);

            return CreatedAtRoute("GetConfiguration", new { id = configurationDto.Id }, configurationDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateConfiguration(int id, [FromBody] UpdateConfigurationDto updateConfigurationDto)
        {
            if (updateConfigurationDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var configuration = _unitOfWork.ConfigurationRepository.Get(id);
            if (configuration == null)
            {
                return NotFound();
            }

            _mapper.Map(updateConfigurationDto, configuration);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating configuration {configuration.ConfigCode} failed on saving.");

            return Ok(updateConfigurationDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteConfiguration(int id)
        {
            var configuration = _unitOfWork.ConfigurationRepository.Get(id);
            if (configuration == null)
            {
                return NotFound();
            }

            //_unitOfWork.ConfigurationRepository.Remove(configuration);
            configuration.Status = Statuses.Inactive.Humanize();
            if (!_unitOfWork.Save())
                throw new Exception($"Deleting configuration {configuration.ConfigCode} failed on saving.");

            _logger.LogInformation(100, $"Configuration {configuration.ConfigCode} was deleted.");

            return CreatedAtRoute("GetConfigurations", new MasterPageResourceParams());
        }

        [HttpPut()]
        public IActionResult UpdateConfigurations([FromBody] UpdateConfigurationDtoContainer updateConfigurationDtoContainer)
        {
            if (updateConfigurationDtoContainer == null)
                return BadRequest();

            var propertyInfos = typeof(UpdateConfigurationDtoContainer).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyValue = propertyInfo.GetValue(updateConfigurationDtoContainer);
                if (propertyValue == null) continue;

                var configuration = _unitOfWork.ConfigurationRepository.FirstOrDefault(c => c.ConfigCode == propertyInfo.Name);
                if (configuration == null) continue;

                var configDto = new UpdateConfigurationDto { ConfigValue = propertyValue.ToString() };

                _mapper.Map(configDto, configuration);

                if (!_unitOfWork.Save())
                    throw new Exception($"Updating configuration {configuration.ConfigCode} failed on saving.");
            }

            return Ok();
        }
    }
}