using MediatR;
using FHIRAI.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FHIRAI.Application.Fhir.Queries.GetFhirResource;

/// <summary>
/// Handler for GetFhirResourceQuery
/// </summary>
public class GetFhirResourceQueryHandler : IRequestHandler<GetFhirResourceQuery, GetFhirResourceResponse?>
{
    private readonly IFhirResourceRepository _fhirResourceRepository;
    private readonly ILogger<GetFhirResourceQueryHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fhirResourceRepository">FHIR resource repository</param>
    /// <param name="logger">Logger instance</param>
    public GetFhirResourceQueryHandler(IFhirResourceRepository fhirResourceRepository, ILogger<GetFhirResourceQueryHandler> logger)
    {
        _fhirResourceRepository = fhirResourceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handle the query
    /// </summary>
    /// <param name="request">Query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response or null if not found</returns>
    public async Task<GetFhirResourceResponse?> Handle(GetFhirResourceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting FHIR resource: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);

        try
        {
            var resource = request.VersionId.HasValue
                ? await _fhirResourceRepository.GetByVersionAsync(request.ResourceType, request.FhirId, request.VersionId.Value, cancellationToken)
                : await _fhirResourceRepository.GetByFhirIdAsync(request.ResourceType, request.FhirId, cancellationToken);

            if (resource == null)
            {
                _logger.LogDebug("FHIR resource not found: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);
                return null;
            }

            _logger.LogDebug("Successfully retrieved FHIR resource: {ResourceType}/{FhirId}, version {VersionId}", 
                request.ResourceType, request.FhirId, resource.VersionId);

            return new GetFhirResourceResponse
            {
                FhirId = resource.FhirId,
                ResourceType = resource.ResourceType,
                VersionId = resource.VersionId,
                ResourceJson = resource.ResourceJson,
                Status = resource.Status,
                LastUpdated = resource.LastUpdated,
                CreatedAt = resource.CreatedAt,
                LastModifiedAt = resource.LastModifiedAt,
                CompositeKey = resource.CompositeKey,
                PatientReference = resource.PatientReference,
                OrganizationReference = resource.OrganizationReference,
                PractitionerReference = resource.PractitionerReference
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FHIR resource: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);
            throw;
        }
    }
}
