using ArticleService.Infrastructure;

namespace ArticleService;

public class ArticleCacheBackgroundService : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
    private readonly HttpClient _client = new();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var id = new Random().Next(1, 8);
                Console.WriteLine("Trying with this article id: "+id);
                var response = await _client.GetAsync($"http://article-service:8080/Article/{id}", stoppingToken);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Fetched article content: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching article: {ex.Message}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}