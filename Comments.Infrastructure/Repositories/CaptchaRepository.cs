using Comments.Application;
using Comments.Core.Interfaces;
using Comments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Comments.Infrastructure.Repositories;

public class CaptchaRepository : BaseRepository<Captcha>, ICaptchaRepository
{
    public CaptchaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Captcha?> GetUnusedCaptchaAsync(string code)
    {
        return await _context.Captchas
            .FirstOrDefaultAsync(c => c.Code == code &&
                                      !c.IsUsed &&
                                      c.ExpiresAt > DateTime.UtcNow);
    }

    public async Task CleanupExpiredCaptchasAsync()
    {
        var expiredCaptchas = await _context.Captchas
            .Where(c => c.ExpiresAt <= DateTime.UtcNow || c.IsUsed)
            .ToListAsync();

        _context.Captchas.RemoveRange(expiredCaptchas);
        await _context.SaveChangesAsync();
    }
}