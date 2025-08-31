using MediatR;

namespace FHIRAI.Application.Fhir.Commands.ImportFhirBundle;

/// <summary>
/// Command to import FHIR resources from a FHIR Bundle
/// </summary>
public record ImportFhirBundleCommand : IRequest<ImportFhirBundleResponse>
{
    /// <summary>
    /// FHIR Bundle JSON content
    /// </summary>
    public string BundleJson { get; init; } = string.Empty;

    /// <summary>
    /// Whether to validate FHIR resources before import
    /// </summary>
    public bool ValidateResources { get; init; } = true;

    /// <summary>
    /// Whether to skip existing resources
    /// </summary>
    public bool SkipExisting { get; init; } = false;

    /// <summary>
    /// Whether to update existing resources
    /// </summary>
    public bool UpdateExisting { get; init; } = true;

    /// <summary>
    /// Import strategy for handling conflicts
    /// </summary>
    public ImportStrategy Strategy { get; init; } = ImportStrategy.CreateOrUpdate;
}

/// <summary>
/// Response for importing FHIR Bundle
/// </summary>
public record ImportFhirBundleResponse
{
    /// <summary>
    /// Total number of resources processed
    /// </summary>
    public int TotalProcessed { get; init; }

    /// <summary>
    /// Number of successfully imported resources
    /// </summary>
    public int SuccessfullyImported { get; init; }

    /// <summary>
    /// Number of resources that failed to import
    /// </summary>
    public int FailedToImport { get; init; }

    /// <summary>
    /// Number of resources that were skipped
    /// </summary>
    public int Skipped { get; init; }

    /// <summary>
    /// Number of resources that were updated
    /// </summary>
    public int Updated { get; init; }

    /// <summary>
    /// Import job ID for tracking
    /// </summary>
    public string ImportJobId { get; init; } = string.Empty;

    /// <summary>
    /// Import timestamp
    /// </summary>
    public DateTimeOffset ImportedAt { get; init; }

    /// <summary>
    /// List of imported resource IDs
    /// </summary>
    public List<ImportedResource> ImportedResources { get; init; } = new();

    /// <summary>
    /// List of import errors
    /// </summary>
    public List<ImportError> Errors { get; init; } = new();
}

/// <summary>
/// Information about an imported resource
/// </summary>
public record ImportedResource
{
    /// <summary>
    /// Resource type
    /// </summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>
    /// Resource ID
    /// </summary>
    public string FhirId { get; init; } = string.Empty;

    /// <summary>
    /// Resource composite key (ResourceType/FhirId)
    /// </summary>
    public string CompositeKey { get; init; } = string.Empty;

    /// <summary>
    /// Import status
    /// </summary>
    public ImportStatus Status { get; init; }

    /// <summary>
    /// Resource version after import
    /// </summary>
    public int VersionId { get; init; }

    /// <summary>
    /// Error message if import failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Import error details
/// </summary>
public record ImportError
{
    /// <summary>
    /// Resource type
    /// </summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>
    /// Original resource ID from bundle
    /// </summary>
    public string? OriginalId { get; init; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Error code
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>
    /// Error severity
    /// </summary>
    public ErrorSeverity Severity { get; init; } = ErrorSeverity.Error;
}

/// <summary>
/// Import status enumeration
/// </summary>
public enum ImportStatus
{
    /// <summary>
    /// Successfully imported
    /// </summary>
    Success,

    /// <summary>
    /// Import failed
    /// </summary>
    Failed,

    /// <summary>
    /// Skipped (already exists)
    /// </summary>
    Skipped,

    /// <summary>
    /// Updated existing resource
    /// </summary>
    Updated
}

/// <summary>
/// Import strategy enumeration
/// </summary>
public enum ImportStrategy
{
    /// <summary>
    /// Create new resources only
    /// </summary>
    CreateOnly,

    /// <summary>
    /// Update existing resources only
    /// </summary>
    UpdateOnly,

    /// <summary>
    /// Create new or update existing
    /// </summary>
    CreateOrUpdate,

    /// <summary>
    /// Skip existing resources
    /// </summary>
    SkipExisting
}

/// <summary>
/// Error severity enumeration
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Information
    /// </summary>
    Information,

    /// <summary>
    /// Warning
    /// </summary>
    Warning,

    /// <summary>
    /// Error
    /// </summary>
    Error,

    /// <summary>
    /// Fatal
    /// </summary>
    Fatal
}
