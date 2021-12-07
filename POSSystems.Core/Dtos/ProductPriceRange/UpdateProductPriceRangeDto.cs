using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos
{
    public class UpdateProductPriceRangeDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string TableName { get; set; }

        [Required]
        [MaxLength(10)]
        public string CostPreference { get; set; }
    }
}