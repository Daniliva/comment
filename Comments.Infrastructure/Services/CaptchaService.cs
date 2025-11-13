using Comments.Core.DTOs.Responses;
using Comments.Core.Entities;
using Comments.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Comments.Infrastructure.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly ICaptchaRepository _captchaRepository;
        private readonly ILogger<CaptchaService> _logger;
        private readonly Random _random = new();

        public CaptchaService(ICaptchaRepository captchaRepository, ILogger<CaptchaService> logger)
        {
            _captchaRepository = captchaRepository;
            _logger = logger;
        }

        public async Task<CaptchaResponse> GenerateCaptchaAsync()
        {
            var code = GenerateRandomCode(6);
            var imageData = GenerateCaptchaImage(code);

            var captcha = new Captcha
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _captchaRepository.AddAsync(captcha);
            await _captchaRepository.SaveChangesAsync();

            await CleanupOldCaptchas();

            return new CaptchaResponse
            {
                CaptchaId = captcha.Id.ToString(),
                ImageData = imageData,
                ExpiresAt = captcha.ExpiresAt
            };
        }

        public async Task<bool> ValidateCaptchaAsync(string captchaId, string code)
        {
            if (!int.TryParse(captchaId, out int id))
            {
                return false;
            }

            var captcha = await _captchaRepository.GetByIdAsync(id);
            if (captcha == null || captcha.ExpiresAt <= DateTime.UtcNow)
            {
                return false;
                
            }

            return string.Equals(captcha.Code, code, StringComparison.OrdinalIgnoreCase);
        }

        public async Task MarkAsUsedAsync(string captchaId)
        {
            if (!int.TryParse(captchaId, out int id))
            {
                return;
            }

            var captcha = await _captchaRepository.GetByIdAsync(id);
            if (captcha != null)
            {
                captcha.IsUsed = true;
                await _captchaRepository.SaveChangesAsync();
            }
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        private string GenerateCaptchaImage(string code)
        {
            using var image = new Image<Rgba32>(200, 60);

            // Fill background
            image.Mutate(ctx => ctx.BackgroundColor(Color.White));

            // Simple noise - random pixels
            for (int i = 0; i < 1000; i++)
            {
                var x = _random.Next(0, image.Width);
                var y = _random.Next(0, image.Height);
                image[x, y] = Color.FromRgb(
                    (byte)_random.Next(150, 255),
                    (byte)_random.Next(150, 255),
                    (byte)_random.Next(150, 255)
                );
            }

            // Draw text
            //  var font = SystemFonts.CreateFont("Arial", 24, FontStyle.Bold);
            var font = SystemFonts.CreateFont("DejaVu Sans", 24, FontStyle.Bold);
            var textOptions = new TextOptions(font)
            {
                Origin = new PointF(20, 15),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Draw each character with slight variations
            for (int i = 0; i < code.Length; i++)
            {
                var charText = code[i].ToString();
                textOptions.Origin = new PointF(25 + i * 25, 15 + _random.Next(-3, 3));
                var position = new PointF(25 + i * 25, 15 + _random.Next(-3, 3));
                // Vary font size slightly
                //   var variedFont = SystemFonts.CreateFont("Arial", 22 + _random.Next(0, 6), FontStyle.Bold);
                var variedFont = SystemFonts.CreateFont("DejaVu Sans", 22 + _random.Next(0, 6), FontStyle.Bold);
                textOptions.Font = variedFont;

                image.Mutate(ctx => ctx.DrawText(
                    charText,
                    variedFont,
                    GetRandomDarkColor(),
                    position
                ));
            }

            using var memoryStream = new MemoryStream();
            image.SaveAsPng(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private Color GetRandomDarkColor()
        {
            var colors = new[]
            {
                Color.DarkBlue, Color.DarkGreen, Color.DarkRed,
                Color.DarkMagenta, Color.DarkCyan, Color.Black,
                Color.DarkSlateGray, Color.MidnightBlue

            };
            return colors[_random.Next(colors.Length)];
        }

        private async Task CleanupOldCaptchas()
        {
            try
            {
                await _captchaRepository.CleanupExpiredCaptchasAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired captchas");
            }
        }
    }
}