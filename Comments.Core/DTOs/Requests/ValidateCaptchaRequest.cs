using System.ComponentModel.DataAnnotations;

namespace Comments.Core.DTOs.Requests
{
    public class ValidateCaptchaRequest
    {
        [Required]
        public string CaptchaId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
    }
}