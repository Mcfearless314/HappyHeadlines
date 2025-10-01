using CommentService.Models;

namespace CommentService.Infrastructure;

public class DbInitializer
{
    private readonly DbProvider _dbProvider;
    private readonly CommentCache _commentCache;

    public DbInitializer(DbProvider dbProvider, CommentCache commentCache)
    {
        _dbProvider = dbProvider;
        _commentCache = commentCache;
    }

    public void Initialize()
    {
        var random = new Random();
        var sampleComments = new List<Comment>();
      
        for (int articleId = 1; articleId <= 40; articleId++)
        {
            var comments = new List<Comment>();
            for (int j = 0; j < 5; j++)
            {
                var comment = new Comment
                {
                    Content = $"Comment {j + 1} for article {articleId}",
                    UserId = random.Next(1, 21),
                    ArticleId = articleId
                };
                sampleComments.Add(comment);
                comments.Add(comment);
            }
            
            if (articleId <= 30)
            {
                _commentCache.PreloadComments(articleId, comments);
            }
        }

        using var connection = _dbProvider.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        DROP TABLE IF EXISTS Comments;
        CREATE TABLE Comments(Id INT IDENTITY(1,1) PRIMARY KEY, Content NVARCHAR(50), UserId INT, ArticleId INT)";
        command.ExecuteNonQuery();

        foreach (var Comment in sampleComments)
        {
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText =
                "INSERT INTO Comments (Content, UserId, ArticleId) VALUES (@content, @userId, @articleId)";

            var contentParam = insertCommand.CreateParameter();
            contentParam.ParameterName = "@content";
            contentParam.Value = Comment.Content;
            insertCommand.Parameters.Add(contentParam);

            var userIdParam = insertCommand.CreateParameter();
            userIdParam.ParameterName = "@userId";
            userIdParam.Value = Comment.UserId;
            insertCommand.Parameters.Add(userIdParam);
            
            var articleIdParam = insertCommand.CreateParameter();
            articleIdParam.ParameterName = "@articleId";
            articleIdParam.Value = Comment.ArticleId;
            insertCommand.Parameters.Add(articleIdParam);

            insertCommand.ExecuteNonQuery();
        }
    }
}