namespace YurkinssonAuthentication.Models
{
    public class TokenRequest
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; internal set; }
    }
}
