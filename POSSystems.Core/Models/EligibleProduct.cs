using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("eligible_product")]
    public class EligibleProduct : EntityBase
    {
        [Key]
        [StringLength(11)]
        [Column("UPC")]
        public string UPC { get; set; }

        [StringLength(14)]
        [Column("GTIN")]
        public string GTIN { get; set; }

        [StringLength(42)]
        [Column("Description")]
        public string Description { get; set; }

        [StringLength(4)]
        [Column("flc")]
        public string FLC { get; set; }

        [StringLength(100)]
        [Column("category_description")]
        public string CategoryDescription { get; set; }

        [StringLength(100)]
        [Column("sub_category_description")]
        public string SubCategoryDescription { get; set; }

        [StringLength(100)]
        [Column("finest_category_description")]
        public string FinestCategoryDescription { get; set; }

        [StringLength(50)]
        [Column("manufacturer_name")]
        public string ManufacturerName { get; set; }

        [StringLength(10)]
        [Column("change_date")]
        public string ChangeDate { get; set; }

        [StringLength(1)]
        [Column("change_indicator")]
        public string ChangeIndicator { get; set; }
    }
}