using Comments.Application;
using Comments.Core.Exceptions;
using Comments.Core.Interfaces;
using Comments.Core.Specifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
// Добавлено

// Добавлено
namespace Comments.API.Service
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        private const int MaxImageWidth = 320;
        private const int MaxImageHeight = 240;
        private const long MaxTextFileSize = 100 * 1024; // 100KB
        private const long MaxImageFileSize = 5 * 1024 * 1024; // 5MB

        public FileService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<FileService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FileSaveResult> SaveFileAsync(IFormFile file)
        {
            ValidateFile(file);

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var fileName = Guid.NewGuid().ToString();
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fullFileName = $"{fileName}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fullFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string? thumbnailPath = null;
            var fileType = GetFileType(file.ContentType);

            if (fileType == FileType.Image)
            {
                thumbnailPath = await CreateThumbnailAsync(filePath, fileName);
            }

            return new FileSaveResult
            {
                FilePath = $"/uploads/{fullFileName}",
                FileName = Path.GetFileNameWithoutExtension(file.FileName),
                FileExtension = fileExtension,
                FileSize = file.Length,
                FileType = fileType,
                ThumbnailPath = thumbnailPath
            };
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", fullPath);
            }

            var thumbnailPath = Path.ChangeExtension(fullPath, ".thumb" + Path.GetExtension(fullPath));
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }

        public bool FileExists(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            return File.Exists(fullPath);
        }

        private void ValidateFile(IFormFile file)
        {
            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            var allowedTextTypes = new[] { "text/plain" };

            if (file.ContentType.StartsWith("image/"))
            {
                if (!allowedImageTypes.Contains(file.ContentType))
                {
                    throw new ValidationException($"Image type {file.ContentType} is not allowed");
                }

                if (file.Length > MaxImageFileSize)
                {
                    throw new ValidationException($"Image size cannot exceed {MaxImageFileSize / 1024 / 1024}MB");
                }
            }
            else if (file.ContentType == "text/plain")
            {
                if (file.Length > MaxTextFileSize)
                {
                    throw new ValidationException($"Text file size cannot exceed {MaxTextFileSize / 1024}KB");
                }
            }
            else
            {
                throw new ValidationException($"File type {file.ContentType} is not allowed");
            }
        }

        private async Task<string?> CreateThumbnailAsync(string originalPath, string fileName)
        {
            try
            {
                using var image = await Image.LoadAsync(originalPath);

                var dimensions = CalculateDimensions(image.Width, image.Height, MaxImageWidth, MaxImageHeight);

                image.Mutate(x => x.Resize(dimensions.Width, dimensions.Height));

                var thumbnailPath = Path.Combine(Path.GetDirectoryName(originalPath)!, $"{fileName}.thumb{Path.GetExtension(originalPath)}");
                await image.SaveAsync(thumbnailPath);

                return $"/uploads/{Path.GetFileName(thumbnailPath)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating thumbnail for {FilePath}", originalPath);
                return null;
            }
        }

        private (int Width, int Height) CalculateDimensions(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            return (
                Width: (int)(originalWidth * ratio),
                Height: (int)(originalHeight * ratio)
            );
        }

        private FileType GetFileType(string contentType)
        {
            return contentType.StartsWith("image/") ? FileType.Image : FileType.Text;
        }
    }
}
