using Comments.Core.DTOs.Requests;
using Comments.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;


namespace Comments.Infrastructure.Validators
    {
        public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
        {
            private readonly IHtmlSanitizerService _htmlSanitizer;

            public CreateCommentRequestValidator(IHtmlSanitizerService htmlSanitizer)
            {
                _htmlSanitizer = htmlSanitizer;

                RuleFor(x => x.UserName)
                    .NotEmpty().WithMessage("Username is required")
                    .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
                    .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Username can only contain letters and numbers");

                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required")
                    .EmailAddress().WithMessage("Invalid email format")
                    .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

                RuleFor(x => x.HomePage)
                    .Must(BeAValidUrl).WithMessage("Invalid URL format")
                    .When(x => !string.IsNullOrEmpty(x.HomePage));

                RuleFor(x => x.Text)
                    .NotEmpty().WithMessage("Comment text is required")
                    .Length(1, 5000).WithMessage("Comment must be between 1 and 5000 characters")
                    .Must(BeValidHtml).WithMessage("Comment contains invalid HTML tags");

                RuleFor(x => x.CaptchaId)
                    .NotEmpty().WithMessage("Captcha ID is required");

                RuleFor(x => x.CaptchaCode)
                    .NotEmpty().WithMessage("Captcha code is required")
                    .Length(4, 10).WithMessage("Captcha code must be between 4 and 10 characters");

                RuleFor(x => x.File)
                    .Must(BeValidFile).WithMessage("Invalid file type or size")
                    .When(x => x.File != null);
            }

            private bool BeAValidUrl(string? url)
            {
                if (string.IsNullOrEmpty(url)) return true;
                return Uri.TryCreate(url, UriKind.Absolute, out _);
            }

            private bool BeValidHtml(string html)
            {
                var sanitized = _htmlSanitizer.Sanitize(html);
                return sanitized == html || !string.IsNullOrEmpty(sanitized);
            }

            private bool BeValidFile(IFormFile? file)
            {
                if (file == null) return true;

                var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                var allowedTextTypes = new[] { "text/plain" };

                if (file.ContentType.StartsWith("image/"))
                {
                    return allowedImageTypes.Contains(file.ContentType) && file.Length <= 5 * 1024 * 1024;
                }

                if (file.ContentType == "text/plain")
                {
                    return file.Length <= 100 * 1024;
                }

                return false;
            }
        }
    }

