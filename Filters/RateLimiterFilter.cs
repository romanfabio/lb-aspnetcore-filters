using Dapper;
using lb_aspnetcore_filters.Common;
using lb_aspnetcore_filters.Common.Models;

namespace lb_aspnetcore_filters.Filters;

public class RateLimiterFilter(ConnectionPool pool) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        var userId = context.HttpContext.User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }
        
        await using var connection = pool.GetConnection();
        await connection.OpenAsync();

        var user =
            await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = userId });

        if (user == null)
        {
            return Results.Unauthorized();
        }

        if ((DateTime.UtcNow - user.LastRequest).TotalSeconds < 10)
        {
            return Results.StatusCode(429);
        }
        
        await connection.ExecuteAsync("UPDATE Users SET LastRequest = CURRENT_TIMESTAMP WHERE Id = @Id", new { Id = user.Id });

        return await next(context);
    }
}