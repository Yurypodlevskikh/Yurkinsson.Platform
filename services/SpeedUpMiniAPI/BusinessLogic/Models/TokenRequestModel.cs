namespace BusinessLogic.Models;

public class TokenRequestModel
{
    public string JwtToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}