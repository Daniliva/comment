using System.Text.Json;
using AutoMapper;
using Comments.Core.DTOs.Requests;
using Comments.Core.DTOs.Responses;
using Comments.Core.Entities;
using Comments.Core.Exceptions;
using Comments.Core.Interfaces;
using Comments.Core.Specifications;
using Comments.Infrastructure.Data;
using Comments.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Nest;
using CommentCreatedEvent = Comments.API.CommentCreatedEvent;

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
        private readonly CustomWebSocketManager _wsManager;
        private readonly IElasticClient _elasticClient;
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IDistributedCache _cache; 
        private readonly IPublishEndpoint _publishEndpoint; 

        public CommentService(
            ICommentRepository commentRepository,
            IUserRepository userRepository,
            ICaptchaService captchaService,
            IFileService fileService,
            IHtmlSanitizerService htmlSanitizer,
            IMapper mapper,
            ILogger<CommentService> logger,
            CustomWebSocketManager wsManager,
            IElasticClient elasticClient,
            IHubContext<CommentHub> hubContext,
            IDistributedCache cache, 
            IPublishEndpoint publishEndpoint) 
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _captchaService = captchaService;
            _fileService = fileService;
            _htmlSanitizer = htmlSanitizer;
            _mapper = mapper;
            _logger = logger;
            _wsManager = wsManager;
            _elasticClient = elasticClient;
            _hubContext = hubContext;
            _cache = cache;
            _publishEndpoint = publishEndpoint;
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
            var response = _mapper.Map<PagedResponse<CommentResponse>>(pagedComments);

            return response;
        }

        public async Task<CommentResponse?> GetCommentAsync(int id)
        {
            var cacheKey = $"comment_{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<CommentResponse>(cachedData);
            }

            var comment = await _commentRepository.GetCommentWithRepliesAsync(id);
            if (comment == null) return null;

            var response = _mapper.Map<CommentResponse>(comment);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return response;
        }

        public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, string ipAddress, string userAgent)
        {
            var isCaptchaValid = await _captchaService.ValidateCaptchaAsync(request.CaptchaId, request.CaptchaCode);
            if (!isCaptchaValid)
            {
                throw new ValidationException("Invalid CAPTCHA"+ request.CaptchaId+" "+ request.CaptchaCode);
            }
            var user = await _userRepository.GetOrCreateUserAsync(
                request.UserName,
                request.Email,
                request.HomePage,
                ipAddress,
                userAgent);

            var sanitizedHtml = _htmlSanitizer.Sanitize(request.Text);

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


            if (request.ParentId.HasValue)
            {
                var parentExists = await _commentRepository.ExistsAsync(c => c.Id == request.ParentId.Value);
                if (!parentExists)
                {
                    throw new ValidationException("Parent comment not found");
                }
            }

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

            await _captchaService.MarkAsUsedAsync(request.CaptchaId);

            _logger.LogInformation("Comment created with ID {CommentId} by user {UserName}", comment.Id, user.UserName);

            var response = _mapper.Map<CommentResponse>(comment);
            
            await _publishEndpoint.Publish(new CommentCreatedEvent { CommentId = response.Id, UserName = response.UserName });
            
            await _hubContext.Clients.All.SendAsync("NewComment", response);
            await _wsManager.BroadcastAsync(new { Type = "NewComment", Data = response });

            if (response.File != null && thumbnailPath != null)
            {
                response.File.ThumbnailPath = thumbnailPath;
            }
            
            await _cache.RemoveAsync($"comment_{response.Id}");
       
            return response;
        }
        public async Task<List<CommentResponse>> SearchCommentsAsync(string query)
        {
            var response = await _elasticClient.SearchAsync<CommentResponse>(s => s
                .Query(q => q.MultiMatch(m => m
                    .Fields(f => f.Fields("text", "userName", "email"))
                    .Query(query)
                ))
                .Size(25)
            );
            return response.Documents.ToList();
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
                await _cache.RemoveAsync($"comment_{id}");
            await _cache.RefreshAsync($"comment_{id}");
              await _hubContext.Clients.All.SendAsync("DeletedComment", id);
            await _wsManager.BroadcastAsync(new { Type = "DeletedComment", Id = id });
            
        

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