using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs;

public class ForgotPasswordRequestDto
{
    public string? Email { get; set; }
    public string? ClientUri { get; set; }
}