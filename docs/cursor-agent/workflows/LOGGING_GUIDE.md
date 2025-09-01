# FHIRAI - Logging Guide

## 🎯 Purpose
Centralized logging requirements and patterns for FHIRAI. This guide consolidates policies that were previously scattered across multiple docs. Follow this as the single source of truth for logging.

## 🔑 Core Policies
- **Structured logging only**: Use message templates with named placeholders, e.g., `"User {UserId} created TodoItem {Id}"`.
- **No PII/PHI in logs**: Never log raw personal data. Use masking when absolutely necessary.
- **Audit logging required for healthcare operations**: Log data access and changes via FHIR `AuditEvent` and application audit trails.
- **Operational context**: Include `UserId`, `TenantId`, `RequestId/CorrelationId` when available.
- **Correct log levels**: Trace/Debug for diagnostics, Information for normal ops, Warning for unusual but handled states, Error for failures, Critical for system-threatening conditions.

## 🧩 MediatR Pipeline Logging
Register logging behaviours in this order (request pre-processing first):

```csharp
// Application.DependencyInjection
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

    cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));     // Log incoming request
    cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));     // Catch & log exceptions
    cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));          // Check authorization
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));             // Validate request
    cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));            // Monitor performance
});
```

Behaviours provide: request logging, exception logging, slow request detection, and consistent cross-cutting logging.

## 🩺 Healthcare Audit Logging (FHIR)
- Use FHIR `AuditEvent` for healthcare data access and operations.
- Maintain an append-only audit trail; never mutate audit history.
- Do not include PII/PHI in log text. Log identifiers and metadata only.
- Reference implementation samples exist in `HEALTHCARE_DATA_PATTERN_REFERENCE.md` (Audit Service and Security Service examples).

Minimal pattern for application-level audit service:
```csharp
_logger.LogInformation("Healthcare data audit: {Action} on {ResourceType} {ResourceId} by user {UserId}",
    action, resourceType, resourceId, currentUserService.UserId);
```

## 🔒 PII/PHI Masking
- Never print raw emails, phone numbers, addresses, or clinical details.
- Use masking helpers (see `SECURITY_GUIDE.md` Data Masking section) before logging anything derived from user input or PHI.
- Prefer identifiers (IDs) over raw content. Example: `PatientId`, `FhirId`.

## 🧪 Log Levels Cheatsheet
```csharp
_logger.LogTrace("Entering {Method} with {@Params}", methodName, parameters);
_logger.LogDebug("Processing TodoItem {Id} with priority {Priority}", id, priority);
_logger.LogInformation("User {UserId} created TodoItem {Id}", userId, id);
_logger.LogWarning("TodoItem {Id} missing due date", id);
_logger.LogError(ex, "Failed to save TodoItem {Id}", id);
_logger.LogCritical(ex, "Database connection failed for TodoItem {Id}", id);
```

## ⚙️ Configuration
Use structured console logging. Example appsettings:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "FHIRAI": "Information"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
      }
    }
  }
}
```

Program setup:
```csharp
builder.Logging.AddJsonConsole(options =>
{
    options.JsonWriterOptions = new JsonWriterOptions { Indented = true };
});
```

## ✅ Implementation Checklist
- [ ] Use structured logging placeholders everywhere
- [ ] Do not log PII/PHI; apply masking if needed
- [ ] Include `UserId`, `TenantId`, `RequestId/CorrelationId` where available
- [ ] Register logging, exception, validation, performance behaviours
- [ ] Add audit logging for healthcare operations
- [ ] Validate log levels are appropriate and not noisy

## 🔗 Related References
- `CODE_PATTERNS.md`: Handler/endpoint logging examples and MediatR behaviours
- `HEALTHCARE_DATA_PATTERN_REFERENCE.md`: FHIR audit logging patterns and healthcare observability
- `SECURITY_GUIDE.md`: Data masking and security checklists


