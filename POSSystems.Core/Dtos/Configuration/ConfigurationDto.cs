namespace POSSystems.Core.Dtos.Configuration
{
    public class ConfigurationDto : DtoBase
    {
        public int Id { get; set; }
        public string ConfigCode { get; set; }
        public string ConfigValue { get; set; }
        public string Description { get; set; }
    }
}