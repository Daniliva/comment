using AutoMapper;
using Comments.Core.DTOs.Responses;
using Comments.Core.Events;
using Comments.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nest;

namespace Comments.API.Service
{
    public class CommentCreatedConsumer : IConsumer<CommentCreatedEvent>
    {
        private readonly IElasticClient _elasticClient;
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentCreatedConsumer> _logger;

        public CommentCreatedConsumer(IElasticClient elasticClient, ICommentRepository commentRepository, IMapper mapper, ILogger<CommentCreatedConsumer> logger)
        {
            _elasticClient = elasticClient;
            _commentRepository = commentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CommentCreatedEvent> context)
        {
            var comment = await _commentRepository.GetByIdAsync(context.Message.CommentId);
            if (comment != null)
            {
                var response = _mapper.Map<CommentResponse>(comment);
                await _elasticClient.IndexDocumentAsync(response);
                _logger.LogInformation("Indexed comment {CommentId} in Elasticsearch", context.Message.CommentId);
            }
        }
    }
}