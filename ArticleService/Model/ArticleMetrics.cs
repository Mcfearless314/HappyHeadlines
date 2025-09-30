namespace ArticleService.Model;

public class ArticleMetrics
{
    public int Hits { get; set; }
    public int Misses { get; set; }
    public LinkedList<int> LruList { get; set; }
    public double CacheHitRatio { get; set; }
}