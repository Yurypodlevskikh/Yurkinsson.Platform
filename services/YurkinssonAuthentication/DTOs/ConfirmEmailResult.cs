namespace YurkinssonAuthentication.DTOs;

public class ConfirmEmailResult
{
    public bool Succeeded { get; set; }
    public ErrorCodes ErrorCode { get; set; } = ErrorCodes.None;
    public string? ErrorMessage { get; set; }
    public bool CanResend { get; set; }
}
