using System;

namespace POSSystems.Core.Dtos.Sales
{
    public class RxItemDto
    {
        public string RxNo { get; set; }
        public string RefillNo { get; set; }
        public string PrescribedDrug { get; set; }
        public string DrugClass { get; set; }
        public double? DispensingQuantity { get; set; }
        public string DrugNDC { get; set; }
        public bool IsTaxable { get; set; }
        public double Tax { get; set; }
        public double? Copay { get; set; }
        public DateTime? ProcessTime { get; set; }
        public bool? IsFSA { get; set; }

        public double? OverriddenPrice { get; set; }

        public double RealPrice => OverriddenPrice ?? Copay ?? 0;

        public double TaxAmount
        {
            get
            {
                if (!IsTaxable)
                    return 0;

                return Convert.ToDouble(Math.Round((RealPrice * Tax) / 100, 2, MidpointRounding.AwayFromZero));
            }
        }

        public string RxRefillNo => RxNo + (!string.IsNullOrEmpty(RefillNo) ? RefillNo.PadLeft(2, '0') : string.Empty);

        public DateTime? DispensingDate { get; set; }
    }
}