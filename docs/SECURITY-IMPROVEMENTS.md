# Security Improvements Report

**Date:** March 2025  
**Branch:** feature/security-improvements  
**Scope:** Blogsphere.Notification.Service

---

## Executive Summary

Four security enhancements were implemented to harden the notification service: rate limiting for email sends, input validation for messages and storage keys, HTML sanitization of email template values, and safe exception logging that avoids exposing sensitive data.

---

## 1. Rate Limiting for Sending Emails

### Problem
No throttling existed on email sends. A misconfigured or malicious producer could flood the system with messages, leading to SMTP abuse, resource exhaustion, or blocking by email providers.

### Solution
A configurable sliding-window rate limiter restricts the number of emails sent per minute.

### Changes

| File | Change |
|------|--------|
| `Configurations/RateLimiterOption.cs` | **New.** Options: `Enabled`, `MaxEmailsPerMinute` (default: 60) |
| `Services/RateLimiting/IEmailRateLimiter.cs` | **New.** Interface with `TryAcquire()` |
| `Services/RateLimiting/SlidingWindowRateLimiter.cs` | **New.** In-memory sliding window implementation |
| `Services/EmailService.cs` | Call `TryAcquire()` before each send; break loop if limit reached |
| `appsettings.json` | Added `RateLimiter` section |
| `docker-compose.override.yml` | Added `RateLimiter__Enabled`, `RateLimiter__MaxEmailsPerMinute` env vars |

### Configuration

```json
"RateLimiter": {
  "Enabled": true,
  "MaxEmailsPerMinute": 60
}
```

---

## 2. Validation of Emails, PartitionKey/RowKey, and Message Payloads

### Problem
- Emails were used as Azure Table PartitionKey without format checks.
- MessageId was used as RowKey without validation.
- Large payloads could cause performance or storage issues.

### Solution
A shared validation service checks email format, Azure Table key rules, and payload size before processing.

### Changes

| File | Change |
|------|--------|
| `Models/Validation/ValidationResult.cs` | **New.** `IsValid` and `Errors` for validation results |
| `Services/Validation/IValidationService.cs` | **New.** `ValidateEmail`, `ValidateTableKey`, `ValidatePayloadSize` |
| `Services/Validation/ValidationService.cs` | **New.** Regex for email, Azure key rules (length, invalid chars), 64KB payload limit |
| `EventBus/ConsumerValidationExtensions.cs` | **New.** `ValidateNotificationPayload` extension used by all consumers |
| All 6 Consumers | Call `ValidateNotificationPayload` before processing; return early on failure (message is acked, not retried) |

### Validation Rules

- **Email:** Non-empty, max 254 chars, regex for valid format.
- **PartitionKey/RowKey:** Non-empty, max 1024 chars, no `\ / # ? \t \n \r`, no leading/trailing whitespace.
- **Payload:** Max 64 KB (configurable).

### Affected Consumers
- AuthCodeSentConsumer  
- UserInvitationSentConsumer  
- PasswordResetInstructionSentConsumer  
- PasswordResetOneTimeCodeSentConsumer  
- ManagementUserWelcomeEmailSentConsumer  
- ManagementUserPasswordEmailSentConsumer  

---

## 3. HTML Sanitization of Template Values

### Problem
Email template values were inserted with `string.Replace` without encoding. Malicious content (e.g. `<script>`, HTML) could lead to XSS or HTML injection in rendered emails.

### Solution
All template values are HTML-encoded before substitution using `System.Net.WebUtility.HtmlEncode`.

### Changes

| File | Change |
|------|--------|
| `Services/Sanitization/IHtmlSanitizer.cs` | **New.** `Sanitize(string value)` |
| `Services/Sanitization/HtmlSanitizer.cs` | **New.** Uses `WebUtility.HtmlEncode` |
| `Services/EmailService.cs` | Injects `IHtmlSanitizer`; sanitizes each `TemplateFields.Value` before `Replace` |

### Behavior
- Characters such as `<`, `>`, `"`, `'`, `&` are encoded.
- Template keys (placeholders) are unchanged; only values are sanitized.

---

## 4. Safe Exception Logging

### Problem
Full exceptions (including stack traces, connection strings, and paths) were logged with `_logger.Error(ex, ...)`, risking exposure of sensitive data.

