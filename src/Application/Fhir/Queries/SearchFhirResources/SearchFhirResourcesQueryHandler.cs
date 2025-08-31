using MediatR;
using FHIRAI.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FHIRAI.Application.Fhir.Queries.SearchFhirResources;

/// <summary>
/// Handler for SearchFhirResourcesQuery
/// </summary>
public class SearchFhirResourcesQueryHandler : IRequestHandler<SearchFhirResourcesQuery, SearchFhirResourcesResponse>
{
    private readonly IFhirResourceRepository _fhirResourceRepository;
    private readonly ILogger<SearchFhirResourcesQueryHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fhirResourceRepository">FHIR resource repository</param>
    /// <param name="logger">Logger instance</param>
    public SearchFhirResourcesQueryHandler(IFhirResourceRepository fhirResourceRepository, ILogger<SearchFhirResourcesQueryHandler> logger)
    {
        _fhirResourceRepository = fhirResourceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handle the query
    /// </summary>
    /// <param name="request">Query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response</returns>
    public async Task<SearchFhirResourcesResponse> Handle(SearchFhirResourcesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Searching FHIR resources: {ResourceType} with {ParameterCount} parameters, page {PageNumber}", 
            request.ResourceType, request.SearchParameters.Count, request.PageNumber);

        try
        {
            // Use repository search method
            var (resources, totalCount) = await _fhirResourceRepository.SearchAsync(
                request.ResourceType,
                request.SearchParameters,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var hasNextPage = request.PageNumber < totalPages;
            var hasPreviousPage = request.PageNumber > 1;

            // Map to DTOs
            var resourceDtos = resources.Select(r => new SearchFhirResourcesResponse.FhirResourceDto
            {
                FhirId = r.FhirId,
                ResourceType = r.ResourceType,
                VersionId = r.VersionId,
                ResourceJson = r.ResourceJson,
                Status = r.Status,
                LastUpdated = r.LastUpdated,
                CreatedAt = r.CreatedAt,
                LastModifiedAt = r.LastModifiedAt,
                CompositeKey = r.CompositeKey,
                PatientReference = r.PatientReference,
                OrganizationReference = r.OrganizationReference,
                PractitionerReference = r.PractitionerReference
            }).ToList();

            _logger.LogDebug("Found {Count} FHIR resources for {ResourceType}", resourceDtos.Count, request.ResourceType);

            return new SearchFhirResourcesResponse
            {
                Resources = resourceDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching FHIR resources: {ResourceType}", request.ResourceType);
            throw;
        }
    }
}
