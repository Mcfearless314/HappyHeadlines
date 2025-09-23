using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace CommentService.Infrastructure;

public class DbProvider
{
    private DbConnection? _connection;
    private string _connectionString;
    
    public DbProvider(string connectionString)
    {
        _connectionString = connectionString;
    }
    public DbConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
        }

        return _connection;
    }
}
