using System;

namespace POSSystems.Core.Dtos.Product
{
    public class ProductDto : DtoBase
    {
        public int ProductId { get; set; }

        public int Id { get; set; }

        public int? Quantity { get; set; }

        public int MeasurementId { get; set; }

        public double? PurchasePrice { get; set; }

        public double SalesPrice { get; set; }

        public int SupplierId { get; set; }

        public bool TaxInd { get; set; }

        public bool TaxIndicator { get; set; }

        public string CategoryName { get; set; }

        public string TaxIndStr => TaxIndicator.ToString();

        public string Supplier { get; set; }

        public string MeasurementUnit { get; set; }

        public string UpcCode { get; set; }

        public string UpcScanCode { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        public double? PackageSize { get; set; }

        public string Strength { get; set; }

        public string ItemNo { get; set; }

        public string Description { get; set; }

        public string ProductName { get; set; }

        public int CategoryId { get; set; }

        public int? MinStock { get; set; }

        public int? MaxStock { get; set; }

        public int? ReorderLevel { get; set; }

        public int? ProductPriceRangeId { get; set; }

        public string TableName { get; set; }

        public int? ManufacturerId { get; set; }

        public string Manufacturer { get; set; }
    }
}