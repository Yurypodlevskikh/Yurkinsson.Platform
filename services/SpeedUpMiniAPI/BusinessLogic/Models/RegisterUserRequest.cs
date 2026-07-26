using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Models;

public class RegisterUserRequest
{
    [Required]
    [StringLength(256)]
    public string? Nickname { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [MinLength(6)]
    public string? Password { get; set; }
}