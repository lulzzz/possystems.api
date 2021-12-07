using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSSystems.Dtos.User
{
    public class UpdateUserDto
    {
        public int Id { get; set; }

        [Required]
        public string Password { get; set; }

        [Required, DisplayName("Display Name")]
        public string DisplayName { get; set; }

        [DisplayName("Full Name")]
        public string FullName { get; set; }

        public string Designation { get; set; }

        [DisplayName("Contact No.")]
        public string ContactNo { get; set; }

        [Required]
        public string Email { get; set; }

        public string Address { get; set; }
        public string Barcode { get; set; }
    }
}