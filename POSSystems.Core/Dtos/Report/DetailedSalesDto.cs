using System;

namespace POSSystems.Core.Dtos.Report
{
    public class DetailedSalesDto : DtoBase
    {
        public DateTime? SalesDate { get; set; }
        public string ProductName { get; set; }
        public string UPCCode { get; set; }
        public Int32? Quantity { get; set; }
        public double? Price { get; set; }
        public double? Discount { get; set; }
        public double? SalesPrice { get; set; }
        public string SupplierName { get; set; }
    }
}