using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("product_category")]
    public class ProductCategory : EntityBase
    {
        [Key]
        [Column("category_id")]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("category_name")]
        public string CategoryName { get; set; }

        [StringLength(255)]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("tax_ind")]
        public bool TaxInd { get; set; }

        [Required]
        [Column("signature_required")]
        public bool SignatureReq { get; set; }
    }
}