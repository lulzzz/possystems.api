using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.PriceRange
{
    public class UpdatePriceRangeDto
    {
        public int Id { get; set; }

        [Required]
        public int ProductPriceRangeId { get; set; }

        [Required]
        public double MinRange { get; set; }

        [Required]
        public double MaxRange { get; set; }

        [Required]
        public double Markup { get; set; }
    }
}