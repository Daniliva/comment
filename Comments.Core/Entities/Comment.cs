using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comments.Core.Entities
{
    public class Comment : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Comment? Parent { get; set; }

        [Required]
        [StringLength(5000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public string TextHtml { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [StringLength(255)]
        public string? FileName { get; set; }

        [StringLength(10)]
        public string? FileExtension { get; set; }

        public long? FileSize { get; set; }

        [StringLength(500)]
        public string? FilePath { get; set; }

        public FileType? FileType { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}