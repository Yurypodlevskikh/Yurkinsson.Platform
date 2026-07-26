namespace YurkinssonAuthentication.Models
{
    public class TokenModel
    {
        public string NameIdentifier { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public List<string> UserRoles { get; set; }
        public string Audience { get; set; }
    }
}
