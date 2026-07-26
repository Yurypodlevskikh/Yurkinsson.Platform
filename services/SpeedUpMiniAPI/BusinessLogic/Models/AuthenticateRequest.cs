using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Models;

public class AuthenticateRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; } = false;
    public string? Audience { get; set; }
}