using MediatR;
using FHIRAI.Application.Common.Security;

namespace FHIRAI.Application.Fhir.Commands.UpdateFhirResource;

/// <summary>
/// Command to update an existing FHIR resource
/// </summary>
[FhirScope("user/*", RequiresPatientAccess = true, PatientIdParameter = "FhirId")]
public record UpdateFhirResourceCommand : IRequest<UpdateFhirResourceResponse>
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
    /// FHIR resource JSON content
    /// </summary>
    public string ResourceJson { get; init; } = string.Empty;

    /// <summary>
    /// Status of the FHIR resource
    /// </summary>
    public string? Status { get; init; }

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
/// Response for updating FHIR resource
/// </summary>
public record UpdateFhirResourceResponse
{
    /// <summary>
    /// Updated FHIR resource ID
    /// </summary>
    public string FhirId { get; init; } = string.Empty;

    /// <summary>
    /// Updated FHIR resource version
    /// </summary>
    public int VersionId { get; init; }

    /// <summary>
    /// Updated FHIR resource JSON
    /// </summary>
    public string ResourceJson { get; init; } = string.Empty;

    /// <summary>
    /// Updated resource composite key (ResourceType/FhirId)
    /// </summary>
    public string CompositeKey { get; init; } = string.Empty;

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; init; }
}
