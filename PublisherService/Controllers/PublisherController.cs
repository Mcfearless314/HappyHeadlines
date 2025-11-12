using Microsoft.AspNetCore.Mvc;
using PublisherService.Infrastructure;
using PublisherService.Models;

namespace PublisherService.Controllers;

[ApiController]
[Route("[controller]")]
public class PublisherController : ControllerBase
{
    private readonly Database _database;

    public PublisherController(Database database)
    {
        _database = database;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var articles = await _database.GetArticlesAsync();
        return Ok(articles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateArticle([FromBody] Article article)
    {
        var createdArticle = await _database.CreateArticleAsync(article);
        return Ok(createdArticle);
    }
}