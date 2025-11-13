using Comments.Core.Entities;

namespace Comments.Core.Specifications
{
    public class CommentSpecification
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int? ParentId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IQueryable<Comment> Apply(IQueryable<Comment> query)
        {
            if (ParentId.HasValue)
            {
                query = query.Where(c => c.ParentId == ParentId);
            }
            else
            {
                query = query.Where(c => c.ParentId == null);
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                query = query.Where(c => c.User.UserName.Contains(UserName));
            }

            if (!string.IsNullOrEmpty(Email))
            {
                query = query.Where(c => c.User.Email.Contains(Email));
            }

            if (StartDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= EndDate.Value);
            }

            query = SortBy.ToLower() switch
            {
                "username" => SortDescending
                    ? query.OrderByDescending(c => c.User.UserName)
                    : query.OrderBy(c => c.User.UserName),
                "email" => SortDescending
                    ? query.OrderByDescending(c => c.User.Email)
                    : query.OrderBy(c => c.User.Email),
                _ => SortDescending
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt)
            };

            return query;
        }
    }
}