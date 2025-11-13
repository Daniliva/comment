using Comments.Core.Entities;
using Comments.Core.Specifications;

namespace Comments.Core.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<PagedList<Comment>> GetCommentsAsync(CommentSpecification specification);
        Task<IEnumerable<Comment>> GetRepliesAsync(int parentId);
        Task<Comment?> GetCommentWithRepliesAsync(int id);
        Task<int> GetCommentCountAsync(CommentSpecification? specification = null);
    }
}