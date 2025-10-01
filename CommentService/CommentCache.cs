using CommentService.Models;
using Prometheus;

namespace CommentService;

public class CommentCache
{
    private readonly Database _database;
    private static Dictionary<int, List<Comment>?> _cachedComments = new();
    private static LinkedList<int> _lruList = new();
    private const int MaxCacheSize = 30;
    private static int _hits = 0;
    private static int _misses = 0;
    
    private static readonly Counter CacheHits = Metrics.CreateCounter("comment_cache_hits_total", "Number of cache hits");
    private static readonly Counter CacheMisses = Metrics.CreateCounter("comment_cache_misses_total", "Number of cache misses");
    private static readonly Gauge CacheHitRatio = Metrics.CreateGauge("comment_cache_hit_ratio", "Cache hit ratio");

    public CommentCache(Database database)
    {
        _database = database;
    }

    public async Task<List<Comment>?> GetCachedCommentsAsync(int articleId)
    {
        Console.WriteLine("LRU list: " + string.Join(", ", _lruList));
        Console.WriteLine("Cached comments keys: " + string.Join(", ", _cachedComments.Keys));


        if (_cachedComments.ContainsKey(articleId))
        {
            Console.WriteLine($"Cache hit, found comments in cache for article: {articleId}");
            Console.WriteLine("Cached comments: " + _cachedComments.GetValueOrDefault(articleId).First().Content);
            _lruList.Remove(articleId);
            _lruList.AddFirst(articleId);
            _hits++;
            CacheHits.Inc();
            UpdateCacheHitRatio();
            return _cachedComments.GetValueOrDefault(articleId);
        }

        Console.WriteLine("Cache miss - loading from DB");
        _misses++;
        CacheMisses.Inc();
        UpdateCacheHitRatio();
        var comments = await GetDbComments(articleId);

        if (comments != null)
        {
            if (_lruList.Count >= MaxCacheSize)
            {
                Console.WriteLine("Cache full - evicting least recently used item");
                var lruArticleId = _lruList.Last.Value;
                Console.WriteLine($"Removing article ID: {lruArticleId}");

                _lruList.RemoveLast();
                _cachedComments.Remove(lruArticleId);

                _lruList.AddFirst(articleId);
                _cachedComments.Add(articleId, comments);

                return comments;
            }

            Console.WriteLine($"Adding article: {articleId} to cache along with comments");
            _lruList.AddFirst(articleId);
            _cachedComments.Add(articleId, comments);
            Console.WriteLine("Cached comments: " + _cachedComments.GetValueOrDefault(articleId).First().Content);
            return comments;
        }

        Console.Write("No comments found in DB, returning null");
        return null;
    }

    public async Task<List<Comment>?> GetDbComments(int articleId)
    {
        return await _database.GetCommentsByArticleId(articleId);
    }

    public void PreloadComments(int articleId, List<Comment> comments)
    {
        _lruList.AddFirst(articleId);
        _cachedComments.Add(articleId, comments);
    }
    
    public CommentMetrics GetMetrics()
    {
        var totalRequests = _hits + _misses;
        var hitRatio = totalRequests == 0 ? 0 : (double)_hits / totalRequests;
        CacheHitRatio.Set(hitRatio);

        return new CommentMetrics
        {
            Hits = _hits,
            Misses = _misses,
            LruList = _lruList,
            CacheHitRatio = hitRatio
        };
    }
    
    private void UpdateCacheHitRatio()
    {
        var totalRequests = _hits + _misses;
        var hitRatio = totalRequests == 0 ? 0 : (double)_hits / totalRequests;
        CacheHitRatio.Set(hitRatio);
    }
}