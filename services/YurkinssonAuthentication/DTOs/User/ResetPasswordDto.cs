using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.DTOs.User;

public class ResetPasswordDto
{
    [Required]
    public string? Password { get; set; }
    //[Compare("Password")]
    //public string? ConfirmPassword { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Token { get; set; }
}
