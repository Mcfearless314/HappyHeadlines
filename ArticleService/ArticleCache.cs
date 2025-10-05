using ArticleService.Model;
using Microsoft.Extensions.Caching.Memory;
using Prometheus;

namespace ArticleService;

public class ArticleCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _memoryCacheDuration = TimeSpan.FromDays(14);
    private static HashSet<int> _cachedArticleIds = new();
    
    private static readonly Counter CacheHits = Metrics.CreateCounter("article_cache_hits_total", "Number of cache hits");
    private static readonly Counter CacheMisses = Metrics.CreateCounter("article_cache_misses_total", "Number of cache misses");
    private static readonly Gauge CacheHitRatio = Metrics.CreateGauge("article_cache_hit_ratio", "Cache hit ratio");

    public ArticleCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<Article?> GetArticleAsync(int id, Func<Task<Article?>> factory)
    {
        if (_memoryCache.TryGetValue(id, out Article? article))
        {
            Console.WriteLine("Article found in memory cache: " + id);
            CacheHits.Inc();
            UpdateCacheHitRatio();
            return article;
        }
        
        Console.WriteLine("Article not found in memory cache, loading from DB: " + id);
        CacheMisses.Inc();
        UpdateCacheHitRatio();

        article = await factory();

        if (article != null)
        {
            _memoryCache.Set(id, article, _memoryCacheDuration);
            _cachedArticleIds.Add(id);
            Console.WriteLine("Storing article in memory cache: " + id);
        }

        return article;
    }
    
    public void PreloadArticles(IEnumerable<Article> articles)
    {
        foreach (var article in articles)
        {
            if (_memoryCache.TryGetValue(article.Id, out _))
            {
                Console.WriteLine("Article already in cache, skipping: " + article.Id);
            }
            else
            {
                _memoryCache.Set(article.Id, article, _memoryCacheDuration);
                _cachedArticleIds.Add(article.Id);
                Console.WriteLine("Preloading article into cache: " + article.Id);
            }
        }
    }

    public ArticleMetrics GetMetrics()
    {
        List<int> cachedIds = _cachedArticleIds.ToList();
        var totalRequests = CacheHits.Value + CacheMisses.Value;
        var hitRatio = totalRequests == 0 ? 0 : CacheHits.Value / totalRequests;
        CacheHitRatio.Set(hitRatio);
        
        return new ArticleMetrics
        {
            Hits = CacheHits.Value,
            Misses = CacheMisses.Value,
            ArticleIdsInCache = cachedIds,
            CacheHitRatio = hitRatio
        };
    }
    
    private void UpdateCacheHitRatio()
    {
        var totalRequests = CacheHits.Value + CacheMisses.Value;
        var hitRatio = totalRequests == 0 ? 0 : CacheHits.Value / totalRequests;
        hitRatio = hitRatio * 100;
        CacheHitRatio.Set(hitRatio);
    }
}