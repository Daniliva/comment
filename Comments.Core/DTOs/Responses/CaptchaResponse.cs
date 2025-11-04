namespace Comments.Core.DTOs.Responses;

public class CaptchaResponse
{
    public string CaptchaId { get; set; } = string.Empty;
    public string ImageData { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}