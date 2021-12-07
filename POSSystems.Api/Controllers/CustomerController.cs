using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Dtos.Customer;
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
    [Route("api/customers")]
    [TypeFilter(typeof(CheckPermissionAttribute), Arguments = new object[] { Permission.Customer })]
    public class CustomerController : BaseController<CustomerController>
    {
        public CustomerController(IUnitOfWork unitOfWork,
            ILogger<CustomerController> logger,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            IMapper mapper)
            : base(unitOfWork, logger, urlHelper, propertyMappingService, mapper)
        {
        }

        [HttpGet(Name = "GetCustomers")]
        public IActionResult GetCustomers(MasterPageResourceParams customerPageResourceParams)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<CustomerDto, Customer>
                (customerPageResourceParams.OrderBy))
            {
                return BadRequest();
            }

            var customersBeforePaging = _unitOfWork.CustomerRepository
                .GetAllDeferred(ep => (customerPageResourceParams.Q == null || ep.LoyaltyCardNumber.StartsWith(customerPageResourceParams.Q)) &&
                (customerPageResourceParams.SearchName == null || ep.CustomerName.StartsWith(customerPageResourceParams.SearchName)))
                .Where(s => s.Status == customerPageResourceParams.Status)
                .ApplySort(customerPageResourceParams.OrderBy, _propertyMappingService.GetPropertyMapping<CustomerDto, Customer>());

            var customersWithPaging = PagedList<Customer>.Create(customersBeforePaging
                , customerPageResourceParams.Range);

            var paginationMetadata = GeneratePaginationMetadata(customersWithPaging);

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"customers:{paginationMetadata.Response}");

            var customerDtos = _mapper.Map<List<CustomerDto>>(customersWithPaging);

            return Ok(customerDtos);
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult GetCustomer(int id)
        {
            var customer = _unitOfWork.CustomerRepository.Get(id);

            if (customer == null)
                return NotFound();

            var customerDto = _mapper.Map<CustomerDto>(customer);

            return Ok(customerDto);
        }

        [HttpPost]
        public IActionResult CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            if (createCustomerDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var customer = _mapper.Map<Customer>(createCustomerDto);

            var configInitialPointRewarded = int.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("initialPointReward", "0"));
            var configRedeemThresholdPoint = int.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("redeemThresholdPoint", "0"));
            var configDollarPointConversionRatio = int.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("dollarPointConversionRatio", "0"));
            var configPointDollarConversionRatio = int.Parse(_unitOfWork.ConfigurationRepository.GetConfigByKey("pointDollarConversionRatio", "0"));

            customer.InitialPointRewarded = configInitialPointRewarded;
            customer.RedeemThresholdPoint = configRedeemThresholdPoint;
            customer.DollarPointConversionRatio = configDollarPointConversionRatio;
            customer.PointDollarConversionRatio = configPointDollarConversionRatio;

            customer.LoyaltyPointEarned = customer.InitialPointRewarded;
            _unitOfWork.CustomerRepository.Add(customer);

            if (!_unitOfWork.Save())
                throw new Exception($"Creating customer {createCustomerDto.CustomerName} failed on saving.");

            var customerDto = _mapper.Map<CustomerDto>(customer);

            return CreatedAtRoute("GetCustomer", new { id = customerDto.Id }, customerDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateCustomerDto)
        {
            if (updateCustomerDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var customer = _unitOfWork.CustomerRepository.Get(id);
            if (customer == null)
            {
                return NotFound();
            }

            _mapper.Map(updateCustomerDto, customer);

            if (!_unitOfWork.Save())
                throw new Exception($"Updating customer {updateCustomerDto.CustomerName} failed on saving.");

            return Ok(updateCustomerDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _unitOfWork.CustomerRepository.Get(id);
            if (customer == null)
            {
                return NotFound();
            }

            //_unitOfWork.CustomerRepository.Remove(customer);
            customer.Status = Statuses.Inactive.Humanize();

            if (!_unitOfWork.Save())
                throw new Exception($"Deleting customer {customer.CustomerName} failed on saving.");

            _logger.LogInformation(100, $"Customer {customer.CustomerID}-{customer.CustomerName}  was deleted.");

            //return NoContent();
            return CreatedAtRoute("GetCustomers", new MasterPageResourceParams());
        }
    }
}