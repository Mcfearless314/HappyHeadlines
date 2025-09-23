using CommentService.Infrastructure;
using CommentService.Models;
using CommentService.Policies;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentController : Controller
{
    private readonly Database _database;
    private readonly CommentPolicy _commentPolicy;
    public CommentController(Database database, CommentPolicy commentPolicy)
    {
        _database = database;
        _commentPolicy = commentPolicy;
    }

    [HttpGet]
    public IEnumerable<Comment> Get()
    {
        return _database.GetComments();
    }

    [HttpPost]
    public IActionResult PostComment([FromBody] Comment comment)
    {
        var isCommentValid = _commentPolicy.CheckProfanity(comment.Content).Result;

        if (isCommentValid)
        {
            _database.CreateComment(comment.Content, comment.UserId, comment.ArticleId);
            return Ok();
        }
        else
        {
            return BadRequest("Comment contains profanity and cannot be added.");
        }
    }
}