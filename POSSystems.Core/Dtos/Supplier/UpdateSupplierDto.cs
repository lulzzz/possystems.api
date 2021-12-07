using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Supplier
{
    public class UpdateSupplierDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SupplierName { get; set; }

        [MaxLength(200)]
        public string Address1 { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(50)]
        public string Website { get; set; }

        [MaxLength(50)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string ContactPerson { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        [MaxLength(200)]
        public string Address2 { get; set; }

        [MaxLength(50)]
        public string State { get; set; }

        [MaxLength(50)]
        public string Zip { get; set; }

        public string Fax { get; set; }
    }
}