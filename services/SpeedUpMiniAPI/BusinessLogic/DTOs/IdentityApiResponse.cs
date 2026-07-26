using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs;

/// <summary>
/// Internal DTO used to deserialize responses from Identity service.
/// </summary>
public class IdentityApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string[]? ValidationErrors { get; set; }
    public string? DeveloperMessage { get; set; }
}
