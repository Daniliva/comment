using Comments.Core.Entities;

namespace Comments.Core.DTOs.Responses
{
    public class FileInfoResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public FileType FileType { get; set; }
        public string? ThumbnailPath { get; set; }
    }
}