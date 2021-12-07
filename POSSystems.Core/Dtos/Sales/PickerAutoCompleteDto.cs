namespace POSSystems.Core.Dtos.Sales
{
    public class PickerAutoCompleteDto
    {
        public string Id => PickerId;

        public string label => PickerName;

        public string value => PickerName;

        public string PickerId { get; set; }
        public string PickerName { get; set; }
        public string Relation { get; set; }
        public string VerificationId { get; set; }
        public string VerificationIdType { get; set; }
        public string VerificationIdDesc { get; set; }
        public string IsDefaultPicker { get; set; }
        public string PickerGroupId { get; set; }
    }
}