using Comments.Core.DTOs.Requests;
using FluentValidation;

namespace Comments.Infrastructure.Validators;

public class GetCommentsRequestValidator : AbstractValidator<GetCommentsRequest>
{
    public GetCommentsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Invalid sort field");
    }

    private bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "CreatedAt", "UserName", "Email" };
        return validFields.Contains(sortBy);
    }
}