using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.PurchaseReturn
{
    public class CreatePurchaseReturnDto
    {
        [Required]
        public int PurchaseId { get; set; }

        public int ProductDetailId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal ReturnAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReturnType { get; set; }

        public int? ReasonId { get; set; }

        public string UpcScanCode { get; set; }
    }
}