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
            // Newer articles
            new Article { Title = "Hello World", Content = "This is the first article.", PublishDate = DateTime.Today.AddDays(-1) },
            new Article { Title = "Caching Tips", Content = "How to use caches effectively.", PublishDate = DateTime.Today.AddDays(-3) },
            new Article { Title = "Grafana Dashboards", Content = "Visualizing metrics easily.", PublishDate = DateTime.Today.AddDays(-7) },

            // Older articles 
            new Article { Title = "Old Article 1", Content = "This article is older than 14 days.", PublishDate = DateTime.Today.AddDays(-20) },
            new Article { Title = "Old Article 2", Content = "Another old article.", PublishDate = DateTime.Today.AddDays(-30) },
            new Article { Title = "Old Article 3", Content = "Yet another old article.", PublishDate = DateTime.Today.AddDays(-45) }
        };

        using var connection = _dbProvider.GetConnection();
        connection.Open(); 

        using var command = connection.CreateCommand();
        command.CommandText = @"
        DROP TABLE IF EXISTS Articles;
        CREATE TABLE Articles(Id INT IDENTITY(1,1) PRIMARY KEY, Title NVARCHAR(50), Content NVARCHAR(200), PublishDate DATETIME NOT NULL)";
        command.ExecuteNonQuery();

        foreach (var article in sampleArticles)
        {
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO Articles (Title, Content, PublishDate) VALUES (@title, @content, @publishDate)";
            var titleParam = insertCommand.CreateParameter();
            titleParam.ParameterName = "@title";
            titleParam.Value = article.Title;
            insertCommand.Parameters.Add(titleParam);

            var contentParam = insertCommand.CreateParameter();
            contentParam.ParameterName = "@content";
            contentParam.Value = article.Content;
            insertCommand.Parameters.Add(contentParam);
            
            var dateParam = insertCommand.CreateParameter();
            dateParam.ParameterName = "@publishDate";
            dateParam.Value = article.PublishDate;
            insertCommand.Parameters.Add(dateParam);

            insertCommand.ExecuteNonQuery();
        }
    }
}