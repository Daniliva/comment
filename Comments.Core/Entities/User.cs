using System.ComponentModel.DataAnnotations;

namespace Comments.Core.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(500)]
        public string? HomePage { get; set; }

        [Required]
        [StringLength(45)]
        public string UserIP { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivity { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}