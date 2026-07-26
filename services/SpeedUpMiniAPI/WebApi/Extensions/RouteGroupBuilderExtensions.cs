using WebApi.Filters;

namespace WebApi.Extensions;

public static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder MapProtectedGroup(this RouteGroupBuilder group)
    {
        return group.AddEndpointFilter<TokenValidationFilter>();
    }
}
