using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.PriceRange
{
    public class CreatePriceRangeDto
    {
        public int ProductPriceRangeId { get; set; }

        [Required]
        public double MaxRange { get; set; }

        [Required]
        public double MinRange { get; set; }

        [Required]
        public double Markup { get; set; }
    }
}