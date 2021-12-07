using System;

namespace POSSystems.Core.Dtos.Return
{
    public class ReturnDto
    {
        public string Date => DateTime.Today.ToString("MM/dd/yyyy");

        public bool EnableLoyalty { get; set; } = false;
        public bool CreditCardLikeCash { get; set; } = false;
    }
}