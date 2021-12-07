namespace POSSystems.Core.Dtos.Product
{
    public class ProductStockDto
    {
        public string UpcCode { get; set; }

        public string ProductName { get; set; }

        public string CategoryName { get; set; }

        public int? Quantity { get; set; }

        public double? PurchasePrice { get; set; }

        public double SalesPrice { get; set; }

        public string MeasurementUnit { get; set; }

        public int? PackageSize { get; set; }
    }
}