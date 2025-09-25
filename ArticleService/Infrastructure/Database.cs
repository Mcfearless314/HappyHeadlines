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

        using (var connection = _dbProvider.GetConnection())
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Articles";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var article = new Article
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Content = reader.GetString(reader.GetOrdinal("Content"))
                        };
                        articles.Add(article);
                    }
                }
            }
        }
        return articles;
    }
    
    public async Task<Article?> GetArticleById(int id)
    {
        Console.WriteLine($"Getting article by id: {id}");
        using (var connection = _dbProvider.GetConnection())
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Title, Content FROM Articles WHERE Id = @Id";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Id";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Article
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Content = reader.GetString(reader.GetOrdinal("Content"))
                        };
                    }
                }
            }
        }
        return null; 
    }
}