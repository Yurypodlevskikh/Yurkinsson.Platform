using YurkinssonAuthentication.DTOs;

namespace YurkinssonAuthentication.Models
{
    public class EmailConfirmationResult
    {
        public bool Succeeded { get; set; }
        public bool CanResend { get; set; }
        public ErrorCodes ErrorCode { get; set; } = ErrorCodes.None;
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }
}
