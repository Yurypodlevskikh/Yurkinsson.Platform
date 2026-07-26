namespace BusinessLogic.Models;

public class AuthenticateResponse
{
    public string JwtToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiry { get; set; }
}