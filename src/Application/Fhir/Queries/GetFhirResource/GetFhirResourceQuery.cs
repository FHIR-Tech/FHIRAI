using MediatR;

namespace FHIRAI.Application.Fhir.Queries.GetFhirResource;

/// <summary>
/// Query to get a FHIR resource by type and ID
/// </summary>
public record GetFhirResourceQuery : IRequest<GetFhirResourceResponse?>
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
    /// Specific version ID (optional)
    /// </summary>
    public int? VersionId { get; init; }
}

/// <summary>
/// Response for getting FHIR resource
/// </summary>
public record GetFhirResourceResponse
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
