using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Company;
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
    [Route("api/companies")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Company })]
    public class CompanyController : BaseController<CompanyController>
    {
        public CompanyController(IUnitOfWork unitOfWork,
            ILogger<CompanyController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetCompanies")]
        public IActionResult GetCompanies(MasterPageResourceParams pageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<CompanyDto, Company>
                (pageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var companysBeforePaging = _unitOfWork.CompanyRepository
                .GetAllDeferred().ApplySort(pageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<CompanyDto, Company>())
                .Where(s => s.Status == pageResourceParams.Status);

            var companysWithPaging = PagedList<Company>.Create(companysBeforePaging
                , pageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(companysWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"companies:{paginationMetadata.Response}");

            var companyDtos = _mapper.Map<List<CompanyDto>>(companysWithPaging);

            return Ok(companyDtos);
        }

        [HttpGet("{id}", Name = "GetCompany")]
        public IActionResult GetCompany(int id)
        {
            var company = _unitOfWork.CompanyRepository.Get(id);

            if (company == null)
                return NotFound();

            var companyDto = _mapper.Map<CompanyDto>(company);

            return Ok(companyDto);
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody] CreateCompanyDto createCompanyDto)
        {
            if (createCompanyDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var company = _mapper.Map<Company>(createCompanyDto);

            _unitOfWork.CompanyRepository.Add(company);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating company {createCompanyDto.Name} failed on saving.");

            var companyDto = _mapper.Map<CompanyDto>(company);

            return CreatedAtRoute("GetCompany", new { id = companyDto.Id }, companyDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCompany(int id, [FromBody] UpdateCompanyDto updateCompanyDto)
        {
            if (updateCompanyDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var company = _unitOfWork.CompanyRepository.Get(id);
            if (company == null)
            {
                return NotFound();
            }

            _mapper.Map(updateCompanyDto, company);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating company {updateCompanyDto.Name} failed on saving.");

            return Ok(updateCompanyDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCompany(int id)
        {
            var company = _unitOfWork.CompanyRepository.Get(id);
            if (company == null)
            {
                return NotFound();
            }

            company.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting company {company.Name} failed on saving.");

            _logger.LogInformation(100, $"Company {company.Id}-{company.Name}  was deleted.");

            return CreatedAtRoute("GetCompanies", new MasterPageResourceParams());
        }

        [HttpGet("my", Name = "GetMyCompany")]
        public IActionResult GetMyCompany()
        {
            var company = _unitOfWork.CompanyRepository.FirstOrDefault( c => c.Status == Statuses.Active.Humanize());
            var companyDto = _mapper.Map<CompanyDto>(company);

            return Ok(companyDto);
        }

        [HttpPut("my")]
        public IActionResult UpdateMyCompany([FromBody]UpdateCompanyDto updateCompanyDto)
        {
            if (updateCompanyDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var company = _unitOfWork.CompanyRepository.Get(updateCompanyDto.Id);
            if (company == null)
            {
                return NotFound();
            }

            _mapper.Map(updateCompanyDto, company);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating company {updateCompanyDto.Name} failed on saving.");

            return Ok(updateCompanyDto);
        }
    }
}