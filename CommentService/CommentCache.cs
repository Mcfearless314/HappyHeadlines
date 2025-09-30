using CommentService.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CommentService;

public class CommentCache
{
    private readonly Database _database;
    private static Dictionary<int, List<Comment>?> _cachedComments = new();
    private static LinkedList<int> _lruList = new();
    private const int MaxCacheSize = 30;

    public CommentCache(Database database)
    {
        _database = database;
    }

    public async Task<List<Comment>?> GetCachedCommentsAsync(int articleId)
    {
        if (_cachedComments.ContainsKey(articleId))
        {
            Console.WriteLine($"Cache hit, found comments in cache for article: {articleId}");
            Console.WriteLine("Cached comments: " + _cachedComments.GetValueOrDefault(articleId).First().Content);
            _lruList.Remove(articleId);
            _lruList.AddFirst(articleId);
            return _cachedComments.GetValueOrDefault(articleId);
        }

        Console.WriteLine("Cache miss - loading from DB");
        var comments = await GetDbComments(articleId);

        if (comments != null)
        {
            if (_lruList.Count >= MaxCacheSize)
            {
                Console.WriteLine("Cache full - evicting least recently used item");
                var lruArticleId = _lruList.Last.Value;

                _lruList.RemoveLast();
                _cachedComments.Remove(lruArticleId);

                _lruList.AddFirst(articleId);
                _cachedComments.Add(articleId, comments);
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
}