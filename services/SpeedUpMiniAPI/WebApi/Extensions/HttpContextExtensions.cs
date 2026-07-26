namespace WebApi;

public static class HttpContextExtensions
{
    public static string? GetAccessToken(this HttpContext httpContext) => httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
}
