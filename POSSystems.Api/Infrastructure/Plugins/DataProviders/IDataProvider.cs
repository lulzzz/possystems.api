using POSSystems.Core.Dtos.Sales;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.Plugins.DataProviders
{
    public interface IDataProvider
    {
        Task<RxBatchDto> GetRxInfo(string batch);

        Task<IEnumerable<PickerRelationDto>> GetRelationList();

        Task<IEnumerable<PatientIdTypeDto>> GetVerificationIdTypeList();

        Task<IEnumerable<PickerAutoCompleteDto>> GetPickerLookupList(string term);

        Task UpdateRxInfo(BatchPickupPostDataDto batchPickupPostDataDto);
    }
}