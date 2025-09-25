using ArticleService.Model;
using Microsoft.Extensions.Caching.Memory;

namespace ArticleService;

public class ArticleCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _memoryCacheDuration = TimeSpan.FromDays(14);

    public ArticleCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<Article?> GetArticleAsync(int id, Func<Task<Article?>> factory)
    {
        if (_memoryCache.TryGetValue(id, out Article? article))
        {
            Console.WriteLine("Article found in memory cache: " + id);
            return article;
        }

        article = await factory();

        if (article != null)
        {
            _memoryCache.Set(id, article, _memoryCacheDuration);
            Console.WriteLine("Storing article in memory cache: " + id);
        }

        return article;
    }
}