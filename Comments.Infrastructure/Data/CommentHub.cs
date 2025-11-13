using Microsoft.AspNetCore.SignalR;

namespace Comments.Infrastructure.Data;

public class CommentHub : Hub
{
    public async Task JoinComments()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "CommentsGroup");
    }

    public async Task LeaveComments()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "CommentsGroup");
    }
}