using ArticleService.Model;
using Microsoft.Extensions.Caching.Memory;

namespace ArticleService;

public class ArticleCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _memoryCacheDuration = TimeSpan.FromDays(14);
    private static int _hits = 0;
    private static int _misses = 0;

    public ArticleCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<Article?> GetArticleAsync(int id, Func<Task<Article?>> factory)
    {
        if (_memoryCache.TryGetValue(id, out Article? article))
        {
            Console.WriteLine("Article found in memory cache: " + id);
            _hits++;
            return article;
        }
        
        Console.WriteLine("Article not found in memory cache, loading from DB: " + id);
        _misses++;

        article = await factory();

        if (article != null)
        {
            _memoryCache.Set(id, article, _memoryCacheDuration);
            Console.WriteLine("Storing article in memory cache: " + id);
        }

        return article;
    }
    
    public void PreloadArticles(IEnumerable<Article> articles)
    {
        foreach (var article in articles)
        {
            _memoryCache.Set(article.Id, article, _memoryCacheDuration);
        }
    }

    public ArticleMetrics GetMetrics()
    {
        var totalRequests = _hits + _misses;
        var hitRatio = totalRequests == 0 ? 0 : (double)_hits / totalRequests;
        
        return new ArticleMetrics
        {
            Hits = _hits,
            Misses = _misses,
            CacheHitRatio = hitRatio
        };
    }
}