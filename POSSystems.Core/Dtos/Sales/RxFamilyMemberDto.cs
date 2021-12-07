using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Sales
{
    public class RxFamilyMemberDto
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Birthdate { get; set; }
        public bool? HasEasyTwistSelected { get; set; }
        public IList<RxItemDto> RxList { get; set; }
    }
}