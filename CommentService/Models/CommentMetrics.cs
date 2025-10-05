namespace CommentService.Models;

public class CommentMetrics
{
    public double Hits { get; set; }
    public double Misses { get; set; }
    public LinkedList<int> LruList { get; set; }
    public double CacheHitRatio { get; set; }
}