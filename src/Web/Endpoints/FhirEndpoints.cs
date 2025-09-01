using FHIRAI.Application.Fhir.Commands.CreateFhirResource;
using FHIRAI.Application.Fhir.Commands.DeleteFhirResource;
using FHIRAI.Application.Fhir.Commands.UpdateFhirResource;
using FHIRAI.Application.Fhir.Commands.ImportFhirBundle;
using FHIRAI.Application.Fhir.Queries.GetFhirResource;
using FHIRAI.Application.Fhir.Queries.GetFhirResourceHistory;
using FHIRAI.Application.Fhir.Queries.SearchFhirResources;
using FHIRAI.Application.Fhir.Queries.ExportFhirBundle;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FHIRAI.Web.Endpoints;

public class FhirEndpoints : EndpointGroupBase
{
    public override string? GroupName => "fhir";

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(SearchFhirResources, "{resourceType}");
        groupBuilder.MapGet(GetFhirResource, "{resourceType}/{id}");
        groupBuilder.MapGet(GetFhirResourceVersion, "{resourceType}/{id}/_history/{versionId:int}");
        groupBuilder.MapGet(GetFhirResourceHistory, "{resourceType}/{id}/_history");
        groupBuilder.MapPost(CreateFhirResource, "{resourceType}");
        groupBuilder.MapPut(UpdateFhirResource, "{resourceType}/{id}");
        groupBuilder.MapDelete(DeleteFhirResource, "{resourceType}/{id}");

        // System-level operations
        groupBuilder.MapPost(ImportFhirBundle, "$import-bundle");
        groupBuilder.MapGet(ExportFhirBundleGet, "$export-bundle");
        groupBuilder.MapPost(ExportFhirBundlePost, "$export-bundle");
    }

    public async Task<Ok<SearchFhirResourcesResponse>> SearchFhirResources(
        ISender sender,
        HttpRequest request,
        string resourceType,
        int page = 1,
        int _count = 100,
        string? _sort = null,
        string? _sortDir = null,
        string? status = null,
        string? patient = null,
        string? organization = null,
        string? practitioner = null)
    {
        var searchParams = request.Query
            .Where(kvp => kvp.Key is not ("page" or "_count" or "_sort" or "_sortDir" or "status" or "patient" or "organization" or "practitioner"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

        var query = new SearchFhirResourcesQuery
        {
            ResourceType = resourceType,
            PageNumber = page,
            PageSize = _count,
            SortBy = _sort,
            SortDirection = string.IsNullOrWhiteSpace(_sortDir) ? "desc" : _sortDir,
            Status = status,
            PatientReference = patient,
            OrganizationReference = organization,
            PractitionerReference = practitioner,
            SearchParameters = searchParams
        };

        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<GetFhirResourceResponse>, NotFound>> GetFhirResource(
        ISender sender,
        string resourceType,
        string id)
    {
        var query = new GetFhirResourceQuery
        {
            ResourceType = resourceType,
            FhirId = id
        };

        var result = await sender.Send(query);
        if (result is null) return TypedResults.NotFound();
        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<GetFhirResourceResponse>, NotFound>> GetFhirResourceVersion(
        ISender sender,
        string resourceType,
        string id,
        int versionId)
    {
        var query = new GetFhirResourceQuery
        {
            ResourceType = resourceType,
            FhirId = id,
            VersionId = versionId
        };

        var result = await sender.Send(query);
        if (result is null) return TypedResults.NotFound();
        return TypedResults.Ok(result);
    }

    public async Task<Ok<GetFhirResourceHistoryResponse>> GetFhirResourceHistory(
        ISender sender,
        string resourceType,
        string id,
        int page = 1,
        int _count = 100,
        bool includeDeleted = false)
    {
        var query = new GetFhirResourceHistoryQuery
        {
            ResourceType = resourceType,
            FhirId = id,
            PageNumber = page,
            PageSize = _count,
            IncludeDeleted = includeDeleted
        };

        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Created<string>> CreateFhirResource(
        ISender sender,
        HttpRequest request,
        string resourceType)
    {
        using var reader = new StreamReader(request.Body);
        var json = await reader.ReadToEndAsync();

        var command = new CreateFhirResourceCommand
        {
            ResourceType = resourceType,
            ResourceJson = json
        };

        var result = await sender.Send(command);
        var location = $"/fhir/{resourceType}/{result.FhirId}";
        return TypedResults.Created(location, result.FhirId);
    }

    public async Task<Ok<UpdateFhirResourceResponse>> UpdateFhirResource(
        ISender sender,
        HttpRequest request,
        string resourceType,
        string id)
    {
        using var reader = new StreamReader(request.Body);
        var json = await reader.ReadToEndAsync();

        var command = new UpdateFhirResourceCommand
        {
            ResourceType = resourceType,
            FhirId = id,
            ResourceJson = json
        };

        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public async Task<NoContent> DeleteFhirResource(
        ISender sender,
        string resourceType,
        string id,
        string? reason)
    {
        var command = new DeleteFhirResourceCommand
        {
            ResourceType = resourceType,
            FhirId = id,
            Reason = reason
        };

        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public async Task<Ok<ImportFhirBundleResponse>> ImportFhirBundle(
        ISender sender,
        HttpRequest request,
        bool validateResources = true,
        bool skipExisting = false,
        bool updateExisting = true,
        string strategy = "CreateOrUpdate")
    {
        using var reader = new StreamReader(request.Body);
        var bundleJson = await reader.ReadToEndAsync();

        var importStrategy = strategy?.ToLowerInvariant() switch
        {
            "createonly" => ImportStrategy.CreateOnly,
            "updateonly" => ImportStrategy.UpdateOnly,
            "skipexisting" => ImportStrategy.SkipExisting,
            _ => ImportStrategy.CreateOrUpdate
        };

        var command = new ImportFhirBundleCommand
        {
            BundleJson = bundleJson,
            ValidateResources = validateResources,
            SkipExisting = skipExisting,
            UpdateExisting = updateExisting,
            Strategy = importStrategy
        };

        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public async Task<ContentHttpResult> ExportFhirBundleGet(
        ISender sender,
        HttpRequest request,
        string? resourceType = null,
        string? fhirIds = null,
        int page = 1,
        int _count = 1000,
        string bundleType = "collection",
        bool includeHistory = false,
        int maxHistoryVersions = 10,
        bool includeDeleted = false,
        string format = "json",
        string? patient = null,
        string? organization = null,
        string? practitioner = null)
    {
        var additionalParams = request.Query
            .Where(kvp => kvp.Key is not ("resourceType" or "fhirIds" or "page" or "_count" or "bundleType" or "includeHistory" or "maxHistoryVersions" or "includeDeleted" or "format" or "patient" or "organization" or "practitioner"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

        var query = new ExportFhirBundleQuery
        {
            ResourceType = resourceType,
            FhirIds = !string.IsNullOrWhiteSpace(fhirIds) ? fhirIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : null,
            PageNumber = page,
            PageSize = _count,
            BundleType = bundleType,
            IncludeHistory = includeHistory,
            MaxHistoryVersions = maxHistoryVersions,
            IncludeDeleted = includeDeleted,
            Format = format,
            PatientReference = patient,
            OrganizationReference = organization,
            PractitionerReference = practitioner,
            SearchParameters = additionalParams
        };

        var result = await sender.Send(query);
        return TypedResults.Content(result.BundleJson, "application/fhir+json");
    }

    public async Task<ContentHttpResult> ExportFhirBundlePost(
        ISender sender,
        ExportFhirBundleQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Content(result.BundleJson, "application/fhir+json");
    }
}


