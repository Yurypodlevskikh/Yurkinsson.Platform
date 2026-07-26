namespace YurkinssonAuthentication.DTOs;

public enum ErrorCodes
{
    None = 0,

    // General
    INVALID_MODEL,
    VALIDATION_ERROR,
    UNKNOWN_ERROR,

    // Auth
    INVALID_CREDENTIALS,
    UNAUTHORIZED,
    // Registration
    EMAIL_ALREADY_EXISTS,
    REGISTRATION_FAILED,

    // Email confirmation
    INVALID_TOKEN,
    TOKEN_EXPIRED,
    USER_NOT_FOUND,
    ALREADY_CONFIRMED,
    CONFIRM_FAILED
}
