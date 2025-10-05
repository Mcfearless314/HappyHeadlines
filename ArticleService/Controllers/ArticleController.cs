using ArticleService.Infrastructure;
using ArticleService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticleController : ControllerBase
{
    private readonly Database _database;
    private readonly ArticleCache _articleCache;
    
    public ArticleController(Database database, ArticleCache articleCache)
    {
        _database = database;
        _articleCache = articleCache;
    }

    [HttpGet]
    public async Task<IEnumerable<Article>> Get()
    {
        return await _database.GetArticles();
    }
    
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Article?> Get([FromRoute]int id)
    {
        return await _articleCache.GetArticleAsync(id, () => _database.GetArticleById(id));
    }
}