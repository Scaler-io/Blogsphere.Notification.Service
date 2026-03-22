using Blogsphere.Notification.Service.Models.Validation;

namespace Blogsphere.Notification.Service.Services.Validation;

public interface IValidationService
{
    ValidationResult ValidateEmail(string email);
    ValidationResult ValidateTableKey(string key, string paramName);
    ValidationResult ValidatePayloadSize(int byteLength, int maxBytes = 64 * 1024);
    bool ValidateEmailNotificationPayload(NotificationPayloadValidationContext ctx);
    bool ValidateSmsNotificationPayload(NotificationPayloadValidationContext ctx);
}
