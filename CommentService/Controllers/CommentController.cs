using CommentService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentController : ControllerBase
{
    private readonly Database _database;
    private readonly CommentCache _commentCache;

    public CommentController(Database database, CommentCache commentCache)
    {
        _database = database;
        _commentCache = commentCache;
    }

    [HttpGet]
    public async Task<List<Comment>> Get()
    {
        return await _database.GetComments();
    }

    [HttpGet("{articleId}")]
    public async Task<List<Comment?>> Get([FromRoute] int articleId)
    {
        var comments = await _commentCache.GetCachedCommentsAsync(articleId);
        return comments == null
            ? throw new ArgumentNullException($"No comments found for this article: {articleId}")
            : comments!;
    }
}