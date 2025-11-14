using ArticleService.Model;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Infrastructure;

public class Database
{
    private readonly DbProvider _dbProvider;

    public Database(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public async Task<List<Article>> GetArticles()
    {
        var articles = new List<Article>();

        var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Articles";

        var reader = await command.ExecuteReaderAsync();
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

    public async Task<Article?> GetArticleById(int id)
    {
        Console.WriteLine($"Getting article by id: {id}");
        var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Content, PublishDate FROM Articles WHERE Id = @Id";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        var reader = await command.ExecuteReaderAsync();
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

    public async Task AddArticle(Article article)
    {
        Console.WriteLine($"Adding article: {article.Title} to database");
        
        var connection = _dbProvider.GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Articles (Id, Title, Content, PublishDate) VALUES(@Id, @Title, @Content, @PublishDate)";
        
        var pId = command.CreateParameter();
        pId.ParameterName = "@Id";
        pId.Value = article.Id;
        command.Parameters.Add(pId);
        
        var pTitle = command.CreateParameter();
        pTitle.ParameterName = "@Title";
        pTitle.Value = article.Title;
        command.Parameters.Add(pTitle);

        var pContent = command.CreateParameter();
        pContent.ParameterName = "@Content";
        pContent.Value = article.Content;
        command.Parameters.Add(pContent);

        var pPublishDate = command.CreateParameter();
        pPublishDate.ParameterName = "@PublishDate";
        pPublishDate.Value = article.PublishDate;
        command.Parameters.Add(pPublishDate);

        await command.ExecuteNonQueryAsync();
        
    }
}