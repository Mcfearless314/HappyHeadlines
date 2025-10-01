namespace CommentService.Models;

public class CommentMetrics
{
    public int Hits { get; set; }
    public int Misses { get; set; }
    public LinkedList<int> LruList { get; set; }
    public double CacheHitRatio { get; set; }
}