using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("supplier")]
    public class Supplier : EntityBase
    {
        [Key]
        [Required]
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("supplier_name")]
        public string SupplierName { get; set; }

        [StringLength(200)]
        [Column("address1")]
        public string Address1 { get; set; }

        [StringLength(50)]
        [Column("phone")]
        public string Phone { get; set; }

        [StringLength(10)]
        [Column("website")]
        public string Website { get; set; }

        [StringLength(50)]
        [Column("country")]
        public string Country { get; set; }

        [StringLength(50)]
        [Column("contact_person")]
        public string ContactPerson { get; set; }

        [StringLength(50)]
        [Column("email")]
        public string Email { get; set; }

        [StringLength(50)]
        [Column("city")]
        public string City { get; set; }

        [StringLength(200)]
        [Column("address2")]
        public string Address2 { get; set; }

        [StringLength(50)]
        [Column("state")]
        public string State { get; set; }

        [StringLength(50)]
        [Column("zip")]
        public string Zip { get; set; }

        [StringLength(50)]
        [Column("fax")]
        public string Fax { get; set; }
    }
}