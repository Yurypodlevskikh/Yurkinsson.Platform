namespace BusinessLogic.DTOs;

public enum ErrorCodes
{
    None,
    UserNotFound,
    InvalidToken,
    TokenExpired,
    AlreadyConfirmed,
    ConfirmFailed
}
