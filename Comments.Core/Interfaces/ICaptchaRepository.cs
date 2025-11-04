using Comments.Application;

namespace Comments.Core.Interfaces;

public interface ICaptchaRepository : IRepository<Captcha>
{
    Task<Captcha?> GetUnusedCaptchaAsync(string code);
    Task CleanupExpiredCaptchasAsync();
}