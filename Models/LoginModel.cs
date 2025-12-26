using System.ComponentModel.DataAnnotations;

namespace ConnectingDotsAPI.Models.Auth
{
    public class LoginModel
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required AuthModel.LoginType Type { get; set; }
    }
  
}
