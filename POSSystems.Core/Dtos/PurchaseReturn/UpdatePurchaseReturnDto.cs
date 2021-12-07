using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.PurchaseReturn
{
    public class UpdatePurchaseReturnDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int PurchaseId { get; set; }

        public int ProductDetailId { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        public double Price { get; set; }

        public double ReturnAmount { get; set; }

        [MaxLength(50)]
        public string BatchId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReturnType { get; set; }

        public int ReasonId { get; set; }

        public string UpcScanCode { get; set; }
    }
}