namespace BusinessLogic.Models;

public class ChangeNicknameRequest
{
    public string AccessToken {get; set;} = string.Empty;
    public string Nickname { get; set; } = string.Empty;
}