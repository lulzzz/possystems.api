using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("product_pricerange")]
    public class ProductPriceRange : EntityBase
    {
        [Key]
        [Column("product_pricerange_id")]
        public int ProductPriceRangeId { get; set; }

        [Column("table_name")]
        public string TableName { get; set; }

        [Column("cost_preference")]
        public string CostPreference { get; set; }
    }
}