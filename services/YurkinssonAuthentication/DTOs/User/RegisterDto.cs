using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.DTOs.User
{
    public class RegisterDto
    {
        [Required]
        [StringLength(256)]
        public string Nickname { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        [StringLength(256)]
        /// <summary>
        /// The identifier or domain of the client application requesting registration.
        /// </summary>
        public string ClientApp {  get; set; }
    }
}
