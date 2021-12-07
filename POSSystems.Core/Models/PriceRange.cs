using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("pricerange")]
    public class PriceRange : EntityBase
    {
        [Key]
        [Column("pricerange_id")]
        public int PriceRangeId { get; set; }

        [Column("product_pricerange_id")]
        public int ProductPriceRangeId { get; set; }

        [Column("minrange")]
        public double? MinRange { get; set; }

        [Column("maxrange")]
        public double? MaxRange { get; set; }

        [Column("markup")]
        public double Markup { get; set; }

        [ForeignKey("ProductPriceRangeId")]
        public virtual ProductPriceRange ProductPriceRange { get; set; }
    }
}