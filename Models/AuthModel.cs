using System.ComponentModel.DataAnnotations;

namespace ConnectingDotsAPI.Models
{
    public class AuthModel
    {
        public class JwtCustomerDetails
        {
            public required string Id { get; set; }
            public required string Email { get; set; }
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
        }
        public class AuthResult
        {
            public string? Token { get; set; }
            public bool Result { get; set; } = false;
            public string? Message { get; set; }
        }
        public class JwtUserDetails
        {
            public required string Id { get; set; }
            public required string Guid { get; set; }
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public object? Roles { get; set; }
        }
        public class UpdatePasswordRequest
        {
            [Required]
            public required string Id { get; set; }
            [Required]
            public required string Password { get; set; }
        }
        public class IValidTokenRequest
        {
            [Required]
            public required string Token { get; set; }
            [Required]
            public LoginType Type { get; set; }
        }
        public enum LoginType
        {
            Admin, Customer
        }
        public class RegisterRequest
        {
            [Required]
            public string Username { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MinLength(6)]
            public string Password { get; set; } = string.Empty;

            [Required]
            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;
        }
    }
}
