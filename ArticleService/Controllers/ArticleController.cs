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
    public async Task<IActionResult> GetArticles([FromQuery] int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("Page Number and Page Size must be positive integers.");

        var articles = await _database.GetArticlesWithPagination(pageNumber, pageSize);

        return Ok(new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Articles = articles
        });
    }
    
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Article?> Get([FromRoute]int id)
    {
        return await _articleCache.GetArticleAsync(id, () => _database.GetArticleById(id));
    }
}