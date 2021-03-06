using System;
using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Product
{
    public class UpdateProductDto
    {
        [Required]
        public int id { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int MeasurementId { get; set; }

        public double? PurchasePrice { get; set; }

        public double SalesPrice { get; set; }

        public int SupplierId { get; set; }

        public bool TaxInd { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string UpcCode { get; set; }

        public string UpcScanCode { get; set; }

        public double? PackageSize { get; set; }

        public string Strength { get; set; }

        public string ItemNo { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public string CatalogProductId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? MinStock { get; set; }

        public int? MaxStock { get; set; }

        public int? ReorderLevel { get; set; }

        public int? ProductPriceRangeId { get; set; }

        public int? ManufacturerId { get; set; }
    }
}