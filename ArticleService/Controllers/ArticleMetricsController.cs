using ArticleService.Model;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticleMetricsController : ControllerBase
{
    private readonly ArticleCache _articleCache;
    
    public ArticleMetricsController(ArticleCache articleCache)
    {
        _articleCache = articleCache;
    }
    
    [HttpGet("metrics")]
    public ArticleMetrics GetMetrics()
    {
        var result = _articleCache.GetMetrics();
        return result;
    }
}