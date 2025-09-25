using CommentService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentController : ControllerBase
{
    public readonly Database _database;

    public CommentController(Database database)
    {
        _database = database;
    }

    [HttpGet("{id}")]
    public async Task<Comment?> Get([FromRoute]int id)
    {
        return await _database.GetCommentsByArticleId(id);
    }
}