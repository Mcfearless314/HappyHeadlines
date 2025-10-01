using ArticleService.Model;
using Microsoft.Extensions.Caching.Memory;

namespace ArticleService;

public class ArticleCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _memoryCacheDuration = TimeSpan.FromDays(14);
    private static int _hits = 0;
    private static int _misses = 0;
    private static HashSet<int> _cachedArticleIds = new();

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
        var totalRequests = _hits + _misses;
        var hitRatio = totalRequests == 0 ? 0 : (double)_hits / totalRequests;
        
        return new ArticleMetrics
        {
            Hits = _hits,
            Misses = _misses,
            ArticleIdsInCache = cachedIds,
            CacheHitRatio = hitRatio
        };
    }
}