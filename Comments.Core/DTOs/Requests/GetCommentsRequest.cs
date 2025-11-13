namespace Comments.Core.DTOs.Requests
{
    public class GetCommentsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int? ParentId { get; set; }
    }
}