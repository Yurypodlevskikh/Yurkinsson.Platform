using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.DTOs.User;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? ClientUri { get; set; }
}
