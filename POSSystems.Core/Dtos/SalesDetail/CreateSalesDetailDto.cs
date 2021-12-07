using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.SalesDetail
{
    public class CreateSalesDetailDto
    {
        [Required]
        public int SalesId { get; set; }

        public int? ProductId { get; set; }

        [MaxLength(100)]
        public string UpcCode { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public int? Quantity { get; set; }

        public double? Price { get; set; }

        [MaxLength(50)]
        public string SalesProductType { get; set; }

        [MaxLength(150)]
        public string RefPrescriptionId { get; set; }
    }
}