using Dapper;
using lb_aspnetcore_filters.Common;
using lb_aspnetcore_filters.Common.Models;

namespace lb_aspnetcore_filters.Filters;

public class UserValidatorFilter(ConnectionPool pool) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.GetArgument<User?>(1);
        if (user != null)
        {
            var connection = pool.GetConnection();
            await connection.OpenAsync();
            
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE AccessToken = @Token", new {Token = user.AccessToken});

            if (count > 0)
            {
                return Results.BadRequest("A user with that access token already exists.");
            }
            
            return await next(context);
        }

        return Results.BadRequest("Invalid body");
    }
    
    
}