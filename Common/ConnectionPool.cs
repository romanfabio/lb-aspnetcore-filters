using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace lb_aspnetcore_filters.Common;

public class ConnectionPool(IOptions<ConnectionPoolOptions> options)
{
    private readonly ConnectionPoolOptions _settings = options.Value;

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_settings.ConnectionString);
    }
}