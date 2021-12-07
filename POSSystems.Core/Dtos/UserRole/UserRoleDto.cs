namespace POSSystems.Core.Dtos.UserRole
{
    public class UserRoleDto : DtoBase
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int RoleId { get; set; }

        public string UserName { get; set; }
        public string RoleName { get; set; }
    }
}