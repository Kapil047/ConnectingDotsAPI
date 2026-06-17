using ConnectingDotsAPI.DBModels;
using System.ComponentModel.DataAnnotations;

namespace ConnectingDotsAPI.Models
{
    public class UserModel
    {
        public class IUserSaveRequest
        {
            public string? Guid { get; set; }
            [Required]
            [MaxLength(200)]
            public required string FirstName { get; set; }
            [MaxLength(200)]
            public string? LastName { get; set; }
            [MaxLength(50)]
            public string? Password { get; set; }
            [Required]
            public string UserName { get; set; } = string.Empty;
            public bool Active { get; set; }
            [MaxLength(1000)]
            public string? AdminComment { get; set; }
            public string? ParentUserId { get; set; }
        }
        public class UpdateRoleRequest
        {
            [Required]
            public required string UserId { get; set; }
            [Required]
            public int RoleId { get; set; }
            [Required]
            public MethodType Action { get; set; }
        }
        public class UpdateRoleAssigment
        {
            [Required]
            public required int ParentRoleId { get; set; }
            [Required]
            public required int RoleId { get; set; }
            [Required]
            public MethodType Action { get; set; }
        }

        public class UserDetails
        {
            public string FirstName { get; set; } = null!;
            public string? LastName { get; set; }
            public string Username { get; set; } = null!;
            public bool Active { get; set; }
            public Guid? Guid { get; set; }
            public object? ActivityLogs { get; set; }
            public object? UserAuthTokens { get; set; }
            public object? Passwords { get; set; }
            public object? Roles { get; set; }
            public Guid? ParentUserGuid { get; set; }
            public string? ParentUser { get; set; }
        }

        public enum MethodType
        {
            insert, delete, edit
        }
        public class RoleDetails
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? SystemName { get; set; }
            public bool Active { get; set; }
            public bool IsSystemRole { get; set; }
            public bool EnablePasswordLifetime { get; set; }
            public object? Pages { get; set; }
            public object? Roles { get; set; }
        }
        public class SaveRoleRequest
        {
            public int? Id { get; set; }
            public required string Name { get; set; }
            public required string SystemName { get; set; }
            public bool IsSystemRole { get; set; } = false;
        }
    }
}
