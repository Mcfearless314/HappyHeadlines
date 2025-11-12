namespace PublisherService.Infrastructure;

public class DbInitializer
{
    private readonly DbProvider _dbProvider;

    public DbInitializer(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public void Initialize()
    {
        using var connection = _dbProvider.GetConnection();
        connection.Open(); 

        using var command = connection.CreateCommand();
        command.CommandText = @"
        DROP TABLE IF EXISTS Articles;
        CREATE TABLE Articles(Id INT IDENTITY(1,1) PRIMARY KEY, Title NVARCHAR(50), Content NVARCHAR(200), PublishDate DATETIME NOT NULL)";
        command.ExecuteNonQuery();
    }
}