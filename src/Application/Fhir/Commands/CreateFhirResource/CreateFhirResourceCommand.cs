using MediatR;
using FHIRAI.Application.Common.Security;

namespace FHIRAI.Application.Fhir.Commands.CreateFhirResource;

/// <summary>
/// Command to create a new FHIR resource
/// </summary>
[FhirScope("user/*", RequiresPatientAccess = true)]
public record CreateFhirResourceCommand : IRequest<CreateFhirResourceResponse>
{
    /// <summary>
    /// FHIR resource type (e.g., Patient, Observation, etc.)
    /// </summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>
    /// FHIR resource JSON content
    /// </summary>
    public string ResourceJson { get; init; } = string.Empty;

    /// <summary>
    /// FHIR resource ID (optional, will be generated if not provided)
    /// </summary>
    public string? FhirId { get; init; }

    /// <summary>
    /// Status of the FHIR resource (default: active)
    /// </summary>
    public string Status { get; init; } = "active";

    /// <summary>
    /// Search parameters for efficient querying
    /// </summary>
    public string? SearchParameters { get; init; }

    /// <summary>
    /// Security labels for access control
    /// </summary>
    public string? SecurityLabels { get; init; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public string? Tags { get; init; }
}

/// <summary>
/// Response for creating FHIR resource
/// </summary>
public record CreateFhirResourceResponse
{
    /// <summary>
    /// Created FHIR resource ID
    /// </summary>
    public string FhirId { get; init; } = string.Empty;

    /// <summary>
    /// FHIR resource version
    /// </summary>
    public int VersionId { get; init; }

    /// <summary>
    /// Created FHIR resource JSON
    /// </summary>
    public string ResourceJson { get; init; } = string.Empty;

    /// <summary>
    /// Created resource composite key (ResourceType/FhirId)
    /// </summary>
    public string CompositeKey { get; init; } = string.Empty;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}
