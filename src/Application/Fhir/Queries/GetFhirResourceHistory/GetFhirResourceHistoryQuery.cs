using MediatR;

namespace FHIRAI.Application.Fhir.Queries.GetFhirResourceHistory;

/// <summary>
/// Query to get version history of a FHIR resource
/// </summary>
public record GetFhirResourceHistoryQuery : IRequest<GetFhirResourceHistoryResponse>
{
    /// <summary>
    /// FHIR resource type (e.g., Patient, Observation, etc.)
    /// </summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>
    /// FHIR resource ID
    /// </summary>
    public string FhirId { get; init; } = string.Empty;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; init; } = 100;

    /// <summary>
    /// Whether to include deleted versions
    /// </summary>
    public bool IncludeDeleted { get; init; } = false;
}

/// <summary>
/// Response for getting FHIR resource history
/// </summary>
public record GetFhirResourceHistoryResponse
{
    /// <summary>
    /// FHIR resource type
    /// </summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>
    /// FHIR resource ID
    /// </summary>
    public string FhirId { get; init; } = string.Empty;

    /// <summary>
    /// Resource composite key (ResourceType/FhirId)
    /// </summary>
    public string CompositeKey { get; init; } = string.Empty;

    /// <summary>
    /// List of resource versions
    /// </summary>
    public List<FhirResourceVersion> Versions { get; init; } = new();

    /// <summary>
    /// Total number of versions
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Whether there are more pages
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Whether there are previous pages
    /// </summary>
    public bool HasPreviousPage { get; init; }
}

/// <summary>
/// FHIR resource version information
/// </summary>
public record FhirResourceVersion
{
    /// <summary>
    /// Version ID
    /// </summary>
    public int VersionId { get; init; }

    /// <summary>
    /// FHIR resource JSON at this version
    /// </summary>
    public string ResourceJson { get; init; } = string.Empty;

    /// <summary>
    /// Resource status at this version
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? LastUpdated { get; init; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; init; }

    /// <summary>
    /// Whether this version is the current version
    /// </summary>
    public bool IsCurrentVersion { get; init; }

    /// <summary>
    /// Whether this version is deleted
    /// </summary>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Deletion timestamp (if deleted)
    /// </summary>
    public DateTimeOffset? DeletedAt { get; init; }

    /// <summary>
    /// User who created this version
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// User who last modified this version
    /// </summary>
    public string LastModifiedBy { get; init; } = string.Empty;

    /// <summary>
    /// User who deleted this version (if deleted)
    /// </summary>
    public string? DeletedBy { get; init; }

    /// <summary>
    /// Operation that created this version (create, update, delete)
    /// </summary>
    public string Operation { get; init; } = string.Empty;
}
