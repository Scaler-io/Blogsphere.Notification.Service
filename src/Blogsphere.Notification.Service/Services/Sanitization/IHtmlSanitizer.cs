namespace Blogsphere.Notification.Service.Services.Sanitization;

public interface IHtmlSanitizer
{
    string Sanitize(string value);
}
