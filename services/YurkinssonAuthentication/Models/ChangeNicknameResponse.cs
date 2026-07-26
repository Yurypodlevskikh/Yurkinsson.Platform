using Microsoft.AspNetCore.Identity;

namespace YurkinssonAuthentication.Models
{
    public class ChangeNicknameResponse
    {
        public bool IsChanged { get; set; } = false;
        public string NewNickname { get; set; } = string.Empty;
    }
}
