using Comments.Application;
using Comments.Core.DTOs.Requests;
using Comments.Core.DTOs.Responses;

using Comments.Core.Specifications;
using Nest;
using AutoMapper;
using Profile = AutoMapper.Profile;
namespace Comments.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Comment mappings
            CreateMap<Comment, CommentResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.HomePage, opt => opt.MapFrom(src => src.User.HomePage))
                .ForMember(dest => dest.File, opt => opt.MapFrom(src => src.FilePath != null ? new FileInfoResponse
                {
                    FileName = src.FileName!,
                    FileExtension = src.FileExtension!,
                    FileSize = src.FileSize ?? 0,
                    FilePath = src.FilePath!,
                    FileType = src.FileType!.Value,
                    ThumbnailPath = null // Will be set in service if available
                } : null))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

            CreateMap<CreateCommentRequest, Comment>()
                .ForMember(dest => dest.TextHtml, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore());

            // User mappings
            CreateMap<CreateCommentRequest, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.HomePage, opt => opt.MapFrom(src => src.HomePage))
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            // Captcha mappings
            CreateMap<Captcha, CaptchaResponse>()
                .ForMember(dest => dest.CaptchaId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ImageData, opt => opt.Ignore());

            // Paged list mappings
            CreateMap<PagedList<Comment>, PagedResponse<CommentResponse>>()
                .ConvertUsing<PagedListConverter<Comment, CommentResponse>>();
        }
    }
}