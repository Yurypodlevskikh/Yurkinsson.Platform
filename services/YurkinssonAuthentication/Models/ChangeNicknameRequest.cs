namespace YurkinssonAuthentication.Models
{
    public class ChangeNicknameRequest
    {
        public required string UserId { get; set; }
        public string? NewNickname { get; set; }
    }
}
