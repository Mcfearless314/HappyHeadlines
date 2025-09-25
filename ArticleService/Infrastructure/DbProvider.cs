using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace ArticleService.Infrastructure;

public class DbProvider
{
    private DbConnection? _connection;

    public DbConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new SqlConnection("Server=article-db;User Id=sa;Password=SuperSecret7!;TrustServerCertificate=True;");
            _connection.Open();
        }

        return _connection;
    }
}