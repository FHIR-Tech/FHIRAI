using FHIRAI.Domain.Entities;

namespace FHIRAI.Domain.Repositories;

/// <summary>
/// Repository interface for FHIR resources
/// Provides FHIR-specific operations for resource management
/// </summary>
public interface IFhirResourceRepository : IRepository<FhirResource>
{
    /// <summary>
    /// Get FHIR resource by type and ID
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FHIR resource or null if not found</returns>
    Task<FhirResource?> GetByFhirIdAsync(string resourceType, string fhirId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get FHIR resources by type with pagination
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByTypeAsync(string resourceType, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search FHIR resources by parameters
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="searchParameters">Search parameters</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    Task<(IEnumerable<FhirResource> Items, int TotalCount)> SearchAsync(string resourceType, Dictionary<string, string> searchParameters, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get FHIR resource history
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of FHIR resource versions</returns>
    Task<IEnumerable<FhirResource>> GetHistoryAsync(string resourceType, string fhirId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get FHIR resource by version
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="versionId">Version ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FHIR resource or null if not found</returns>
    Task<FhirResource?> GetByVersionAsync(string resourceType, string fhirId, int versionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get FHIR resources by patient reference
    /// </summary>
    /// <param name="patientReference">Patient reference</param>
    /// <param name="resourceType">Optional resource type filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByPatientReferenceAsync(string patientReference, string? resourceType = null, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get FHIR resources by organization reference
    /// </summary>
    /// <param name="organizationReference">Organization reference</param>
    /// <param name="resourceType">Optional resource type filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByOrganizationReferenceAsync(string organizationReference, string? resourceType = null, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get FHIR resources by practitioner reference
    /// </summary>
    /// <param name="practitionerReference">Practitioner reference</param>
    /// <param name="resourceType">Optional resource type filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByPractitionerReferenceAsync(string practitionerReference, string? resourceType = null, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of FHIR resources by type
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of resources</returns>
    Task<int> GetCountByTypeAsync(string resourceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if FHIR resource exists by type and ID
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByFhirIdAsync(string resourceType, string fhirId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get FHIR resource by type and ID (alias for GetByFhirIdAsync)
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FHIR resource or null if not found</returns>
    Task<FhirResource?> GetByTypeAndIdAsync(string resourceType, string fhirId, CancellationToken cancellationToken = default);
}
