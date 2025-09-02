using MediatR;
using FHIRAI.Application.Common.Security;

namespace FHIRAI.Application.Fhir.Commands.DeleteFhirResource;

/// <summary>
/// Command to delete (soft delete) a FHIR resource
/// </summary>
[FhirScope("user/*", RequiresPatientAccess = true, PatientIdParameter = "FhirId")]
public record DeleteFhirResourceCommand : IRequest<DeleteFhirResourceResponse>
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
    /// Reason for deletion
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Response for deleting FHIR resource
/// </summary>
public record DeleteFhirResourceResponse
{
    /// <summary>
    /// Deleted FHIR resource ID
    /// </summary>
    public string FhirId { get; init; } = string.Empty;

    /// <summary>
    /// Deleted resource composite key (ResourceType/FhirId)
    /// </summary>
    public string CompositeKey { get; init; } = string.Empty;

    /// <summary>
    /// Deletion timestamp
    /// </summary>
    public DateTimeOffset DeletedAt { get; init; }

    /// <summary>
    /// User who performed the deletion
    /// </summary>
    public string DeletedBy { get; init; } = string.Empty;

    /// <summary>
    /// Success indicator
    /// </summary>
    public bool Success { get; init; }
}
