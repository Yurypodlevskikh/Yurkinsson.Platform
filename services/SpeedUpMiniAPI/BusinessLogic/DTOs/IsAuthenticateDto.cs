namespace BusinessLogic.DTOs;

public class IsAuthenticateDto
{
    public bool SignedIn { get; set; }
    public string? RefreshToken { get; set; } = null!;
}