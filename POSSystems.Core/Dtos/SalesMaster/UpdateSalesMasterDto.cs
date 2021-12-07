using System;
using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.SalesMaster
{
    public class UpdateSalesMasterDto
    {
        [Required]
        public int? SalesId { get; set; }

        public int? CustomerId { get; set; }

        [MaxLength(50)]
        public string PayMethod { get; set; }

        public DateTime? SalesDate { get; set; }

        public double? TotalDiscount { get; set; }

        public double? GrandTotal { get; set; }

        public double? Payment { get; set; }

        public double? Due { get; set; }

        public double? SalesTax { get; set; }

        //public int? TerminalId { get; set; }

        [MaxLength(50)]
        public string DrivingLicenseNo { get; set; }
    }
}