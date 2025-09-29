using CommentService.Models;

namespace CommentService;

public class DbInitializer
{
    private readonly DbProvider _dbProvider;

    public DbInitializer(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }
    
    public void Initialize()
    {
        var random = new Random();
        var sampleComments = new List<Comment>();

        for (int i = 1; i <= 50; i++)
        {
            sampleComments.Add(new Comment
            {
                Content = $"This is comment {i}",
                UserId = random.Next(1, 21),      
                ArticleId = i   
            });
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
            insertCommand.CommandText = "INSERT INTO Comments (Content, UserId, ArticleId) VALUES (@content, @userId, @articleId)";

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