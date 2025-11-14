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
        Console.WriteLine("[Debug] Raw message received:");
        Console.WriteLine(raw);
                    return HandleArticleEvent(raw);
    },
    stoppingToken
);


            Console.WriteLine("[Subscriber] Subscription established.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Subscriber] Subscription failed: {ex.Message}. Retrying...");

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
        Console.WriteLine($"Received Article: {article.Id} - {article.Title}");
        Console.WriteLine($"Content: {article.Content}");
        Console.WriteLine($"Publish Date: {article.PublishDate}");


        var articleExists = await _database.GetArticleById(article.Id);

        if (articleExists == null)
        {
            await _database.AddArticle(article);
        }
        else
        {
            Console.WriteLine($"Article: {article.Id} already exists in the database, skipping...");
        }
    }
}