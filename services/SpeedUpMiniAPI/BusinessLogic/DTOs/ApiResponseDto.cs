using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs;

/// <summary>
/// Public response contract returned to frontend clients.
/// Contains only safe, user-facing information. 
/// No sensitive details or internal error messages should be included here.
/// </summary>
public class ApiResponseDto
{
    public bool Success { get; set; }
    /// <summary>
    /// Generalized, non-sensitive message safe for clients.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public string? ErrorCode { get; set; }

    public bool? CanResend { get; set; }

    public static ApiResponseDto Ok(string message)=>
        new () { Success = true, Message = message };

    public static ApiResponseDto Fail(string message, string? errorCode = null, bool? canResend = null) =>
        new() { Success = false, Message = message, ErrorCode = errorCode, CanResend = canResend };
}
