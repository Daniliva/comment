using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Comments.Core.DTOs.Requests;
using Comments.Core.DTOs.Responses;
using Comments.Core.Exceptions;
using Comments.Core.Interfaces;
using Comments.Core.Specifications;

namespace Comments.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICaptchaService _captchaService;
        private readonly IFileService _fileService;
        private readonly IHtmlSanitizerService _htmlSanitizer;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ICommentRepository commentRepository,
            IUserRepository userRepository,
            ICaptchaService captchaService,
            IFileService fileService,
            IHtmlSanitizerService htmlSanitizer,
            IMapper mapper,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _captchaService = captchaService;
            _fileService = fileService;
            _htmlSanitizer = htmlSanitizer;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<CommentResponse>> GetCommentsAsync(GetCommentsRequest request)
        {
            var specification = new CommentSpecification
            {
                Page = request.Page,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                SortDescending = request.SortDescending,
                ParentId = request.ParentId
            };

            var pagedComments = await _commentRepository.GetCommentsAsync(specification);
            return _mapper.Map<PagedResponse<CommentResponse>>(pagedComments);
        }

        public async Task<CommentResponse?> GetCommentAsync(int id)
        {
            var comment = await _commentRepository.GetCommentWithRepliesAsync(id);
            return comment != null ? _mapper.Map<CommentResponse>(comment) : null;
        }

        public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, string ipAddress, string userAgent)
        {
            // Validate CAPTCHA
            var isCaptchaValid = await _captchaService.ValidateCaptchaAsync(request.CaptchaId, request.CaptchaCode);
            if (!isCaptchaValid)
            {
                throw new ValidationException("Invalid CAPTCHA");
            }

            // Get or create user
            var user = await _userRepository.GetOrCreateUserAsync(
                request.UserName,
                request.Email,
                request.HomePage,
                ipAddress,
                userAgent);

            // Sanitize HTML
            var sanitizedHtml = _htmlSanitizer.Sanitize(request.Text);

            // Handle file upload
            string? filePath = null;
            string? fileName = null;
            string? fileExtension = null;
            long? fileSize = null;
            FileType? fileType = null;
            string? thumbnailPath = null;

            if (request.File != null)
            {
                var fileResult = await _fileService.SaveFileAsync(request.File);
                filePath = fileResult.FilePath;
                fileName = fileResult.FileName;
                fileExtension = fileResult.FileExtension;
                fileSize = fileResult.FileSize;
                fileType = fileResult.FileType;
                thumbnailPath = fileResult.ThumbnailPath;
            }

            // Validate parent comment exists if ParentId is provided
            if (request.ParentId.HasValue)
            {
                var parentExists = await _commentRepository.ExistsAsync(c => c.Id == request.ParentId.Value);
                if (!parentExists)
                {
                    throw new ValidationException("Parent comment not found");
                }
            }

            // Create comment
            var comment = new Comment
            {
                UserId = user.Id,
                ParentId = request.ParentId,
                Text = request.Text,
                TextHtml = sanitizedHtml,
                FileName = fileName,
                FileExtension = fileExtension,
                FileSize = fileSize,
                FilePath = filePath,
                FileType = fileType,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            // Mark CAPTCHA as used
            await _captchaService.MarkAsUsedAsync(request.CaptchaId);

            _logger.LogInformation("Comment created with ID {CommentId} by user {UserName}", comment.Id, user.UserName);

            var response = _mapper.Map<CommentResponse>(comment);
            if (response.File != null && thumbnailPath != null)
            {
                response.File.ThumbnailPath = thumbnailPath;
            }

            return response;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return false;
            }

            var hasReplies = await _commentRepository.ExistsAsync(c => c.ParentId == id);
            if (hasReplies)
            {
                throw new InvalidOperationException("Cannot delete comment with replies");
            }

            if (!string.IsNullOrEmpty(comment.FilePath))
            {
                await _fileService.DeleteFileAsync(comment.FilePath);
            }

            _commentRepository.Remove(comment);
            await _commentRepository.SaveChangesAsync();

            _logger.LogInformation("Comment with ID {CommentId} deleted", id);
            return true;
        }

        public async Task<IEnumerable<CommentResponse>> GetRepliesAsync(int parentId)
        {
            var replies = await _commentRepository.GetRepliesAsync(parentId);
            return _mapper.Map<IEnumerable<CommentResponse>>(replies);
        }
    }
}
