using System.Security.Claims;
using System.Security.Principal;
using Dapper;
using lb_aspnetcore_filters.Common;
using lb_aspnetcore_filters.Common.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;

namespace lb_aspnetcore_filters.Filters;

public class AuthenticationFilter(ConnectionPool pool) : IEndpointFilter
{

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var bearerToken = context.HttpContext.Request.Headers.Authorization.FirstOrDefault() ?? "";
        if (bearerToken.StartsWith("Bearer "))
        {
            var token = bearerToken.Split(" ").Last();

            await using var connection = pool.GetConnection();
            await connection.OpenAsync();

            var user = await connection.QuerySingleOrDefaultAsync<User>(
                "SELECT TOP(1) * FROM Users WHERE AccessToken = @Token", new { Token = token });

            if (user is not null)
            {
                context.HttpContext.User = new ClaimsPrincipal(new GenericIdentity(user.Id.ToString()));
                return await next(context);
            }
        }

        return Results.Unauthorized();
    }
}