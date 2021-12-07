using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Return
{
    public class CreateReturnDto
    {
        public double ReturnTotal { get; set; }
        public double TransactionTotal { get; set; }
        public List<PosItemDto> PosItems { get; set; }
        public string InvoiceNo { get; set; }
        public string TransactionType { get; set; }
        public string LoyaltyAmount { get; set; }
    }
}