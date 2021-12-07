using POSSystems.Core.Dtos.PurchaseDetail;
using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Product
{
    public class DeliveryPendingProductDto
    {
        public int PurchaseId { get; set; }

        public IList<PurchaseDetailDto> PurchaseDetails { get; set; }
    }
}