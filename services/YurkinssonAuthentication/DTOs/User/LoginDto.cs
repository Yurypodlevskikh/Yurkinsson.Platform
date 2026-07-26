using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.DTOs.User
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string Audience { get; set; }
    }
}
