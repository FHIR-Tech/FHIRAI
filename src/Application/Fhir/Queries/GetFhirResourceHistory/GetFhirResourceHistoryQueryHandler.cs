using MediatR;
using FHIRAI.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FHIRAI.Application.Fhir.Queries.GetFhirResourceHistory;

/// <summary>
/// Handler for GetFhirResourceHistoryQuery
/// </summary>
public class GetFhirResourceHistoryQueryHandler : IRequestHandler<GetFhirResourceHistoryQuery, GetFhirResourceHistoryResponse>
{
    private readonly IFhirResourceRepository _fhirResourceRepository;
    private readonly ILogger<GetFhirResourceHistoryQueryHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fhirResourceRepository">FHIR resource repository</param>
    /// <param name="logger">Logger instance</param>
    public GetFhirResourceHistoryQueryHandler(
        IFhirResourceRepository fhirResourceRepository,
        ILogger<GetFhirResourceHistoryQueryHandler> logger)
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
    public async Task<GetFhirResourceHistoryResponse> Handle(GetFhirResourceHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting FHIR resource history: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);

        try
        {
            // Get all versions of the resource
            var versions = await _fhirResourceRepository.GetHistoryAsync(
                request.ResourceType,
                request.FhirId,
                cancellationToken);

            // Filter deleted versions if not requested
            if (!request.IncludeDeleted)
            {
                versions = versions.Where(v => !v.IsDeleted).ToList();
            }

            // Apply pagination
            var totalCount = versions.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var hasNextPage = request.PageNumber < totalPages;
            var hasPreviousPage = request.PageNumber > 1;

            var pagedVersions = versions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs
            var versionDtos = pagedVersions.Select(v => new FhirResourceVersion
            {
                VersionId = v.VersionId,
                ResourceJson = v.ResourceJson,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                LastUpdated = v.LastUpdated,
                LastModifiedAt = v.LastModifiedAt,
                IsCurrentVersion = v.Status == "active" && !v.IsDeleted,
                IsDeleted = v.IsDeleted,
                DeletedAt = v.DeletedAt,
                CreatedBy = v.CreatedBy ?? "system",
                LastModifiedBy = v.LastModifiedBy ?? "system",
                DeletedBy = v.DeletedBy,
                Operation = DetermineOperation(v)
            }).ToList();

            _logger.LogDebug("Found {Count} versions for FHIR resource: {ResourceType}/{FhirId}", 
                versionDtos.Count, request.ResourceType, request.FhirId);

            return new GetFhirResourceHistoryResponse
            {
                ResourceType = request.ResourceType,
                FhirId = request.FhirId,
                CompositeKey = $"{request.ResourceType}/{request.FhirId}",
                Versions = versionDtos,
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
            _logger.LogError(ex, "Error getting FHIR resource history: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);
            throw;
        }
    }

    private static string DetermineOperation(Domain.Entities.FhirResource resource)
    {
        if (resource.IsDeleted || resource.Status == "deleted")
            return "delete";
        
        if (resource.VersionId == 1)
            return "create";
        
        return "update";
    }
}
