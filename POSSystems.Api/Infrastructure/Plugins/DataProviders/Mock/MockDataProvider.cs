using POSSystems.Core.Dtos.Sales;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.Plugins.DataProviders.VSS
{
    public class MockDataProvider : IDataProvider
    {
        public async Task<RxBatchDto> GetRxInfo(string batch)
        {
            var rxBatchDtoList = new List<RxBatchDto>();
            var rxBatchDto = new RxBatchDto
            {
                BatchId = "123",
                CustomerName = "RxMan",
                CustomerId = "123",
                AddressLine1 = "haha",
                FamilyList = new List<RxFamilyMemberDto>()
            };

            var rxFamilyMemberDto = new RxFamilyMemberDto
            {
                CustomerId = "123",
                CustomerName = "Test Man",
                Birthdate = "11/23/1019",
                RxList = new List<RxItemDto>()
            };

            var rxItemDto = new RxItemDto
            {
                RxNo = "145",
                Copay = 20,
                DispensingQuantity = 3,
                DrugClass = "C2",
                DrugNDC = "15678",
                RefillNo = "34"
            };

            rxFamilyMemberDto.RxList.Add(rxItemDto);
            rxBatchDto.FamilyList.Add(rxFamilyMemberDto);
            rxBatchDtoList.Add(rxBatchDto);

            return rxBatchDtoList.FirstOrDefault(r => r.BatchId == batch);
        }

        /// <summary>
        /// Get Relation List
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PickerRelationDto>> GetRelationList()
        {
            var pickerRelationListDto = new List<PickerRelationDto>();
            pickerRelationListDto.Add(new PickerRelationDto { RelationId = 1, RelationDesc = "Test" });

            return pickerRelationListDto;
        }

        /// <summary>
        /// Get VerficationId Type List
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PatientIdTypeDto>> GetVerificationIdTypeList()
        {
            var patientIdTypes = new List<PatientIdTypeDto>();
            patientIdTypes.Add(new PatientIdTypeDto { PatientIdType = "Test Patient", PatientIdTypeDesc = "Test Test" });

            return patientIdTypes;
        }

        public async Task<IEnumerable<PickerAutoCompleteDto>> GetPickerLookupList(string term)
        {
            return null;
        }

        public async Task UpdateRxInfo(BatchPickupPostDataDto batchPickupPostDataDto)
        {
        }
    }
}