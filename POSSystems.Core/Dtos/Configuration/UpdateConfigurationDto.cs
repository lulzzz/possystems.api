using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Configuration
{
    public class UpdateConfigurationDto
    {
        [Required]
        public string ConfigValue { get; set; }
    }

    public class UpdateConfigurationDtoContainer
    {
        public bool VssIntegrated { get; set; }
        public string VssUrl { get; set; }
        public string DeviceId { get; set; }
        public string Authorization { get; set; }
        public string SigisInterval { get; set; }
        public string Edi832Interval { get; set; }
        public string Edi855Interval { get; set; }
        public string ProfitMargin { get; set; }
        public string TaxPercentage { get; set; }
        public bool FollowMarkup { get; set; }
        public bool LoyaltyEnabled { get; set; }
        public bool TrancloudEnabled { get; set; }
        public bool CreditCardLikeCash { get; set; }
        public bool RxSignatureNeeded { get; set; }
        public bool PrintOnlyRx { get; set; }
        public string InitialPointReward { get; set; }
        public string RedeemThresholdPoint { get; set; }
        public string DollarPointConversionRatio { get; set; }
        public string PointDollarConversionRatio { get; set; }
        public string PrintCopy { get; set; }
        public string Timezone { get; set; }
    }
}