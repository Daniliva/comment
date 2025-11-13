using Comments.Core.DTOs.Requests;
using Comments.Core.DTOs.Responses;

namespace Comments.Core.Interfaces
{
    public interface ICommentService
    {
        Task<PagedResponse<CommentResponse>> GetCommentsAsync(GetCommentsRequest request);
        Task<CommentResponse?> GetCommentAsync(int id);
        Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, string ipAddress, string userAgent);
        Task<bool> DeleteCommentAsync(int id);
        Task<IEnumerable<CommentResponse>> GetRepliesAsync(int parentId);
    }
}