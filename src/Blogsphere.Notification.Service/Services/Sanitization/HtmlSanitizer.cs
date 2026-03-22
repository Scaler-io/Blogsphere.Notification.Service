using System.Net;

namespace Blogsphere.Notification.Service.Services.Sanitization;

public sealed class HtmlSanitizer : IHtmlSanitizer
{
    public string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return WebUtility.HtmlEncode(value);
    }
}
