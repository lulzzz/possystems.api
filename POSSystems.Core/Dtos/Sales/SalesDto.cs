using System;
using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Sales
{
    public class SalesDto
    {
        public string Date => DateTime.Today.ToString("MM/dd/yyyy");

        public double TaxPercentage { get; set; }

        //public string InvoiceNo { get; set; }
        public IEnumerable<PickerRelationDto> PickerRelations { get; set; }

        public IEnumerable<PatientIdTypeDto> PatientIdTypes { get; set; }
        public bool VssIntegrated { get; set; } = true;
        public bool TrancloudEnabled { get; set; } = true;
        public bool EnableLoyalty { get; set; } = false;
        public bool VerifyMode { get; set; } = true;
        public bool OverrideAuthorized { get; set; } = false;
        public bool CreditCardLikeCash { get; set; } = false;
    }
}