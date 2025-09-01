using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Entities;
using FHIRAI.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FHIRAI.Infrastructure.Data.Repositories;

/// <summary>
/// FHIR resource repository implementation
/// Provides FHIR-specific data access operations with PostgreSQL JSONB support
/// </summary>
public class FhirResourceRepository : IFhirResourceRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FhirResourceRepository> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">Application database context</param>
    /// <param name="logger">Logger instance</param>
    public FhirResourceRepository(IApplicationDbContext context, ILogger<FhirResourceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity or null if not found</returns>
    public async Task<FhirResource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resource by ID: {Id}", id);
        
        return await _context.FhirResources
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    public async Task<IEnumerable<FhirResource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all FHIR resources");
        
        return await _context.FhirResources
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get entities with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of entities</returns>
    public async Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resources page {PageNumber} with size {PageSize}", pageNumber, pageSize);
        
        var query = _context.FhirResources.Where(r => !r.IsDeleted);
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(r => r.LastUpdated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task AddAsync(FhirResource entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding FHIR resource: {ResourceType}/{FhirId}", entity.ResourceType, entity.FhirId);
        
        _context.FhirResources.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task UpdateAsync(FhirResource entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating FHIR resource: {ResourceType}/{FhirId}", entity.ResourceType, entity.FhirId);
        
        _context.FhirResources.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Delete entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteAsync(FhirResource entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting FHIR resource: {ResourceType}/{FhirId}", entity.ResourceType, entity.FhirId);
        
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        await UpdateAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    /// <summary>
    /// Check if entity exists
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FhirResources
            .AnyAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Get count of entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of entities</returns>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FhirResources
            .Where(r => !r.IsDeleted)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Save changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected rows</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Get FHIR resource by type and ID
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FHIR resource or null if not found</returns>
    public async Task<FhirResource?> GetByFhirIdAsync(string resourceType, string fhirId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resource: {ResourceType}/{FhirId}", resourceType, fhirId);
        
        return await _context.FhirResources
            .Where(r => r.ResourceType == resourceType && r.FhirId == fhirId && !r.IsDeleted)
            .OrderByDescending(r => r.VersionId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get FHIR resources by type with pagination
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    public async Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByTypeAsync(string resourceType, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resources by type: {ResourceType}, page {PageNumber}", resourceType, pageNumber);
        
        var query = _context.FhirResources
            .Where(r => r.ResourceType == resourceType && !r.IsDeleted);
            
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(r => r.LastUpdated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <summary>
    /// Search FHIR resources by parameters
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="searchParameters">Search parameters</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    public async Task<(IEnumerable<FhirResource> Items, int TotalCount)> SearchAsync(string resourceType, Dictionary<string, string> searchParameters, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Searching FHIR resources: {ResourceType} with {ParameterCount} parameters", resourceType, searchParameters.Count);
        
        var query = _context.FhirResources
            .Where(r => r.ResourceType == resourceType && !r.IsDeleted);

        // Apply search parameters using JSONB queries
        foreach (var param in searchParameters)
        {
            var jsonPath = $"$.{param.Key}";
            query = query.Where(r => EF.Functions.JsonContains(r.SearchParameters, $"\"{param.Value}\""));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(r => r.LastUpdated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <summary>
    /// Get FHIR resource history
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of FHIR resource versions</returns>
    public async Task<IEnumerable<FhirResource>> GetHistoryAsync(string resourceType, string fhirId, int maxVersions = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resource history: {ResourceType}/{FhirId}", resourceType, fhirId);
        
        return await _context.FhirResources
            .Where(r => r.ResourceType == resourceType && r.FhirId == fhirId && r.VersionId <= maxVersions && !r.IsDeleted)
            .OrderByDescending(r => r.VersionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get FHIR resource by version
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="versionId">Version ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FHIR resource or null if not found</returns>
    public async Task<FhirResource?> GetByVersionAsync(string resourceType, string fhirId, int versionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resource version: {ResourceType}/{FhirId}/v{VersionId}", resourceType, fhirId, versionId);
        
        return await _context.FhirResources
            .FirstOrDefaultAsync(r => r.ResourceType == resourceType && r.FhirId == fhirId && r.VersionId == versionId && !r.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Get FHIR resources by patient reference
    /// </summary>
    /// <param name="patientReference">Patient reference</param>
    /// <param name="resourceType">Optional resource type filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    public async Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByPatientReferenceAsync(string patientReference, string? resourceType = null, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resources by patient reference: {PatientReference}", patientReference);
        
        var query = _context.FhirResources
            .Where(r => r.PatientReference == patientReference && !r.IsDeleted);
            
        if (!string.IsNullOrEmpty(resourceType))
        {
            query = query.Where(r => r.ResourceType == resourceType);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(r => r.LastUpdated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <summary>
    /// Get FHIR resources by organization reference
    /// </summary>
    /// <param name="organizationReference">Organization reference</param>
    /// <param name="resourceType">Optional resource type filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    public async Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByOrganizationReferenceAsync(string organizationReference, string? resourceType = null, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resources by organization reference: {OrganizationReference}", organizationReference);
        
        var query = _context.FhirResources
            .Where(r => r.OrganizationReference == organizationReference && !r.IsDeleted);
            
        if (!string.IsNullOrEmpty(resourceType))
        {
            query = query.Where(r => r.ResourceType == resourceType);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(r => r.LastUpdated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <summary>
    /// Get FHIR resources by practitioner reference
    /// </summary>
    /// <param name="practitionerReference">Practitioner reference</param>
    /// <param name="resourceType">Optional resource type filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of FHIR resources</returns>
    public async Task<(IEnumerable<FhirResource> Items, int TotalCount)> GetByPractitionerReferenceAsync(string practitionerReference, string? resourceType = null, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting FHIR resources by practitioner reference: {PractitionerReference}", practitionerReference);
        
        var query = _context.FhirResources
            .Where(r => r.PractitionerReference == practitionerReference && !r.IsDeleted);
            
        if (!string.IsNullOrEmpty(resourceType))
        {
            query = query.Where(r => r.ResourceType == resourceType);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(r => r.LastUpdated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <summary>
    /// Get count of FHIR resources by type
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of resources</returns>
    public async Task<int> GetCountByTypeAsync(string resourceType, CancellationToken cancellationToken = default)
    {
        return await _context.FhirResources
            .Where(r => r.ResourceType == resourceType && !r.IsDeleted)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Check if FHIR resource exists by type and ID
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    public async Task<bool> ExistsByFhirIdAsync(string resourceType, string fhirId, CancellationToken cancellationToken = default)
    {
        return await _context.FhirResources
            .AnyAsync(r => r.ResourceType == resourceType && r.FhirId == fhirId && !r.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Get FHIR resource by type and ID (alias for GetByFhirIdAsync)
    /// </summary>
    /// <param name="resourceType">FHIR resource type</param>
    /// <param name="fhirId">FHIR resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>FHIR resource or null if not found</returns>
    public async Task<FhirResource?> GetByTypeAndIdAsync(string resourceType, string fhirId, CancellationToken cancellationToken = default)
    {
        return await GetByFhirIdAsync(resourceType, fhirId, cancellationToken);
    }
}