### Solution
New extension methods log only a truncated exception message and exception type, plus correlation ID when available. Stack traces are never logged.

### Changes

| File | Change |
|------|--------|
| `Extensions/LoggerExtensions.cs` | Added `LogErrorSafely`, `LogWarningSafely`; private `LogExceptionSafely`; `Truncate` (max 200 chars) |
| `Services/EmailServiceBase.cs` | Replaced `Error(ex, ...)` with `LogErrorSafely(ex, null, ...)` on SMTP connection failure |
| `Services/EmailService.cs` | Replaced `Error(ex, ...)` with `LogErrorSafely(ex, notification.CorrelationId, ...)` |
| `BackgroundJobs/ErrorQueueReprocessorJob.cs` | Replaced `Error(ex, ...)` and `Warning(ex, ...)` with `LogErrorSafely` and `LogWarningSafely` |
| All 6 Consumers | Replaced `Error(ex.Message, ...)` with `LogErrorSafely(ex, context.Message.CorrelationId, ...)` |

### Logged Fields
- `ExceptionType` (e.g. `InvalidOperationException`)
- `SafeMessage` (exception message truncated to 200 chars)
- `CorrelationId` (when available)

---

## Dependency Injection Updates

| Service | Lifetime | Registration |
|---------|----------|--------------|
| `IEmailRateLimiter` | Singleton | `SlidingWindowRateLimiter` |
| `IValidationService` | Singleton | `ValidationService` |
| `IHtmlSanitizer` | Singleton | `HtmlSanitizer` |

---

## Files Summary

### New Files (11)
- `Configurations/RateLimiterOption.cs`
- `Models/Validation/ValidationResult.cs`
- `Services/RateLimiting/IEmailRateLimiter.cs`
- `Services/RateLimiting/SlidingWindowRateLimiter.cs`
- `Services/Validation/IValidationService.cs`
- `Services/Validation/ValidationService.cs`
- `Services/Sanitization/IHtmlSanitizer.cs`
- `Services/Sanitization/HtmlSanitizer.cs`
- `EventBus/ConsumerValidationExtensions.cs`

### Modified Files (14)
- `DI/ServiceCollectionConfigurationExtensions.cs` – RateLimiterOption
- `DI/ServiceCollectionExtensions.cs` – New services
- `Services/EmailService.cs` – Rate limiter, sanitizer, safe logging
- `Services/EmailServiceBase.cs` – Safe logging
- `Extensions/LoggerExtensions.cs` – `LogErrorSafely`, `LogWarningSafely`
- `BackgroundJobs/ErrorQueueReprocessorJob.cs` – Safe logging
- `EventBus/Consumers/AuthCodeSentConsumer.cs` – Validation, safe logging
- `EventBus/Consumers/UserInvitationSentConsumer.cs` – Validation, safe logging
- `EventBus/Consumers/PasswordResetInstructionSentConsumer.cs` – Validation, safe logging
- `EventBus/Consumers/PasswordResetOneTimeCodeSentConsumer.cs` – Validation, safe logging
- `EventBus/Consumers/ManagementUserWelcomeEmailSentConsumer.cs` – Validation, safe logging
- `EventBus/Consumers/ManagementUserPasswordEmailSentConsumer.cs` – Validation, safe logging
- `appsettings.json` – RateLimiter config
- `docker-compose.override.yml` – RateLimiter env vars

---

## Business Logic Impact

- **No behavioral change:** Flow remains the same; validation, sanitization, and rate limiting are added around existing logic.
- **Invalid messages:** Consumers return early on validation failure; the message is acked, so it is not retried.
- **Rate limiting:** When the limit is reached, remaining queued notifications are skipped for that cycle and processed in the next run.
- **Template placeholders:** Placeholder keys (e.g. `[Email]`, `[Code]`) are unchanged; only user-supplied values are sanitized.

---

## Testing Recommendations

1. **Rate limiting:** Send more than `MaxEmailsPerMinute` notifications; verify only the first N are sent.
2. **Validation:** Publish messages with invalid emails, invalid keys, or oversized payloads; verify they are rejected and acked.
3. **Sanitization:** Use template values containing `<script>`, `&`, `"`; verify they are encoded in the sent email.
4. **Safe logging:** Trigger an error; verify logs contain `ExceptionType` and `SafeMessage` but no stack traces or connection strings.
