using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Report
{
    public class SalesEndDto
    {
        public double Total { get; set; } = 0;
        public double TotalTax { get; set; } = 0;
        public int TotalPointsEarned { get; set; } = 0;
        public int TotalPointsRedeemed { get; set; } = 0;
        public double TotalDiscount { get; set; } = 0;
        public int TotalCustomer { get; set; } = 0;

        public double Sales { get; set; } = 0;
        public double Return { get; set; } = 0;
        public double Cash { get; set; } = 0;
        public double Credit { get; set; } = 0;
        public double Debit { get; set; } = 0;
        public double Check { get; set; } = 0;
        public double Other { get; set; } = 0;
        public double Bank { get; set; } = 0;
        public double Card { get; set; } = 0;

        public double MasterCard { get; set; } = 0;
        public double VisaCard { get; set; } = 0;

        public IList<SalesEndKeyValDto> CategoryPrices { get; set; }
    }

    public class SalesEndKeyValDto
    {
        public string K { get; set; }
        public double V { get; set; }
    }
}