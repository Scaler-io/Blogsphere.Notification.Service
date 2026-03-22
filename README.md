# Blogsphere.Notification.Service

A .NET 8 background service that processes notification events from RabbitMQ and delivers **email** and **SMS** notifications. Part of the Blogsphere ecosystem, it consumes event messages, persists notification records to Azure Table Storage, renders HTML email templates from Azure Blob Storage, and sends outbound mail via SMTP. SMS notifications are stored with the same history model and sent through SMTP in development (for example Mailtrap), using a dedicated SMS pipeline and rate limiting.

## Overview

The service acts as a notification hub that:

- **Consumes events** from RabbitMQ via MassTransit (AuthCode, User Invitation, Password Reset, Management User flows, Phone Verification)
- **Persists notification history** to Azure Table Storage for idempotency and auditing (per channel: email or SMS)
- **Renders emails** using HTML templates stored in Azure Blob Storage
- **Sends emails** via SMTP (e.g., Mailtrap for development)
- **Sends SMS** by dequeueing pending rows on the SMS channel, validating payloads, applying a sliding-window SMS rate limit, and delivering via SMTP to a configured test inbox / sender pair in development
- **Reprocesses failed messages** from RabbitMQ error queues with configurable retry limits

## Supported Notification Types

| Event | Channel | Description |
|-------|---------|-------------|
| **UserInvitation** | Email | Welcome/invitation email when a user is invited |
| **AuthCodeSent** | Email | 2FA / authentication code email |
| **PasswordResetInstructionSent** | Email | Password reset instructions email |
| **PasswordResetOneTimeCodeSent** | Email | One-time code (OTP) for password reset |
| **ManagementUserWelcomeEmailSent** | Email | Welcome email for management portal users |
| **ManagementUserPasswordEmailSent** | Email | Password email for management portal users |
| **PhoneVerificationCodeSent** | SMS | Phone verification code (queued for SMS delivery) |

## Architecture

**How to read this chart:** the **happy path** is top-to-bottom: messages enter RabbitMQ, consumers validate and write **Azure Table Storage**, then **EmailProcessingJob** / **SmsProcessingJob** poll unpublished rows and send via **SmtpClientFactory**. **Error queue reprocessing is not a later stage after SMTP** — it runs only when a **consumer** fails (after MassTransit retries). Those messages sit in `*_error` queues; **ErrorQueueReprocessorJob** republishes to the **original exchange and routing key** (`BasicPublishAsync` using `MT-OriginalExchange` / `MT-OriginalRoutingKey`), so they **re-enter the same intake** at RabbitMQ. Failed **SMTP** sends stay in Table Storage (logged); they do **not** use this RabbitMQ retry loop.

Diagram source: [`docs/assets/architecture-flowchart.mmd`](docs/assets/architecture-flowchart.mmd) (keep in sync with the block below).

```mermaid
flowchart TB
    RMQ[(RabbitMQ broker)]

    subgraph intake["Consumer intake (happy path)"]
        MT[MassTransit consumers]
        VAL[Payload and key validation]
        RMQ --> MT --> VAL --> TBL[(Azure Table Storage\nnotification history)]
    end

    subgraph emailOut["Email outbound (polls Table Storage)"]
        EJOB[EmailProcessingJob]
        ESVC[EmailService]
        RL_E[Sliding-window rate limit\nEmailRateLimit]
        MERGE[Blob HTML template +\nHtmlSanitizer on field values]
        EJOB --> ESVC --> RL_E --> MERGE
    end

    subgraph smsOut["SMS outbound (polls Table Storage)"]
        SJOB[SmsProcessingJob]
        SSVC[SmsService]
        RL_S[Sliding-window rate limit\nSmsRateLimit]
        MSG[MimeMessage\n(test inbox / dev SMTP)]
        SJOB --> SSVC --> RL_S --> MSG
    end

    TBL -->|"Channel = Email,\nIsPublished = false"| EJOB
    TBL -->|"Channel = SMS,\nIsPublished = false"| SJOB

    MERGE --> FCY[Shared SmtpClientFactory\nMailKit SMTP send]
    MSG --> FCY

    subgraph retry["Consumer failure only — parallel retry loop"]
        ERR[MassTransit *_error queues\n(consumer threw after retries)]
        REP[ErrorQueueReprocessorJob\nBasicPublish to MT-OriginalExchange\n+ MT-OriginalRoutingKey]
        ERR --> REP --> RMQ
    end

    MT -.->|fault to error queue| ERR
```

## Tech Stack

