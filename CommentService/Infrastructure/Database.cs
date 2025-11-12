using CommentService.Models;

namespace CommentService;

public class Database
{
    private readonly DbProvider _dbProvider;

    public Database(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public async Task<List<Comment>> GetComments()
    {
        var comments = new List<Comment>();

        await using var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Comments";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            comments.Add(new Comment
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                ArticleId = reader.GetInt32(reader.GetOrdinal("ArticleId")),
            });
        }
        return comments;
    }

    public async Task<List<Comment>?> GetCommentsByArticleId(int articleId)
    {
        var comments = new List<Comment>();
        using var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        try
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Content, UserId FROM Comments WHERE ArticleId = @ArticleId";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@ArticleId";
            parameter.Value = articleId;
            command.Parameters.Add(parameter);

            var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                comments.Add(new Comment
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Content = reader.GetString(reader.GetOrdinal("Content")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                });

            
        }
        catch (Exception ex)
        {
            throw new KeyNotFoundException("Article not found in database", ex);
        }

        if (comments.Count > 0)
        {
            return comments;
        }

        return null;
    }

    public async Task<Comment> AddComment(Comment comment)
    {
        using var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Comments (Content, UserId, ArticleId)
        OUTPUT INSERTED.Id, INSERTED.Content, INSERTED.UserId, INSERTED.ArticleId
        VALUES (@Content, @UserId, @ArticleId);";

        var paramContent = command.CreateParameter();
        paramContent.ParameterName = "@Content";
        paramContent.Value = comment.Content;
        command.Parameters.Add(paramContent);

        var paramUserId = command.CreateParameter();
        paramUserId.ParameterName = "@UserId";
        paramUserId.Value = comment.UserId;
        command.Parameters.Add(paramUserId);

        var paramArticleId = command.CreateParameter();
        paramArticleId.ParameterName = "@ArticleId";
        paramArticleId.Value = comment.ArticleId;
        command.Parameters.Add(paramArticleId);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Comment
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                ArticleId = reader.GetInt32(reader.GetOrdinal("ArticleId")),
            };
        }
        return null;
    }
}