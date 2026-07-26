using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.Data
{
    public class AppUser : IdentityUser
    {
        [StringLength(256)]
        public string? Nickname { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLoginTime { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
