using MediatR;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Entities;
using FHIRAI.Domain.Repositories;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FHIRAI.Application.Fhir.Commands.ImportFhirBundle;

/// <summary>
/// Handler for ImportFhirBundleCommand
/// </summary>
public class ImportFhirBundleCommandHandler : IRequestHandler<ImportFhirBundleCommand, ImportFhirBundleResponse>
{
    private readonly IFhirResourceRepository _fhirResourceRepository;
    private readonly IUser _user;
    private readonly ISender _sender;
    private readonly ILogger<ImportFhirBundleCommandHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fhirResourceRepository">FHIR resource repository</param>
    /// <param name="user">Current user</param>
    /// <param name="sender">MediatR sender</param>
    /// <param name="logger">Logger instance</param>
    public ImportFhirBundleCommandHandler(
        IFhirResourceRepository fhirResourceRepository,
        IUser user,
        ISender sender,
        ILogger<ImportFhirBundleCommandHandler> logger)
    {
        _fhirResourceRepository = fhirResourceRepository;
        _user = user;
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Handle the command
    /// </summary>
    /// <param name="request">Command request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response</returns>
    public async Task<ImportFhirBundleResponse> Handle(ImportFhirBundleCommand request, CancellationToken cancellationToken)
    {
        var importJobId = Guid.NewGuid().ToString();
        var importedResources = new List<ImportedResource>();
        var errors = new List<ImportError>();
        var totalProcessed = 0;
        var successfullyImported = 0;
        var failedToImport = 0;
        var skipped = 0;
        var updated = 0;

        _logger.LogInformation("Starting FHIR bundle import: {ImportJobId}", importJobId);

        try
        {
            // Parse FHIR Bundle
            var parser = new FhirJsonParser();
            var bundle = parser.Parse<Bundle>(request.BundleJson);

            _logger.LogInformation("Parsed FHIR bundle with {EntryCount} entries", bundle.Entry?.Count ?? 0);

            // Sort entries by dependencies for proper import order
            var sortedEntries = SortEntriesByDependencies(bundle.Entry?.ToList() ?? new List<Bundle.EntryComponent>());
            
            // Get all resource IDs in the bundle for reference checking
            var bundleResourceIds = new HashSet<string>();
            foreach (var entry in sortedEntries)
            {
                if (entry.Resource?.Id != null)
                {
                    var resourceKey = $"{entry.Resource.GetType().Name}/{entry.Resource.Id}";
                    bundleResourceIds.Add(resourceKey);
                }
            }

            // Process each entry in the bundle
            foreach (var entry in sortedEntries)
            {
                totalProcessed++;

                try
                {
                    var resource = entry.Resource;
                    if (resource == null)
                    {
                        errors.Add(new ImportError
                        {
                            ResourceType = "Unknown",
                            OriginalId = null,
                            Message = "Entry contains no resource",
                            ErrorCode = "INVALID_ENTRY",
                            Severity = ErrorSeverity.Warning
                        });
                        failedToImport++;
                        continue;
                    }

                    var resourceType = resource.GetType().Name;
                    var resourceId = resource.Id;

                    // Determine operation type
                    var operation = entry.Request?.Method?.ToString() ?? "POST";

                    // Validate references before processing
                    if (operation?.ToUpper() != "DELETE")
                    {
                        var invalidReferences = ValidateReferences(resource, bundleResourceIds);
                        if (invalidReferences.Any())
                        {
                            errors.Add(new ImportError
                            {
                                ResourceType = resourceType,
                                OriginalId = resourceId,
                                Message = $"Invalid references found: {string.Join(", ", invalidReferences)}",
                                ErrorCode = "INVALID_REFERENCES",
                                Severity = ErrorSeverity.Error
                            });
                            failedToImport++;
                            continue;
                        }
                    }

                    string finalResourceId;
                    ImportStatus status;
                    int versionId = 1;

                    switch (operation?.ToUpper())
                    {
                        case "PUT":
                        case "PATCH":
                            // Check if resource exists
                            var existingResource = await _fhirResourceRepository.GetByFhirIdAsync(
                                resourceType, resourceId, cancellationToken);

                            if (existingResource != null)
                            {
                                if (request.SkipExisting)
                                {
                                    skipped++;
                                    status = ImportStatus.Skipped;
                                    finalResourceId = resourceId;
                                    break;
                                }

                                if (request.UpdateExisting)
                                {
                                    // Update existing resource
                                    existingResource.ResourceJson = SerializeFhirResource(resource, resourceType);
                                    existingResource.Status = "active"; // Default status for update
                                    existingResource.LastUpdated = DateTime.UtcNow;
                                    existingResource.LastModifiedBy = _user.Id?.ToString() ?? "system";
                                    existingResource.LastModifiedAt = DateTimeOffset.UtcNow;
                                    existingResource.VersionId++;

                                    await _fhirResourceRepository.UpdateAsync(existingResource, cancellationToken);
                                    
                                    updated++;
                                    status = ImportStatus.Updated;
                                    finalResourceId = resourceId;
                                    versionId = existingResource.VersionId;
                                }
                                else
                                {
                                    errors.Add(new ImportError
                                    {
                                        ResourceType = resourceType,
                                        OriginalId = resourceId,
                                        Message = "Resource already exists and update not allowed",
                                        ErrorCode = "RESOURCE_EXISTS",
                                        Severity = ErrorSeverity.Warning
                                    });
                                    failedToImport++;
                                    continue;
                                }
                            }
                            else
                            {
                                // Create new resource
                                var newResource = new FhirResource
                                {
                                    ResourceType = resourceType,
                                    FhirId = resourceId,
                                    ResourceJson = SerializeFhirResource(resource, resourceType),
                                    Status = "active",
                                    LastUpdated = DateTime.UtcNow,
                                    FhirCreated = DateTime.UtcNow,
                                    CreatedBy = _user.Id?.ToString() ?? "system",
                                    CreatedAt = DateTimeOffset.UtcNow
                                };

                                newResource.MarkAsCreated();
                                await _fhirResourceRepository.AddAsync(newResource, cancellationToken);
                                
                                successfullyImported++;
                                status = ImportStatus.Success;
                                finalResourceId = resourceId;
                            }
                            break;

                        case "DELETE":
                            // Handle delete operation
                            var resourceToDelete = await _fhirResourceRepository.GetByFhirIdAsync(
                                resourceType, resourceId, cancellationToken);

                            if (resourceToDelete != null)
                            {
                                resourceToDelete.Status = "deleted";
                                resourceToDelete.LastUpdated = DateTime.UtcNow;
                                resourceToDelete.LastModifiedBy = _user.Id?.ToString() ?? "system";
                                resourceToDelete.LastModifiedAt = DateTimeOffset.UtcNow;
                                resourceToDelete.IsDeleted = true;
                                resourceToDelete.DeletedAt = DateTimeOffset.UtcNow;
                                resourceToDelete.DeletedBy = _user.Id?.ToString() ?? "system";

                                resourceToDelete.MarkAsDeleted();
                                await _fhirResourceRepository.UpdateAsync(resourceToDelete, cancellationToken);
                            }

                            finalResourceId = resourceId;
                            status = ImportStatus.Success;
                            break;

                        default:
                            // Create new resource
                            var createResource = new FhirResource
                            {
                                ResourceType = resourceType,
                                FhirId = resourceId,
                                ResourceJson = SerializeFhirResource(resource, resourceType),
                                Status = "active",
                                LastUpdated = DateTime.UtcNow,
                                FhirCreated = DateTime.UtcNow,
                                CreatedBy = _user.Id?.ToString() ?? "system",
                                CreatedAt = DateTimeOffset.UtcNow
                            };

                            createResource.MarkAsCreated();
                            await _fhirResourceRepository.AddAsync(createResource, cancellationToken);
                            
                            successfullyImported++;
                            status = ImportStatus.Success;
                            finalResourceId = resourceId;
                            break;
                    }

                    importedResources.Add(new ImportedResource
                    {
                        ResourceType = resourceType,
                        FhirId = finalResourceId,
                        VersionId = versionId,
                        Status = status,
                        ErrorMessage = null
                    });
                }
                catch (Exception ex)
                {
                    var resourceType = entry.Resource?.GetType().Name ?? "Unknown";
                    var resourceId = entry.Resource?.Id;

                    _logger.LogError(ex, "Error processing resource {ResourceType}/{ResourceId}", resourceType, resourceId);

                    errors.Add(new ImportError
                    {
                        ResourceType = resourceType,
                        OriginalId = resourceId,
                        Message = ex.Message,
                        ErrorCode = ex.GetType().Name,
                        Severity = ErrorSeverity.Error
                    });

                    failedToImport++;
                }
            }

            // Save all changes
            await _fhirResourceRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Completed FHIR bundle import: {ImportJobId}. Processed: {Total}, Success: {Success}, Failed: {Failed}, Skipped: {Skipped}, Updated: {Updated}", 
                importJobId, totalProcessed, successfullyImported, failedToImport, skipped, updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse FHIR bundle: {ImportJobId}", importJobId);

            errors.Add(new ImportError
            {
                ResourceType = "Bundle",
                OriginalId = null,
                Message = $"Failed to parse bundle: {ex.Message}",
                ErrorCode = "BUNDLE_PARSE_ERROR",
                Severity = ErrorSeverity.Fatal
            });

            failedToImport++;
        }

        return new ImportFhirBundleResponse
        {
            TotalProcessed = totalProcessed,
            SuccessfullyImported = successfullyImported,
            FailedToImport = failedToImport,
            Skipped = skipped,
            Updated = updated,
            ImportJobId = importJobId,
            ImportedResources = importedResources,
            Errors = errors,
            ImportedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Sort bundle entries by dependencies to ensure proper import order
    /// </summary>
    /// <param name="entries">Bundle entries</param>
    /// <returns>Sorted entries</returns>
    private List<Bundle.EntryComponent> SortEntriesByDependencies(List<Bundle.EntryComponent> entries)
    {
        if (entries == null || entries.Count <= 1)
            return entries ?? new List<Bundle.EntryComponent>();

        var result = new List<Bundle.EntryComponent>();
        var processed = new HashSet<string>();
        var resourceIdToEntry = new Dictionary<string, Bundle.EntryComponent>();

        // Create mapping of resource ID to entry
        foreach (var entry in entries)
        {
            if (entry.Resource?.Id != null)
            {
                var key = $"{entry.Resource.GetType().Name}/{entry.Resource.Id}";
                resourceIdToEntry[key] = entry;
            }
        }

        // Define resource type priority (lower number = higher priority)
        var typePriority = new Dictionary<string, int>
        {
            // Foundation resources (should be imported first)
            { "Patient", 1 },
            { "Organization", 2 },
            { "Practitioner", 3 },
            { "Location", 4 },
            
            // Clinical resources (depend on foundation resources)
            { "Encounter", 5 },
            { "Condition", 6 },
            { "Observation", 7 },
            { "Procedure", 8 },
            { "MedicationRequest", 9 },
            { "AllergyIntolerance", 10 },
            
            // Document resources (depend on clinical resources)
            { "DocumentReference", 11 },
            { "Composition", 12 },
            
            // Default for other types
            { "default", 100 }
        };

        // Sort by type priority first, then by dependencies
        var sortedEntries = entries.OrderBy(e => 
        {
            var resourceType = e.Resource?.GetType().Name ?? "Unknown";
            return typePriority.GetValueOrDefault(resourceType, typePriority["default"]);
        }).ToList();

        foreach (var entry in sortedEntries)
        {
            if (entry.Resource?.Id == null)
            {
                result.Add(entry);
                continue;
            }

            var resourceKey = $"{entry.Resource.GetType().Name}/{entry.Resource.Id}";
            
            // Skip if already processed
            if (processed.Contains(resourceKey))
                continue;

            // Add this entry and mark as processed
            result.Add(entry);
            processed.Add(resourceKey);
        }

        return result;
    }

    /// <summary>
    /// Validate references in a resource against available resources in the bundle
    /// </summary>
    /// <param name="resource">Resource to validate</param>
    /// <param name="bundleResourceIds">Available resource IDs in the bundle</param>
    /// <returns>List of invalid references</returns>
    private List<string> ValidateReferences(Resource resource, HashSet<string> bundleResourceIds)
    {
        var invalidReferences = new List<string>();
        var references = FhirBundleReferenceHelper.ExtractReferences(resource);
        
        foreach (var reference in references)
        {
            if (reference.Reference != null)
            {
                var referenceType = FhirBundleReferenceHelper.GetResourceTypeFromReference(reference.Reference);
                var referenceId = FhirBundleReferenceHelper.GetResourceIdFromReference(reference.Reference);
                
                if (referenceType != null && referenceId != null)
                {
                    var resourceKey = $"{referenceType}/{referenceId}";
                    
                    // Check if reference exists in bundle
                    if (!bundleResourceIds.Contains(resourceKey))
                    {
                        // Check if it's a reference to an external resource (has # prefix)
                        if (!reference.Reference.StartsWith("#") && !reference.Reference.StartsWith("http"))
                        {
                            invalidReferences.Add(reference.Reference);
                        }
                    }
                }
            }
        }
        
        return invalidReferences;
    }
    
    /// <summary>
    /// Serialize FHIR resource with proper type information
    /// </summary>
    /// <param name="resource">FHIR resource</param>
    /// <param name="resourceType">Resource type name</param>
    /// <returns>Serialized JSON</returns>
    private string SerializeFhirResource(Resource resource, string resourceType)
    {
        // Use FHIR serializer with proper settings
        var serializer = new FhirJsonSerializer();
        var jsonString = serializer.SerializeToString(resource);
        
        return jsonString;
    }
}
