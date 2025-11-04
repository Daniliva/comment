using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Comments.Core.DTOs.Requests
{
    public class CreateCommentRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]+$")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Url]
        [StringLength(500)]
        public string? HomePage { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Text { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [Required]
        public string CaptchaId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CaptchaCode { get; set; } = string.Empty;

        public IFormFile? File { get; set; }
    }
}
