using Microsoft.Data.SqlClient;

namespace PublisherService.Infrastructure;

public class DbProvider
{
    private readonly string _connectionString;

    public DbProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}