using Comments.Core.Specifications;
using Microsoft.AspNetCore.Http;

namespace Comments.Core.Interfaces;

public interface IFileService
{
    Task<FileSaveResult> SaveFileAsync(IFormFile file);
    Task DeleteFileAsync(string filePath);
    Task<Stream> GetFileAsync(string filePath);
    bool FileExists(string filePath);
}