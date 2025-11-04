using Comments.Core.DTOs.Requests;
using Comments.Core.DTOs.Responses;
using Comments.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comments.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _captchaService;
    private readonly ILogger<CaptchaController> _logger;

    public CaptchaController(ICaptchaService captchaService, ILogger<CaptchaController> logger)
    {
        _captchaService = captchaService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CaptchaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateCaptcha()
    {
        try
        {
            var captcha = await _captchaService.GenerateCaptchaAsync();
            return Ok(ApiResponse<CaptchaResponse>.SuccessResponse(captcha));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CAPTCHA");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while generating CAPTCHA"
            ));
        }
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCaptcha([FromBody] ValidateCaptchaRequest request)
    {
        try
        {
            var isValid = await _captchaService.ValidateCaptchaAsync(request.CaptchaId, request.Code);
            return Ok(ApiResponse<bool>.SuccessResponse(isValid));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CAPTCHA");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while validating CAPTCHA"
            ));
        }
    }
}