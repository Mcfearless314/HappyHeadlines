namespace ArticleService.Model;

public class ArticleMetrics
{
    public double Hits { get; set; }
    public double Misses { get; set; }
    public List<int> ArticleIdsInCache { get; set; }
    public double CacheHitRatio { get; set; }
}