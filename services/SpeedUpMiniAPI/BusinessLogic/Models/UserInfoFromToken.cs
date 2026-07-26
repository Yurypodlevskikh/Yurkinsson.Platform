namespace BusinessLogic.Models
{
    public class UserInfoFromTokenDto
    {
        public string GuidId { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
        public string Audience { get; set; } = string.Empty;
    }
}