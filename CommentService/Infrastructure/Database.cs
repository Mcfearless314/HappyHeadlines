using CommentService.Models;

namespace CommentService;

public class Database
{
    private readonly DbProvider _dbProvider;

    public Database(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }
    
    public async Task<Comment?> GetCommentsByArticleId(int articleId)
    {
        Console.WriteLine($"Getting comment by article id: {articleId}");
        using var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Content, UserId FROM Comments WHERE ArticleId = @ArticleId";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@ArticleId";
        parameter.Value = articleId;
        command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Comment
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            };
        }

        return null; 
    }
}