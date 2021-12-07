using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POSSystems.Core;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Core.Exceptions;
using POSSystems.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.Plugins.DataProviders.VSS
{
    public class VSSDataProvider : IDataProvider
    {
        private readonly ILogger<SalesController> _logger;

        private readonly string _vssUrl;
        private readonly string _deviceId;
        private readonly string _authorization;

        protected readonly IUnitOfWork _unitOfWork;

        public VSSDataProvider(
            ILogger<SalesController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;

            _vssUrl = _unitOfWork.ConfigurationRepository.GetConfigByKey("vssUrl");
            _deviceId = _unitOfWork.ConfigurationRepository.GetConfigByKey("deviceId");
            _authorization = _unitOfWork.ConfigurationRepository.GetConfigByKey("authorization");
        }

        private HttpRequestMessage GetVssRequest(string url)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            request.Headers.Add("device-id", _deviceId);
            request.Headers.Add("Authorization", _authorization);

            return request;
        }

        public async Task<RxBatchDto> GetRxInfo(string batch)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var request = GetVssRequest($"{_vssUrl}/Prescription/GetBatchPickupItems?batchId={batch}");

                    _logger.LogInformation($"Request sent in GetRxInfo: \n {request}");
                    var response = await client.SendAsync(request);
                    _logger.LogInformation($"Response recieved in GetRxInfo: \n {response}");

                    response.EnsureSuccessStatusCode();
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var rawSales = JsonConvert.DeserializeObject<RxBatchDto>(stringResult);

                    return rawSales;
                }
                catch (HttpRequestException httpRequestException)
                {
                    _logger.LogWarning($"Error from VSSPort for batch {batch}: {httpRequestException.Message}");
                    throw new POSException($"Error from VSSPort for batch {batch}: {httpRequestException.Message}");
                }
            }
        }

        /// <summary>
        /// Get Relation List
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PickerRelationDto>> GetRelationList()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var request = GetVssRequest($"{_vssUrl}/Prescription/GetRelationList");

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var rawSales = JsonConvert.DeserializeObject<IEnumerable<PickerRelationDto>>(stringResult);

                    //if (rawSales.DeliveryStatusId == DeliveryStatus.UNDELIVERED)
                    return rawSales;
                    //else
                    //throw new Exception($"Selected batch is not deliverable");
                }
                catch (HttpRequestException httpRequestException)
                {
                    _logger.LogError($"Error getting relations from VSSPort: {httpRequestException.Message}");
                    throw new Exception($"Error getting relations from VSSPort: {httpRequestException.Message}");
                }
            }
        }

        /// <summary>
        /// Get VerficationId Type List
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PatientIdTypeDto>> GetVerificationIdTypeList()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var request = GetVssRequest($"{_vssUrl}/Prescription/GetVerificationIdTypeList");

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var rawSales = JsonConvert.DeserializeObject<IEnumerable<PatientIdTypeDto>>(stringResult);

                    //if (rawSales.DeliveryStatusId == DeliveryStatus.UNDELIVERED)
                    return rawSales;
                    //else
                    //throw new Exception($"Selected batch is not deliverable");
                }
                catch (HttpRequestException httpRequestException)
                {
                    _logger.LogError($"Error on GetVerficationIdTypeList from VSSPort: {httpRequestException.Message}");
                    throw new Exception($"Error on GetVerficationIdTypeList VSSPort: {httpRequestException.Message}");
                }
            }
        }

        public async Task<IEnumerable<PickerAutoCompleteDto>> GetPickerLookupList(string term)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var request = GetVssRequest($"{_vssUrl}/Prescription/GetPickerLookupList?pickerName=" + term);

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var pickers = JsonConvert.DeserializeObject<IEnumerable<PickerAutoCompleteDto>>(stringResult);

                    return pickers;
                }
                catch (HttpRequestException httpRequestException)
                {
                    _logger.LogError($"Error on GetPickerLookupList from VSSPort: {httpRequestException.Message}");
                    throw new Exception($"Error on GetPickerLookupList VSSPort: {httpRequestException.Message}");
                }
            }
        }

        public async Task UpdateRxInfo(BatchPickupPostDataDto batchPickupPostDataDto)
        {
            using (var client = new HttpClient())
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(batchPickupPostDataDto), Encoding.UTF8, "application/json");

                try
                {
                    batchPickupPostDataDto.DeviceId = _deviceId;
                    client.DefaultRequestHeaders.Add("device-id", _deviceId);
                    client.DefaultRequestHeaders.Add("Authorization", _authorization);

                    _logger.LogInformation($"Request on UpdateBatchPickupItems sent: \n {stringContent}");
                    var response = await client.PostAsync(new Uri($"{_vssUrl}/Prescription/UpdateBatchPickupItems"), stringContent);
                    _logger.LogInformation($"Response recieved from UpdateBatchPickupItems: \n {response}");

                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException httpRequestException)
                {
                    string requestContent = await stringContent.ReadAsStringAsync();
                    throw new POSException($"Error updating VSSPort for batch {batchPickupPostDataDto.BatchId}: {httpRequestException.Message}",
                        $"Error updating VSSPort for batch {batchPickupPostDataDto.BatchId}: {httpRequestException.Message}, request sent: {requestContent}");
                }
            }
        }
    }
}