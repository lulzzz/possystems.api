using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.SalesReturn
{
    public class UpdateSalesReturnDto
    {
        public int Id { get; set; }

        public int SalesId { get; set; }

        public int ProductDetailId { get; set; }

        [Required]
        [MaxLength(100)]
        public string UpcCode { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public double Price { get; set; }

        public double? ReturnAmount { get; set; }

        [MaxLength(150)]
        public string RefPrescriptionId { get; set; }

        [MaxLength(50)]
        public string SalesProductType { get; set; }

        [MaxLength(50)]
        public string ReturnType { get; set; }

        public int? ReasonId { get; set; }

        public string InvoiceNo { get; set; }
    }
}