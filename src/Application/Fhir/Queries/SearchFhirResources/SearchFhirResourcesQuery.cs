using MediatR;

namespace FHIRAI.Application.Fhir.Queries.SearchFhirResources;

/// <summary>
/// Query to search FHIR resources
/// </summary>
public record SearchFhirResourcesQuery : IRequest<SearchFhirResourcesResponse>
{
    /// <summary>
    /// FHIR resource type (e.g., Patient, Observation, etc.)
    /// </summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>
    /// Search parameters
    /// </summary>
    public Dictionary<string, string> SearchParameters { get; init; } = new();

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; init; } = 100;

    /// <summary>
    /// Sort field
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string SortDirection { get; init; } = "desc";

    /// <summary>
    /// Filter by status
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Filter by patient reference
    /// </summary>
    public string? PatientReference { get; init; }

    /// <summary>
    /// Filter by organization reference
    /// </summary>
    public string? OrganizationReference { get; init; }

    /// <summary>
    /// Filter by practitioner reference
    /// </summary>
    public string? PractitionerReference { get; init; }
}

/// <summary>
/// Response for searching FHIR resources
/// </summary>
public record SearchFhirResourcesResponse
{
    /// <summary>
    /// Collection of FHIR resources
    /// </summary>
    public IEnumerable<FhirResourceDto> Resources { get; init; } = Enumerable.Empty<FhirResourceDto>();

    /// <summary>
    /// Total count of resources
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

    /// <summary>
    /// FHIR resource DTO
    /// </summary>
    public record FhirResourceDto
    {
        /// <summary>
        /// FHIR resource ID
        /// </summary>
        public string FhirId { get; init; } = string.Empty;

        /// <summary>
        /// FHIR resource type
        /// </summary>
        public string ResourceType { get; init; } = string.Empty;

        /// <summary>
        /// FHIR resource version
        /// </summary>
        public int VersionId { get; init; }

        /// <summary>
        /// FHIR resource JSON
        /// </summary>
        public string ResourceJson { get; init; } = string.Empty;

        /// <summary>
        /// Resource status
        /// </summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>
        /// Last updated timestamp
        /// </summary>
        public DateTime? LastUpdated { get; init; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        public DateTimeOffset LastModifiedAt { get; init; }

        /// <summary>
        /// Resource composite key (ResourceType/FhirId)
        /// </summary>
        public string CompositeKey { get; init; } = string.Empty;

        /// <summary>
        /// Patient reference (if applicable)
        /// </summary>
        public string? PatientReference { get; init; }

        /// <summary>
        /// Organization reference (if applicable)
        /// </summary>
        public string? OrganizationReference { get; init; }

        /// <summary>
        /// Practitioner reference (if applicable)
        /// </summary>
        public string? PractitionerReference { get; init; }
    }
}
