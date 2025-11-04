using Comments.Application;

namespace Comments.Core.Specifications;

public class FileSaveResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileType FileType { get; set; }
    public string? ThumbnailPath { get; set; }
}