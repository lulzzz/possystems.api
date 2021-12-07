using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Sales
{
    public class BatchPickupPostDataDto
    {
        public string DeviceId { get; set; }
        public string BatchId { get; set; }
        public List<string> RxList { get; set; }
        public string PickerMode { get; set; }
        public string PickerId { get; set; }
        public string PickerName { get; set; }
        public string RelationId { get; set; }
        public string VerificationId { get; set; }
        public string VerificationIdType { get; set; }
        public string IdIssueState { get; set; }
        public bool HasHIPAAReceived { get; set; }
        public bool HasCounseled { get; set; }
        public string SignatureData { get; set; }
    }
}