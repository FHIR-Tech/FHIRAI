using MediatR;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Entities;
using FHIRAI.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FHIRAI.Application.Fhir.Commands.DeleteFhirResource;

/// <summary>
/// Handler for DeleteFhirResourceCommand
/// </summary>
public class DeleteFhirResourceCommandHandler : IRequestHandler<DeleteFhirResourceCommand, DeleteFhirResourceResponse>
{
    private readonly IFhirResourceRepository _fhirResourceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteFhirResourceCommandHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fhirResourceRepository">FHIR resource repository</param>
    /// <param name="currentUserService">Current user service</param>
    /// <param name="logger">Logger instance</param>
    public DeleteFhirResourceCommandHandler(
        IFhirResourceRepository fhirResourceRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteFhirResourceCommandHandler> logger)
    {
        _fhirResourceRepository = fhirResourceRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handle the command
    /// </summary>
    /// <param name="request">Command request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response</returns>
    public async Task<DeleteFhirResourceResponse> Handle(DeleteFhirResourceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting FHIR resource: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);

        try
        {
            // Get existing resource
            var existingResource = await _fhirResourceRepository.GetByFhirIdAsync(
                request.ResourceType, 
                request.FhirId, 
                cancellationToken);

            if (existingResource == null)
            {
                _logger.LogWarning("FHIR resource not found: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);
                throw new InvalidOperationException($"Resource {request.ResourceType}/{request.FhirId} not found");
            }

            // Check if already deleted
            if (existingResource.IsDeleted)
            {
                _logger.LogWarning("FHIR resource already deleted: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);
                throw new InvalidOperationException($"Resource {request.ResourceType}/{request.FhirId} is already deleted");
            }

            // Soft delete - mark as deleted
            existingResource.Status = "deleted";
            existingResource.LastUpdated = DateTime.UtcNow;
            existingResource.LastModifiedBy = _currentUserService.UserId ?? "system";
            existingResource.LastModifiedAt = DateTimeOffset.UtcNow;
            
            existingResource.IsDeleted = true;
            existingResource.DeletedAt = DateTimeOffset.UtcNow;
            existingResource.DeletedBy = _currentUserService.UserId ?? "system";

            // Mark as deleted for domain events
            existingResource.MarkAsDeleted();

            // Save changes
            await _fhirResourceRepository.UpdateAsync(existingResource, cancellationToken);

            _logger.LogInformation("Successfully deleted FHIR resource: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);

            return new DeleteFhirResourceResponse
            {
                FhirId = request.FhirId,
                CompositeKey = existingResource.CompositeKey,
                DeletedAt = existingResource.DeletedAt.Value,
                DeletedBy = existingResource.DeletedBy ?? "system",
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting FHIR resource: {ResourceType}/{FhirId}", request.ResourceType, request.FhirId);
            throw;
        }
    }
}
