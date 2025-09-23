using CommentService.Models;

namespace CommentService.Infrastructure;

public class Database
{
    private readonly DbProvider _dbProvider;
    public Database(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public List<Comment> GetComments()
    {
        var connection = _dbProvider.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Comments";
        var res = new List<Comment>();
        
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var content = reader.GetString(1);
                var userId = reader.GetInt32(2);
                var articleId = reader.GetInt32(3);

                res.Add(new Comment
                {
                    Id = id,
                    Content = content,
                    UserId = userId,
                    ArticleId = articleId
                });
            }
        }
        return res;
    }
    
    public void CreateComment(string content, int userId, int articleId)
    {
        var connection = _dbProvider.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Comments (Content, UserId, ArticleId) VALUES (@content, @userId, @articleId)";
        
        var contentParam = command.CreateParameter();
        contentParam.ParameterName = "@content";
        contentParam.Value = content;
        command.Parameters.Add(contentParam);
        
        var userIdParam = command.CreateParameter();
        userIdParam.ParameterName = "@userId";
        userIdParam.Value = userId;
        command.Parameters.Add(userIdParam);
        
        var articleIdParam = command.CreateParameter();
        articleIdParam.ParameterName = "@articleId";
        articleIdParam.Value = articleId;
        command.Parameters.Add(articleIdParam);
        
        command.ExecuteScalar();
        
    }
}