using CommentService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentController : ControllerBase
{
    private readonly Database _database;
    private readonly CommentCache _commentCache;
    private readonly IHttpClientFactory _httpClientFactory;

    public CommentController(Database database, CommentCache commentCache, IHttpClientFactory httpClientFactory)
    {
        _database = database;
        _commentCache = commentCache;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<List<Comment>> Get()
    {
        return await _database.GetComments();
    }

    [HttpGet("{articleId}")]
    public async Task<List<Comment?>> GetById([FromRoute] int articleId)
    {
        var comments = await _commentCache.GetCachedCommentsAsync(articleId);
        return comments == null
            ? throw new ArgumentNullException($"No comments found for this article: {articleId}")
            : comments!;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Comment comment)
    {
        var client = _httpClientFactory.CreateClient("ProfanityService");
        bool hasProfanity;
        
        try
        {
            var response = await client.GetAsync
                ($"/Profanity/check?text={Uri.EscapeDataString(comment.Content)}");
            response.EnsureSuccessStatusCode();

            hasProfanity = await response.Content.ReadFromJsonAsync<bool>();
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"Unable to check comment for profanity: {ex.Message}");
        }

        if (hasProfanity)
            return BadRequest("Comment contains prohibited content");

        var postedComment = await _database.AddComment(comment);
        return Ok(postedComment);
    }

}