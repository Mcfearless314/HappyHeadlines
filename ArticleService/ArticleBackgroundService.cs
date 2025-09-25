using ArticleService.Infrastructure;

namespace ArticleService;

public class ArticleBackgroundService : BackgroundService
{
    private readonly Database _database;
    private readonly ArticleCache _articleCache;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public ArticleBackgroundService(Database database, ArticleCache articleCache)
    {
        _database = database;
        _articleCache = articleCache;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var recentArticles = (await _database.GetArticles())
                    .Where(a => a.PublishDate >= DateTime.Today.AddDays(-14));

                _articleCache.PreloadArticles(recentArticles);

                Console.WriteLine($"Preloaded {recentArticles.Count()} articles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error preloading articles: {ex.Message}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}