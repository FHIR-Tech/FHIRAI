using MediatR;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Repositories;
using FHIRAI.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Diagnostics;

namespace FHIRAI.Application.Fhir.Queries.ExportFhirBundle;

/// <summary>
/// Handler for ExportFhirBundleQuery
/// </summary>
public class ExportFhirBundleQueryHandler : IRequestHandler<ExportFhirBundleQuery, ExportFhirBundleResponse>
{
    private readonly IFhirResourceRepository _fhirResourceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ExportFhirBundleQueryHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fhirResourceRepository">FHIR resource repository</param>
    /// <param name="currentUserService">Current user service</param>
    /// <param name="logger">Logger instance</param>
    public ExportFhirBundleQueryHandler(
        IFhirResourceRepository fhirResourceRepository,
        ICurrentUserService currentUserService,
        ILogger<ExportFhirBundleQueryHandler> logger)
    {
        _fhirResourceRepository = fhirResourceRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handle the query
    /// </summary>
    /// <param name="request">Query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response</returns>
    public async Task<ExportFhirBundleResponse> Handle(ExportFhirBundleQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var exportTimestamp = DateTime.UtcNow;

        _logger.LogInformation("Starting FHIR bundle export. ResourceType: {ResourceType}, PageSize: {PageSize}", 
            request.ResourceType, request.PageSize);

        try
        {
            // Build search parameters for repository
            var searchParams = new Dictionary<string, string>();

            // Apply resource type filter
            if (!string.IsNullOrEmpty(request.ResourceType))
            {
                searchParams["resourceType"] = request.ResourceType;
            }

            // Apply status filter - use search parameters instead
            if (request.SearchParameters.ContainsKey("status"))
            {
                searchParams["status"] = request.SearchParameters["status"];
            }

            // Apply reference filters
            if (!string.IsNullOrEmpty(request.PatientReference))
            {
                searchParams["patientReference"] = request.PatientReference;
            }

            if (!string.IsNullOrEmpty(request.OrganizationReference))
            {
                searchParams["organizationReference"] = request.OrganizationReference;
            }

            if (!string.IsNullOrEmpty(request.PractitionerReference))
            {
                searchParams["practitionerReference"] = request.PractitionerReference;
            }

            // Apply time-based filtering
            var (startDate, endDate) = CalculateDateRange(request);
            if (startDate.HasValue)
            {
                searchParams["startDate"] = startDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }

            if (endDate.HasValue)
            {
                searchParams["endDate"] = endDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }

            // Get resources from repository
            var resourceType = request.ResourceType ?? "Patient"; // Default to Patient if not specified
            var (resources, totalCount) = await _fhirResourceRepository.SearchAsync(
                resourceType,
                searchParams,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Apply additional filters in memory
            var filteredResources = ApplyInMemoryFilters(resources, request);

            // Apply max observations per patient if specified
            if (request.MaxObservationsPerPatient.HasValue && request.MaxObservationsPerPatient.Value > 0)
            {
                var groupedResources = filteredResources
                    .GroupBy(r => ExtractPatientIdFromResource(r.ResourceJson))
                    .SelectMany(g => g.Take(request.MaxObservationsPerPatient.Value))
                    .ToList();
                filteredResources = groupedResources;
            }

            // Get history if requested
            var historyResources = new List<FhirResourceDto>();
            if (request.IncludeHistory)
            {
                var resourceIds = filteredResources.Select(r => new { r.ResourceType, r.FhirId }).Distinct();
                
                foreach (var resourceId in resourceIds.Take(100)) // Limit to prevent performance issues
                {
                    var history = await _fhirResourceRepository.GetHistoryAsync(
                        resourceId.ResourceType,
                        resourceId.FhirId,
                        request.PageNumber,
                        request.MaxHistoryVersions,
                        cancellationToken);

                    historyResources.AddRange(history.Items);
                }
            }

            // Build bundle
            var bundle = BuildFhirBundle(filteredResources, historyResources, request, exportTimestamp);

            // Calculate metadata
            var resourceTypeBreakdown = filteredResources
                .GroupBy(r => r.ResourceType)
                .ToDictionary(g => g.Key, g => g.Count());

            stopwatch.Stop();

            var bundleJson = JsonSerializer.Serialize(bundle, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // FHIR R4B Content-Type header should be set in the response
            // Content-Type: application/fhir+json for FHIR JSON responses

            _logger.LogInformation("Completed FHIR bundle export. Total: {Total}, Duration: {Duration}ms", 
                filteredResources.Count + historyResources.Count, stopwatch.ElapsedMilliseconds);

            return new ExportFhirBundleResponse
            {
                BundleJson = bundleJson,
                BundleMetadata = new BundleMetadata
                {
                    BundleType = request.BundleType,
                    Format = "json",
                    BundleId = Guid.NewGuid().ToString(),
                    TotalResources = filteredResources.Count + historyResources.Count,
                    ResourceTypesCount = resourceTypeBreakdown.Count,
                    ResourceTypeBreakdown = resourceTypeBreakdown,
                    ExportTimestamp = exportTimestamp,
                    ExportDurationMs = stopwatch.ElapsedMilliseconds,
                    BundleSizeBytes = System.Text.Encoding.UTF8.GetByteCount(bundleJson)
                },
                ExportStatistics = new ExportStatistics
                {
                    TotalProcessed = totalCount,
                    Exported = filteredResources.Count,
                    HistoryIncluded = historyResources.Count,
                    Skipped = totalCount - filteredResources.Count,
                    ExportDurationMs = stopwatch.ElapsedMilliseconds
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during FHIR bundle export");
            throw;
        }
    }

    /// <summary>
    /// Apply in-memory filters to resources
    /// </summary>
    /// <param name="resources">Resources to filter</param>
    /// <param name="request">Export request</param>
    /// <returns>Filtered resources</returns>
    private static List<FhirResource> ApplyInMemoryFilters(IEnumerable<FhirResource> resources, ExportFhirBundleQuery request)
    {
        var filtered = resources.ToList();

        // Apply specific IDs filter
        if (request.FhirIds != null && request.FhirIds.Any())
        {
            filtered = filtered.Where(r => request.FhirIds.Contains(r.FhirId)).ToList();
        }

        // Apply observation-specific filtering
        if (!string.IsNullOrEmpty(request.ObservationCode) || !string.IsNullOrEmpty(request.ObservationSystem))
        {
            filtered = filtered.Where(r => r.ResourceType == "Observation").ToList();
            
            if (!string.IsNullOrEmpty(request.ObservationCode))
            {
                filtered = filtered.Where(r => r.ResourceJson.Contains($"\"code\":\"{request.ObservationCode}\"")).ToList();
            }
            
            if (!string.IsNullOrEmpty(request.ObservationSystem))
            {
                filtered = filtered.Where(r => r.ResourceJson.Contains($"\"system\":\"{request.ObservationSystem}\"")).ToList();
            }
        }

        // Filter by patient ID
        if (!string.IsNullOrEmpty(request.PatientId))
        {
            filtered = filtered.Where(r => r.ResourceJson.Contains($"\"reference\":\"Patient/{request.PatientId}\"")).ToList();
        }

        // Apply search parameters if provided
        if (request.SearchParameters.Any())
        {
            foreach (var param in request.SearchParameters)
            {
                switch (param.Key.ToLower())
                {
                    case "identifier":
                        filtered = filtered.Where(r => r.SearchParameters != null && r.SearchParameters.Contains(param.Value)).ToList();
                        break;
                    case "name":
                        filtered = filtered.Where(r => r.SearchParameters != null && r.SearchParameters.Contains(param.Value)).ToList();
                        break;
                    case "code":
                        filtered = filtered.Where(r => r.SearchParameters != null && r.SearchParameters.Contains(param.Value)).ToList();
                        break;
                    case "subject":
                        filtered = filtered.Where(r => r.ResourceJson.Contains(param.Value)).ToList();
                        break;
                }
            }
        }

        return filtered;
    }

    /// <summary>
    /// Calculate date range based on time period parameters
    /// </summary>
    /// <param name="request">Export request</param>
    /// <returns>Tuple of start and end dates</returns>
    private static (DateTime? startDate, DateTime? endDate) CalculateDateRange(ExportFhirBundleQuery request)
    {
        var now = DateTime.UtcNow;
        var startDate = request.StartDate;
        var endDate = request.EndDate;

        // If explicit dates are provided, use them
        if (startDate.HasValue || endDate.HasValue)
        {
            return (startDate, endDate);
        }

        // If time period is specified, calculate the range
        if (!string.IsNullOrEmpty(request.TimePeriod) && request.TimePeriodCount.HasValue)
        {
            endDate = now;
            
            startDate = request.TimePeriod.ToLower() switch
            {
                "days" => now.AddDays(-request.TimePeriodCount.Value),
                "weeks" => now.AddDays(-request.TimePeriodCount.Value * 7),
                "months" => now.AddMonths(-request.TimePeriodCount.Value),
                "years" => now.AddYears(-request.TimePeriodCount.Value),
                _ => null
            };
        }

        return (startDate, endDate);
    }

    /// <summary>
    /// Build FHIR R4B bundle from resources
    /// </summary>
    /// <param name="resources">Resources to include</param>
    /// <param name="historyResources">History resources to include</param>
    /// <param name="request">Export request</param>
    /// <param name="timestamp">Bundle timestamp</param>
    /// <returns>FHIR R4B bundle object</returns>
    private static object BuildFhirBundle(IEnumerable<FhirResourceDto> resources, IEnumerable<FhirResourceDto> historyResources, ExportFhirBundleQuery request, DateTime timestamp)
    {
        var allResources = resources.Concat(historyResources).ToList();
        var entries = new List<object>();

        foreach (var resource in allResources)
        {
            var resourceObj = JsonSerializer.Deserialize<JsonElement>(resource.ResourceJson);
            
            // Apply FHIR R4B bundle options
            if (!request.IncludeContained)
            {
                // Remove contained resources if not requested (FHIR R4B standard)
                if (resourceObj.TryGetProperty("contained", out _))
                {
                    // Create new object without contained
                    var resourceWithoutContained = new JsonObject();
                    foreach (var property in resourceObj.EnumerateObject())
                    {
                        if (property.Name != "contained")
                        {
                            resourceWithoutContained.Add(property.Name, property.Value);
                        }
                    }
                    resourceObj = resourceWithoutContained.AsJsonElement();
                }
            }

            if (!request.IncludeExtensions)
            {
                // Remove extensions if not requested (FHIR R4B standard)
                if (resourceObj.TryGetProperty("extension", out _))
                {
                    // Create new object without extensions
                    var resourceWithoutExtensions = new JsonObject();
                    foreach (var property in resourceObj.EnumerateObject())
                    {
                        if (property.Name != "extension")
                        {
                            resourceWithoutExtensions.Add(property.Name, property.Value);
                        }
                    }
                    resourceObj = resourceWithoutExtensions.AsJsonElement();
                }
            }

            if (!request.IncludeMeta)
            {
                // Remove meta if not requested (FHIR R4B standard)
                if (resourceObj.TryGetProperty("meta", out _))
                {
                    // Create new object without meta
                    var resourceWithoutMeta = new JsonObject();
                    foreach (var property in resourceObj.EnumerateObject())
                    {
                        if (property.Name != "meta")
                        {
                            resourceWithoutMeta.Add(property.Name, property.Value);
                        }
                    }
                    resourceObj = resourceWithoutMeta.AsJsonElement();
                }
            }
            
            // FHIR R4B Bundle Entry with proper search mode
            var entry = new
            {
                resource = resourceObj,
                search = new
                {
                    mode = "match" // FHIR R4B standard search mode
                }
            };

            entries.Add(entry);
        }

        // FHIR R4B Bundle structure
        return new
        {
            resourceType = "Bundle",
            type = request.BundleType, // FHIR R4B bundle type
            timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), // FHIR R4B timestamp format
            total = entries.Count,
            entry = entries
        };
    }

    /// <summary>
    /// Extracts the patient ID from a resource JSON.
    /// </summary>
    /// <param name="resourceJson">The JSON string of the resource.</param>
    /// <returns>The patient ID if found, otherwise null.</returns>
    private static string? ExtractPatientIdFromResource(string resourceJson)
    {
        try
        {
            var json = JsonDocument.Parse(resourceJson);
            var root = json.RootElement;

            if (root.TryGetProperty("subject", out var subjectProperty))
            {
                if (subjectProperty.TryGetProperty("reference", out var referenceProperty))
                {
                    var reference = referenceProperty.GetString();
                    if (reference != null && reference.StartsWith("Patient/"))
                    {
                        return reference.Substring(7); // Remove "Patient/"
                    }
                }
            }
        }
        catch (Exception)
        {
            // Log or handle the error appropriately
        }
        return null;
    }
}
