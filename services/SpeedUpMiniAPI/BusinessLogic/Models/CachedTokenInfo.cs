namespace BusinessLogic.Models
{
    public class CachedTokenInfo
    {
        public DateTime TokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}