using ArticleService.Model;

namespace ArticleService.Infrastructure;

public class DbInitializer
{
    private readonly DbProvider _dbProvider;

    public DbInitializer(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }
    
    public void Initialize()
    {
        var sampleArticles = new List<Article>
        {
            new Article { Title = "Hello World", Content = "This is the first article." },
            new Article { Title = "Caching Tips", Content = "How to use caches effectively." },
            new Article { Title = "Grafana Dashboards", Content = "Visualizing metrics easily." }
        };

        using var connection = _dbProvider.GetConnection();
        connection.Open(); 

        using var command = connection.CreateCommand();
        command.CommandText = @"
        DROP TABLE IF EXISTS Articles;
        CREATE TABLE Articles(Id INT IDENTITY(1,1) PRIMARY KEY, Title NVARCHAR(50), Content NVARCHAR(200))";
        command.ExecuteNonQuery();

        foreach (var article in sampleArticles)
        {
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO Articles (Title, Content) VALUES (@title, @content)";

            var titleParam = insertCommand.CreateParameter();
            titleParam.ParameterName = "@title";
            titleParam.Value = article.Title;
            insertCommand.Parameters.Add(titleParam);

            var contentParam = insertCommand.CreateParameter();
            contentParam.ParameterName = "@content";
            contentParam.Value = article.Content;
            insertCommand.Parameters.Add(contentParam);

            insertCommand.ExecuteNonQuery();
        }
    }
}