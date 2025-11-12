using System.Data.Common;
using PublisherService.Models;

namespace PublisherService.Infrastructure;

public class Database
{
    private readonly DbProvider _dbProvider;
    
    public Database(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }
    
    public async Task<List<Article>> GetArticlesAsync()
    {
        var articles = new List<Article>();

        await using var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Articles";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            articles.Add(new Article
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                PublishDate = reader.GetDateTime(reader.GetOrdinal("PublishDate"))
            });
        }

        return articles;
    }

    public async Task<Article> CreateArticleAsync(Article article)
    {
        using var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Articles (Title, Content, PublishDate) 
        OUTPUT INSERTED.Id, INSERTED.Title, INSERTED.Content, INSERTED.PublishDate 
        VALUES (@Title, @Content, @PublishDate);";
        
        var paramContent = command.CreateParameter();
        paramContent.ParameterName = "@Title";
        paramContent.Value = article.Title;
        command.Parameters.Add(paramContent);

        var paramUserId = command.CreateParameter();
        paramUserId.ParameterName = "@Content";
        paramUserId.Value = article.Content;
        command.Parameters.Add(paramUserId);

        var paramArticleId = command.CreateParameter();
        paramArticleId.ParameterName = "@PublishDate";
        paramArticleId.Value = DateTime.Today;
        command.Parameters.Add(paramArticleId);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Article
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                PublishDate = reader.GetDateTime(reader.GetOrdinal("PublishDate"))
            };
        }
        return null;
    }
}