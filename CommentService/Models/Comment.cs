namespace CommentService.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int UserId { get; set; }
    public int ArticleId { get; set; }
}