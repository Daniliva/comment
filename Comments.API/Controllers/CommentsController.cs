using Comments.Core.DTOs.Requests;
using Comments.Core.DTOs.Responses;
using Comments.Core.Exceptions;
using Comments.Core.Interfaces;
using Comments.Infrastructure.Validators;
using Microsoft.AspNetCore.Mvc;

namespace Comments.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;
        private readonly CreateCommentRequestValidator _createCommentValidator;
        private readonly GetCommentsRequestValidator _getCommentsValidator;

        public CommentsController(
            ICommentService commentService,
            ILogger<CommentsController> logger,
            CreateCommentRequestValidator createCommentValidator,
            GetCommentsRequestValidator getCommentsValidator)
        {
            _commentService = commentService;
            _logger = logger;
            _createCommentValidator = createCommentValidator;
            _getCommentsValidator = getCommentsValidator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<CommentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetComments([FromQuery] GetCommentsRequest request)
        {
            var validationResult = await _getCommentsValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                ));
            }

            try
            {
                var result = await _commentService.GetCommentsAsync(request);
                return Ok(ApiResponse<PagedResponse<CommentResponse>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving comments"
                ));
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CommentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComment(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentAsync(id);
                if (comment == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Comment not found"));
                }

                return Ok(ApiResponse<CommentResponse>.SuccessResponse(comment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment with ID {CommentId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving the comment"
                ));
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CommentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentRequest request)
        {
            var validationResult = await _createCommentValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                ));
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

                var comment = await _commentService.CreateCommentAsync(request, ipAddress, userAgent);

                return CreatedAtAction(nameof(GetComment), new { id = comment.Id },
                    ApiResponse<CommentResponse>.SuccessResponse(comment, "Comment created successfully"));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while creating the comment"
                ));
            }
        }

        [HttpGet("{parentId:int}/replies")]
        [ProducesResponseType(typeof(ApiResponse<List<CommentResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReplies(int parentId)
        {
            try
            {
                var replies = await _commentService.GetRepliesAsync(parentId);
                return Ok(ApiResponse<List<CommentResponse>>.SuccessResponse(replies.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving replies for comment {ParentId}", parentId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving replies"
                ));
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Comment not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Comment deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment with ID {CommentId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while deleting the comment"
                ));
            }
        }
    }
}