- **.NET 8** - Console/worker app
- **MassTransit.RabbitMQ** - Event consumption
- **Azure.Data.Tables** - Notification history persistence
- **Azure.Storage.Blobs** - HTML email template storage
- **MailKit** - SMTP delivery for email and for the SMS pipeline (SMTP-backed in development)
- **Serilog** - Structured logging
- **OpenTelemetry** - Zipkin/Jaeger tracing

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [RabbitMQ](https://www.rabbitmq.com/)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (for local Azure Storage emulation)
- SMTP server (e.g., [Mailtrap](https://mailtrap.io/) for development)

## Configuration

Key settings in `appsettings.json`:

| Section | Key | Description |
|---------|-----|-------------|
| **EventBus** | Host, Username, Password, VirtualHost | RabbitMQ connection |
| **EmailTemplates** | *per event* | Blob template name per notification type (email flows) |
| **EmailSettings** | Server, Port, UserName, Password, CompanyAddress | SMTP server for email sending |
| **SmsSettings** | TestInboxAddress, FromAddress | SMTP “SMS” delivery in dev: inbox that receives messages and required sender address |
| **RateLimiter** | Enabled, MaxEmailsPerMinute, MaxSmsPerMinute | Per-minute caps for email and SMS sending |
| **ConnectionStrings** | AzureTableStorage, BlobStorage | Azure Storage endpoints |
| **AppConfigurations** | NotificationProcessInterval, IntervalUnit | Polling interval for both email and SMS background jobs |
| **ErrorQueueReprocessor** | Enabled, PollIntervalSeconds, MaxAttempts, ErrorQueues | Error queue reprocessing (includes `phone-verification-code-sent_error` when SMS is enabled) |

## Running Locally

1. **Start dependencies** (RabbitMQ, Azurite) - e.g. via `docker-compose` in the parent Blogsphere repo or your local setup.

2. **Configure** `appsettings.json` or `appsettings.Development.json`:
   - EventBus: RabbitMQ host, credentials
   - EmailSettings: SMTP server (e.g., Mailtrap) for email
   - SmsSettings: `TestInboxAddress` and `FromAddress` for the SMS pipeline (SMTP-backed in development)
   - RateLimiter: email and SMS per-minute limits as needed
   - ConnectionStrings: Azure Table and Blob endpoints (use Azurite for local dev)

3. **Upload HTML templates** to Azure Blob Storage (or Azurite) in the `templates` container. Template names must match `EmailTemplates` config (e.g., `UserInvitationSent.html`, `AuthCodeSent.html`). SMS flows do not use these templates; they rely on persisted notification payload data.

4. **Run the service**:
   ```bash
   cd src/Blogsphere.Notification.Service
   dotnet run
   ```

## Running with Docker

From the `src` directory:

```bash
# Create the external network (if not exists)
docker network create blogsphere_dev_net

# Start Azurite + Notification Service
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

The service depends on:
- **blogspherenfstrg** (Azurite) for Azure Table/Blob emulation
- **RabbitMQ** (from the parent Blogsphere setup)
- Env vars: `RABBITMQ_PASSWORD`, `EMAIL_USERNAME`, `EMAIL_PASSWORD`, `CONNECTION_STRING_AZURE_TABLE_STORAGE`, `CONNECTION_STRING_BLOB_STORAGE`

## Project Structure

```
src/Blogsphere.Notification.Service/
|-- BackgroundJobs/
|   |-- EventBusStarterJob.cs       # Starts MassTransit
|   |-- EmailProcessingJob.cs       # Polls Table Storage, sends email notifications
|   |-- SmsProcessingJob.cs         # Polls Table Storage, sends SMS-channel notifications
|   +-- ErrorQueueReprocessorJob.cs # Reprocesses RabbitMQ error queues
|-- Configurations/                # Options classes for appsettings
|-- Data/Storage/                  # Table and Blob repositories
|-- EventBus/
|   |-- Consumers/                 # MassTransit consumers per event type
|   +-- Contracts/                 # Event message definitions
|-- Models/                        # Constants, enums, DTOs
|-- Services/                      # EmailService, SmsService, validation, rate limiting
+-- Program.cs
```

## Error Queue Reprocessing

Failed messages land in RabbitMQ error queues (e.g., `auth-code-sent_error`, `phone-verification-code-sent_error`). The `ErrorQueueReprocessorJob` periodically moves messages back to the original exchange for retry, up to `MaxAttempts`. Configure which queues to reprocess in `ErrorQueueReprocessor:ErrorQueues`.

## License

See [LICENSE](LICENSE) for details.
