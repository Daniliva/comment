using Comments.Core.DTOs.Responses;
using MediatR;

namespace Comments.Core.Events
{
    public class CommentCreatedEvent : INotification
    {
        public CommentResponse Comment { get; }

        public CommentCreatedEvent(CommentResponse comment)
        {
            Comment = comment;
        }
    }
}