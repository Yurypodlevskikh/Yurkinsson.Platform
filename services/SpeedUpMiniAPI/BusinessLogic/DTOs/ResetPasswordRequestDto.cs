using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs;

public class ResetPasswordRequestdDto
{
    [Required]
    public string? Password { get; set; }
    [Required]

    public string? Email { get; set; }
    [Required]
    public string? Token { get; set; }
}