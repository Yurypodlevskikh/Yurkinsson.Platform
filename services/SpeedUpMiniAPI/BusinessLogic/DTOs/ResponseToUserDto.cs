namespace BusinessLogic.DTOs
{
    public class ResponseToUserDto
    {
        public string JwtToken { get; set; } = null!;
        public string? RefreshToken { get; set; } = null!;
        public int HueDegrees { get; set; }
        public string Message { get; set; } = null!;
        public bool IsSuccess { get; set; } = false;
        public string? Nickname { get; set; }
    }
}