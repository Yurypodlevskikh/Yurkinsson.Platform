using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Models;

public class NicknameModel
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Nickname can only contain letters, numbers, and underscores.")]
    public required string Nickname { get; set; }
}