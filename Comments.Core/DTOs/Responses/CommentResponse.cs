using Nest;

namespace Comments.Core.DTOs.Responses
{
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class CommentResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? HomePage { get; set; }
        public string Text { get; set; } = string.Empty;
        public string TextHtml { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ParentId { get; set; }
        public FileInfoResponse? File { get; set; }
        public List<CommentResponse> Replies { get; set; } = new();
    }
}