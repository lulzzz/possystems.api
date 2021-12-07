﻿using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.UserRole
{
    public class UpdateUserRoleDto
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}