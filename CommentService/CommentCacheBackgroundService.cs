namespace CommentService;

public class CommentCacheBackgroundService : BackgroundService
{
    private TimeSpan _interval = TimeSpan.FromSeconds(2);
    private readonly HttpClient _client = new();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var id = new Random().Next(1, 40);
                Console.WriteLine("Trying with this article id: "+id);
                var response =
                    await _client.GetAsync($"http://comment-service:8080/Comment/{id}",
                        stoppingToken);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Fetched comments content: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching comments: {ex.StackTrace}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}