using AutoMapper;
using Comments.Core.DTOs.Responses;
using Comments.Core.Specifications;

namespace Comments.Infrastructure.Mappings
{
    public class PagedListConverter<TSource, TDestination> : ITypeConverter<PagedList<TSource>, PagedResponse<TDestination>>
    {
        public PagedResponse<TDestination> Convert(PagedList<TSource> source, PagedResponse<TDestination> destination, ResolutionContext context)
        {
            var items = context.Mapper.Map<List<TDestination>>(source.Items);

            return new PagedResponse<TDestination>
            {
                Items = items,
                Page = source.Page,
                PageSize = source.PageSize,
                TotalCount = source.TotalCount
            };
        }
    }
}