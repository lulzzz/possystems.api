using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.PurchaseDetail
{
    public class UpdatePurchaseDetailDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int PurchaseId { get; set; }

        [Required]
        public string UpcScanCode { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        public double? Price { get; set; }
    }
}