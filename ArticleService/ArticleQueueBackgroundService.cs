using ArticleService.Infrastructure;
using ArticleService.Messaging;
using ArticleService.Model;
using EasyNetQ;

namespace ArticleService;

public class ArticleQueueBackgroundService : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);
    private readonly MessageClient _messageClient;
    private readonly Database _database;

    public ArticleQueueBackgroundService(MessageClient messageClient, Database database)
    {
        _messageClient = messageClient;
        _database = database;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int retryDelayMs = 2000;

        try
        {
            await _messageClient.SubscribeAsync<Article>(
                "article-queue",
                raw =>
                {
                    MonitorService.MonitorService.Log.Debug("[Subscriber] Received article event from queue.");
                    return HandleArticleEvent(raw);
                }
            );


            MonitorService.MonitorService.Log.Information("[Subscriber] Subscription established.");
        }
        catch (Exception ex)
        {
            MonitorService.MonitorService.Log.Error($"[Subscriber] Subscription failed: {ex.Message}. Retrying...");

            try
            {
                await Task.Delay(retryDelayMs, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[Subscriber] Stopping due to cancellation.");
        }


        Console.WriteLine("[Subscriber] Stopping");
    }

    private async Task HandleArticleEvent(Article article)
    {
        MonitorService.MonitorService.Log.Debug($"[Handler] Handling article: {article.Id} from queue.");
        
        var articleExists = await _database.GetArticleById(article.Id);

        if (articleExists == null)
        {
            await _database.AddArticle(article);
        }
        else
        {
            MonitorService.MonitorService.Log.Debug($"Article: {article.Id} already exists in the database, skipping...");
        }
    }
}