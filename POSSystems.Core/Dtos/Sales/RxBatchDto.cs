using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Sales
{
    public class RxBatchDto
    {
        public string BatchId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? PendingBatchCount { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Zipcode { get; set; }
        public string HomePhone { get; set; }
        public string Fax { get; set; }
        public string VerificationId { get; set; }
        public string VerificationIdType { get; set; }
        public string VerificationIdDesc { get; set; }
        public bool? HasHIPAAReceived { get; set; }
        public string DeliveryStatusId { get; set; }
        public string PickerGroupId { get; set; }
        public IList<PickerDto> PickerList { get; set; }
        public IList<RxFamilyMemberDto> FamilyList { get; set; }
    }
}