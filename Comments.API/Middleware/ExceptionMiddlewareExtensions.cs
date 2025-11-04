using Hellang.Middleware.ProblemDetails;
using Comments.Core.Exceptions;

namespace Comments.API.Middleware
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IServiceCollection AddCustomExceptionHandling(this IServiceCollection services, IWebHostEnvironment environment)
        {
            services.AddProblemDetails(options =>
            {
                options.IncludeExceptionDetails = (ctx, ex) => environment.IsDevelopment();

                options.Map<ValidationException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                });

                options.Map<NotFoundException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });

                options.Map<BusinessException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Business Rule Violation",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = ex.Message,
                    Type = "https://tools.ietf.org/html/rfc4918#section-11.2"
                });

                options.Map<Exception>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = environment.IsDevelopment() ? ex.Message : "An unexpected error occurred",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            });

            return services;
        }
    }
}