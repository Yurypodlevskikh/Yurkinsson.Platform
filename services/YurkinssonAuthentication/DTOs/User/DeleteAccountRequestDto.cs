using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.DTOs.User
{
    public class DeleteAccountRequestDto
    {
        [Required]
        public string? Password { get; set; }
    }
}