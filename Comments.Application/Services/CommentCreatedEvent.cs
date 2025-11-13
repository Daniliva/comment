namespace Comments.API;

public class CommentCreatedEvent
{
    public int CommentId { get; set; }
    public string UserName { get; set; } = string.Empty;
}