using System.Text.RegularExpressions;
using Blogsphere.Notification.Service.Extensions;
using Blogsphere.Notification.Service.Models.Validation;

namespace Blogsphere.Notification.Service.Services.Validation;

public sealed class ValidationService : IValidationService
{
    private const int MaxKeyLength = 1024;
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly char[] InvalidKeyChars = ['\\', '/', '#', '?', '\t', '\n', '\r'];

    public ValidationResult ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ValidationResult.Failure("Email is required");

        if (email.Length > 254)
            return ValidationResult.Failure("Email exceeds maximum length");

        if (!EmailRegex.IsMatch(email))
            return ValidationResult.Failure("Email format is invalid");

        return ValidationResult.Success();
    }

    public ValidationResult ValidateTableKey(string key, string paramName)
    {
        if (string.IsNullOrEmpty(key))
            return ValidationResult.Failure($"{paramName} is required");

        if (key.Length > MaxKeyLength)
            return ValidationResult.Failure($"{paramName} exceeds maximum length of {MaxKeyLength} characters");

        if (key.IndexOfAny(InvalidKeyChars) >= 0)
            return ValidationResult.Failure($"{paramName} contains invalid characters");

        if (char.IsWhiteSpace(key[0]) || char.IsWhiteSpace(key[^1]))
            return ValidationResult.Failure($"{paramName} cannot start or end with whitespace");

        return ValidationResult.Success();
    }

    public ValidationResult ValidatePayloadSize(int byteLength, int maxBytes = 64 * 1024)
    {
        if (byteLength > maxBytes)
            return ValidationResult.Failure($"Payload size ({byteLength} bytes) exceeds maximum allowed ({maxBytes} bytes)");

        return ValidationResult.Success();
    }

    public bool ValidateEmailNotificationPayload(NotificationPayloadValidationContext ctx)
    {
        var emailResult = ValidateEmail(ctx.PartitionKey);
        if (!emailResult.IsValid)
        {
            ctx.Logger.WithCorrelationId(ctx.CorrelationId).Warning(
                "Validation failed for {EventName}: {Errors}", ctx.EventName, string.Join("; ", emailResult.Errors));
            return false;
        }

        var partitionKeyResult = ValidateTableKey(ctx.PartitionKey, "PartitionKey");
        if (!partitionKeyResult.IsValid)
        {
            ctx.Logger.WithCorrelationId(ctx.CorrelationId).Warning(
                "Validation failed for {EventName}: {Errors}", ctx.EventName, string.Join("; ", partitionKeyResult.Errors));
            return false;
        }

        var rowKeyResult = ValidateTableKey(ctx.MessageId, "RowKey");
        if (!rowKeyResult.IsValid)
        {
            ctx.Logger.WithCorrelationId(ctx.CorrelationId).Warning(
                "Validation failed for {EventName}: {Errors}", ctx.EventName, string.Join("; ", rowKeyResult.Errors));
            return false;
        }

        var payloadResult = ValidatePayloadSize(ctx.PayloadByteLength);
        if (!payloadResult.IsValid)
        {
            ctx.Logger.WithCorrelationId(ctx.CorrelationId).Warning(
                "Validation failed for {EventName}: {Errors}", ctx.EventName, string.Join("; ", payloadResult.Errors));
            return false;
        }

        return true;
    }

    public bool ValidateSmsNotificationPayload(NotificationPayloadValidationContext ctx)
    {
        var partitionKeyResult = ValidateTableKey(ctx.PartitionKey, "PartitionKey");
        if (!partitionKeyResult.IsValid)
        {
            ctx.Logger.WithCorrelationId(ctx.CorrelationId).Warning(
                "Validation failed for {EventName}: {Errors}", ctx.EventName, string.Join("; ", partitionKeyResult.Errors));
            return false;
        }

        var rowKeyResult = ValidateTableKey(ctx.MessageId, "RowKey");
        if (!rowKeyResult.IsValid)
        {
            ctx.Logger.WithCorrelationId(ctx.CorrelationId).Warning(
                "Validation failed for {EventName}: {Errors}", ctx.EventName, string.Join("; ", rowKeyResult.Errors));
            return false;
        }

        var payloadResult = ValidatePayloadSize(ctx.PayloadByteLength);
        if (!payloadResult.IsValid)
        {
            ctx.Logger.WithCorrelationId(ctx.CorrelationId).Warning(
                "Validation failed for {EventName}: {Errors}", ctx.EventName, string.Join("; ", payloadResult.Errors));
            return false;
        }

        return true;
    }
}
