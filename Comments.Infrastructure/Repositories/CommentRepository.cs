using Comments.Core.Entities;
using Comments.Core.Interfaces;
using Comments.Core.Specifications;
using Comments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Comments.Infrastructure.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedList<Comment>> GetCommentsAsync(CommentSpecification specification)
        {
            var query = _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .AsQueryable();

            query = specification.Apply(query);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((specification.Page - 1) * specification.PageSize)
                .Take(specification.PageSize)
                .ToListAsync();

            return new PagedList<Comment>(items, totalCount, specification.Page, specification.PageSize);
        }

        public async Task<IEnumerable<Comment>> GetRepliesAsync(int parentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentWithRepliesAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<int> GetCommentCountAsync(CommentSpecification? specification = null)
        {
            var query = _context.Comments.AsQueryable();

            if (specification != null)
            {
                query = specification.Apply(query);
            }

            return await query.CountAsync();
        }
    }
}