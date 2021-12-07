using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos
{
    public class CreateProductPriceRangeDto
    {
        [Required]
        [MaxLength(20)]
        public string TableName { get; set; }

        [Required]
        [MaxLength(10)]
        public string CostPreference { get; set; }
    }
}