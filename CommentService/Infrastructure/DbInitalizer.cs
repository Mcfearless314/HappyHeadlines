namespace CommentService.Infrastructure;

public class DbInitializer
{
    private readonly DbProvider _dbProvider;

    public DbInitializer(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public void Initialize()
    {
        var connection = _dbProvider.GetConnection();
        
        var command = connection.CreateCommand();
        command.CommandText = @"DROP TABLE IF EXISTS Comments;
        CREATE TABLE Comments(Id INT IDENTITY(1,1) PRIMARY KEY, Content NVARCHAR(100), ArticleId INT, UserId INT)";
        command.ExecuteNonQuery();
    }
}