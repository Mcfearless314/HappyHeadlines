using CommentService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentMetricsController : ControllerBase
{
    private readonly CommentCache _commentCache;
    
    public CommentMetricsController(CommentCache commentCache)
    {
        _commentCache = commentCache;
    }
    
    [HttpGet("metrics")]
    public CommentMetrics GetMetrics()
    {
        var result = _commentCache.GetMetrics();
        return result;
    }
}