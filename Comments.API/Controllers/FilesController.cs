using Comments.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comments.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpGet("{*filePath}")]
    public async Task<IActionResult> GetFile(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest("File path is required");
            }

            if (!_fileService.FileExists(filePath))
            {
                return NotFound();
            }

            var stream = await _fileService.GetFileAsync(filePath);
            var contentType = GetContentType(filePath);

            return File(stream, contentType, Path.GetFileName(filePath));
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the file");
        }
    }

    private string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}