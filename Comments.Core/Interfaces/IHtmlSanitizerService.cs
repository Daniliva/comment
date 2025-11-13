namespace Comments.Core.Interfaces
{
    public interface IHtmlSanitizerService
    {
        string Sanitize(string html);
        bool IsValidHtml(string html);
    }
}