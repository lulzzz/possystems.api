using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.RoleClaim
{
    public class CreateRoleClaimDto
    {
        public int Id { get; set; }

        [Required]
        public string ClaimType => "Role";

        [Required]
        public string ClaimValue { get; set; }

        public int RoleId { get; set; }
    }
}