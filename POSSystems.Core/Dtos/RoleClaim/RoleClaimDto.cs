namespace POSSystems.Core.Dtos.RoleClaim
{
    public class RoleClaimDto : DtoBase
    {
        public int Id { get; set; }

        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }

        public string RoleName { get; set; }

        public int RoleId { get; set; }
    }
}