using System;

namespace POSSystems.Core.Dtos.Sales
{
    public class ProductDto : DtoBase
    {
        public int ProductId { get; set; }

        public string Category { get; set; }

        public string Product { get; set; }

        public int Id { get; set; }

        public int Quantity { get; set; }

        public int MeasurementId { get; set; }

        public double? PurchasePrice { get; set; }

        public double SalesPrice { get; set; }

        public int SupplierId { get; set; }

        public bool TaxInd { get; set; }

        public string Supplier { get; set; }

        public string MeasurementUnit { get; set; }

        public string UpcCode { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        public bool? IsFSA { get; set; }

        public double TaxAmount { get; set; }

        public bool TaxIndicator { get; set; }
    }
}