using Dapper;
using lb_aspnetcore_filters.Common;

namespace lb_aspnetcore_filters.Filters;

public class RequestLoggerFilter(ConnectionPool pool) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        await using var connection = pool.GetConnection();
        await connection.OpenAsync();
        
        var requestId = Guid.NewGuid();

        await connection.ExecuteAsync("INSERT INTO Logs VALUES (CURRENT_TIMESTAMP, @Message)", new
        {
            Message = $"Request {requestId} - {context.HttpContext.Request.Method} {context.HttpContext.Request.Path} - User ID {context.HttpContext.User?.Identity?.Name}"
        });

        var result = await next(context);
        
        await connection.ExecuteAsync("INSERT INTO Logs VALUES (CURRENT_TIMESTAMP, @Message)", new
        {
            Message = $"Request {requestId} - {context.HttpContext.Response.StatusCode}"
        });

        return result;
    }
}