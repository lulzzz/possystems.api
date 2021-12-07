using System;

namespace POSSystems.Core.Dtos.Product
{
    public class ProductPurchaseDetailDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int Id { get; set; }

        public int CurStock { get; set; }

        public int MeasurementId { get; set; }

        public double? PurchasePrice { get; set; }

        public double SalesPrice { get; set; }

        public int SupplierId { get; set; }

        public bool TaxInd { get; set; }

        public string TaxIndStr => TaxInd.ToString();

        public string Supplier { get; set; }

        public string MeasurementUnit { get; set; }

        public string UpcCode { get; set; }

        public string UpcScanCode { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        public double? PackageSize { get; set; }

        public string Strength { get; set; }

        public double? MinStock { get; set; }

        public double? MaxStock { get; set; }

        public double? ReorderLevel { get; set; }

        public string ItemNo { get; set; }
    }
}