using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.RoleClaim
{
    public class UpdateRoleClaimDto
    {
        public int Id { get; set; }

        [Required]
        public string ClaimType { get; set; }

        [Required]
        public string ClaimValue { get; set; }

        public int RoleId { get; set; }
    }
}