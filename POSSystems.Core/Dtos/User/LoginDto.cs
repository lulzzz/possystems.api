using POSSystems.Core.Dtos;

namespace POSSystems.Dtos.User
{
    public class UserDto : DtoBase
    {
        public int Id { get; set; }

        public string UserCode { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DisplayName { get; set; }

        public string FullName { get; set; }

        public string Designation { get; set; }

        public string ContactNo { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string Barcode { get; set; }
    }
}