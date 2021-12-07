using System.ComponentModel;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class JobConfig
    {
        public string Sigis { get; set; }
        public string Edi832 { get; set; }
        public string Edi855 { get; set; }
        public string RetailInsights { get; set; }
    }

    public enum JobConfigType
    {
        [Description("Test")]
        Test = 1,

        [Description("Prod")]
        Prod = 2,

        [Description("Dev")]
        Dev = 3,

        [Description("Off")]
        Off = 4
    }
}