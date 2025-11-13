using Comments.Core.DTOs.Responses;

namespace Comments.Core.Interfaces
{
    public interface ICaptchaService
    {
        Task<CaptchaResponse> GenerateCaptchaAsync();
        Task<bool> ValidateCaptchaAsync(string captchaId, string code);
        Task MarkAsUsedAsync(string captchaId);
    }
}