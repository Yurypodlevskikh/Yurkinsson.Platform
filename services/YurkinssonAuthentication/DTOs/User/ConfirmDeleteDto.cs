using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.DTOs.User;

public class ConfirmDeleteDto
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? Token { get; set; }
}