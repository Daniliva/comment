using Comments.Core.Interfaces;
using Ganss.Xss;

namespace Comments.Infrastructure.Services;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();

        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.Add("a");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("strong");

        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("title");

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        _sanitizer.AllowedCssProperties.Clear();

        _sanitizer.KeepChildNodes = true;
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrEmpty(html))
            return html;

        return _sanitizer.Sanitize(html);
    }

    public bool IsValidHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return true;

        var sanitized = Sanitize(html);
        return sanitized == html;
    }
}