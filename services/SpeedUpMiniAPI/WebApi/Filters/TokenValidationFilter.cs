using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLogic.Interfaces;

namespace WebApi.Filters;

public class TokenValidationFilter : IEndpointFilter
{
    private readonly IIdentityApiService _identityApiService;

    public TokenValidationFilter(IIdentityApiService identityApiService)
    {
        _identityApiService = identityApiService;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        var accessTokenHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(accessTokenHeader) || !accessTokenHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Unauthorized();
        }

        var accessToken = accessTokenHeader["Bearer ".Length..].Trim();
        var isSignedIn = await _identityApiService.IsSignedInAsync(accessToken, httpContext.RequestAborted);

        if (isSignedIn == null || !isSignedIn.SignedIn)
        {
            return Results.Unauthorized();
        }

        // Here you can decode the JWT token and attach user information to the HttpContext
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(accessToken) as JwtSecurityToken;
        
        if(jwtToken == null) return Results.Unauthorized();

        var userId = _identityApiService.GetUserGuidId(jwtToken);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        httpContext.Items["UserId"] = userId;

        // var userNickName = _identityApiService.GetUserNickName(jwtToken);

        // if (!string.IsNullOrEmpty(userNickName)) httpContext.Items["UserNickName"] = userNickName;

        return await next(context);
    }
}
