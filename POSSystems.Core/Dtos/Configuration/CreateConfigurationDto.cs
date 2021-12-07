using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Configuration
{
    public class CreateConfigurationDto : DtoBase
    {
        [Required]
        public string ConfigCode { get; set; }

        [Required]
        public string ConfigValue { get; set; }

        public string Description { get; set; }
    }
}