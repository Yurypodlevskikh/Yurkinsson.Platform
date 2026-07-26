using Microsoft.AspNetCore.Identity;

namespace YurkinssonAuthentication.Models
{
    public class TokenResult
    {
        public SignInResult SignedIn { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; internal set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
