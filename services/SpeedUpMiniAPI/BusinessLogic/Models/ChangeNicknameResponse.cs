namespace BusinessLogic.Models;

public class ChangeNicknameResponse
{
    public bool IsChanged { get; set; } = false;
    public string ResultMessage { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string NewNickname { get; set; } = string.Empty;
}