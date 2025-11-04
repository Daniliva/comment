using System.ComponentModel.DataAnnotations;

namespace Comments.Application;

public class Captcha : BaseEntity
{
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsUsed { get; set; } = false;
}