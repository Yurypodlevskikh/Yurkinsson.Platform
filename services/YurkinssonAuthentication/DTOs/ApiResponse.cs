using System.Text.Json.Serialization;

namespace YurkinssonAuthentication.DTOs
{
    /// <summary>
    /// Standard API response for identity endpoints.
    /// Contains a safe message for clients and optional developer-only details.
    /// Includes optional UserId to help the frontend request resends when token is expired.
    /// </summary>
    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Generalized, non-sensitive message safe for clients/proxies.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Machine-friendly error code (TOKEN_EXPIRED, INVALID_TOKEN, etc.).
        /// </summary>
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Optional validation errors for debugging.
        /// </summary>
        [JsonPropertyName("validationErrors")]
        public string[]? ValidationErrors { get; set; }

        /// <summary>
        /// Developer-only diagnostics shown only in Development.
        /// </summary>
        [JsonPropertyName("developerMessage")]
        public string? DeveloperMessage { get; set; }

        /// <summary>
        /// Optional user id (safe to return) so the frontend can request resend when token expired.
        /// </summary>
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }
}
