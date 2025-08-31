using MediatR;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Entities;
using FHIRAI.Domain.Repositories;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FHIRAI.Application.Fhir.Commands.CreateFhirResource;

/// <summary>
/// Handler for CreateFhirResourceCommand
/// </summary>
public class CreateFhirResourceCommandHandler : IRequestHandler<CreateFhirResourceCommand, CreateFhirResourceResponse>
{
    private readonly IFhirResourceRepository _fhirResourceRepository;
    private readonly IUser _user;
    private readonly ILogger<CreateFhirResourceCommandHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fhirResourceRepository">FHIR resource repository</param>
    /// <param name="currentUserService">Current user service</param>
    /// <param name="logger">Logger instance</param>
    public CreateFhirResourceCommandHandler(
        IFhirResourceRepository fhirResourceRepository, 
        IUser user,
        ILogger<CreateFhirResourceCommandHandler> logger)
    {
        _fhirResourceRepository = fhirResourceRepository;
        _user = user;
        _logger = logger;
    }

    /// <summary>
    /// Handle the command
    /// </summary>
    /// <param name="request">Command request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response</returns>
    public async Task<CreateFhirResourceResponse> Handle(CreateFhirResourceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating FHIR resource: {ResourceType}", request.ResourceType);

        try
        {
            // Parse FHIR resource
            var parser = new FhirJsonParser();
            var resource = parser.Parse<Resource>(request.ResourceJson);

            // Generate FHIR ID if not provided
            var fhirId = request.FhirId ?? Guid.NewGuid().ToString();

            // Check if resource already exists
            var existingResource = await _fhirResourceRepository.GetByFhirIdAsync(request.ResourceType, fhirId, cancellationToken);
            if (existingResource != null)
            {
                _logger.LogWarning("FHIR resource already exists: {ResourceType}/{FhirId}", request.ResourceType, fhirId);
                throw new InvalidOperationException($"FHIR resource {request.ResourceType}/{fhirId} already exists");
            }

            // Create FHIR resource entity
            var fhirResource = new FhirResource
            {
                ResourceType = request.ResourceType,
                FhirId = fhirId,
                VersionId = 1,
                ResourceJson = request.ResourceJson,
                Status = request.Status,
                LastUpdated = DateTime.UtcNow,
                FhirCreated = DateTime.UtcNow,
                SearchParameters = request.SearchParameters ?? ExtractSearchParameters(resource),
                Tags = request.Tags ?? ExtractTags(resource),
                SecurityLabels = request.SecurityLabels ?? ExtractSecurityLabels(resource),
                PatientReference = ExtractPatientReference(resource),
                OrganizationReference = ExtractOrganizationReference(resource),
                PractitionerReference = ExtractPractitionerReference(resource)
            };

            // Mark as created for domain events
            fhirResource.MarkAsCreated();

            // Add to database
            await _fhirResourceRepository.AddAsync(fhirResource, cancellationToken);

            _logger.LogInformation("Successfully created FHIR resource: {ResourceType}/{FhirId}", request.ResourceType, fhirId);

            return new CreateFhirResourceResponse
            {
                FhirId = fhirId,
                VersionId = fhirResource.VersionId,
                ResourceJson = request.ResourceJson,
                CompositeKey = fhirResource.CompositeKey,
                CreatedAt = fhirResource.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating FHIR resource: {ResourceType}", request.ResourceType);
            throw;
        }
    }

    private static string? ExtractSearchParameters(Resource resource)
    {
        // Extract common search parameters from FHIR resource
        var parameters = new Dictionary<string, object>();

        if (resource is Hl7.Fhir.Model.Patient patient)
        {
            if (patient.Identifier?.Any() == true)
                parameters["identifier"] = patient.Identifier.First().Value;
            if (patient.Name?.Any() == true)
                parameters["name"] = patient.Name.First().Text ?? patient.Name.First().Given?.FirstOrDefault() ?? string.Empty;
        }
        else if (resource is Observation observation)
        {
            if (observation.Code?.Coding?.Any() == true)
                parameters["code"] = observation.Code.Coding.First().Code;
            if (observation.Subject?.Reference != null)
                parameters["subject"] = observation.Subject.Reference;
        }
        else if (resource is Encounter encounter)
        {
            if (encounter.Subject?.Reference != null)
                parameters["subject"] = encounter.Subject.Reference;
            if (encounter.ServiceProvider?.Reference != null)
                parameters["service-provider"] = encounter.ServiceProvider.Reference;
        }
        else if (resource is MedicationRequest medicationRequest)
        {
            if (medicationRequest.Subject?.Reference != null)
                parameters["subject"] = medicationRequest.Subject.Reference;
            if (medicationRequest.Requester?.Reference != null)
                parameters["requester"] = medicationRequest.Requester.Reference;
        }

        return parameters.Any() ? JsonSerializer.Serialize(parameters) : null;
    }

    private static string? ExtractTags(Resource resource)
    {
        if (resource.Meta?.Tag?.Any() != true)
            return null;

        var tags = resource.Meta.Tag.Select(t => new { t.Code, t.Display, t.System }).ToList();
        return JsonSerializer.Serialize(tags);
    }

    private static string? ExtractSecurityLabels(Resource resource)
    {
        if (resource.Meta?.Security?.Any() != true)
            return null;

        var labels = resource.Meta.Security.Select(s => new { s.Code, s.Display, s.System }).ToList();
        return JsonSerializer.Serialize(labels);
    }

    private static string? ExtractPatientReference(Resource resource)
    {
        return resource switch
        {
            Hl7.Fhir.Model.Patient patient => $"Patient/{patient.Id}",
            Observation observation => observation.Subject?.Reference,
            Encounter encounter => encounter.Subject?.Reference,
            Procedure procedure => procedure.Subject?.Reference,
            Condition condition => condition.Subject?.Reference,
            MedicationRequest medicationRequest => medicationRequest.Subject?.Reference,
            DiagnosticReport diagnosticReport => diagnosticReport.Subject?.Reference,
            Immunization immunization => immunization.Patient?.Reference,
            AllergyIntolerance allergyIntolerance => allergyIntolerance.Patient?.Reference,
            _ => null
        };
    }

    private static string? ExtractOrganizationReference(Resource resource)
    {
        return resource switch
        {
            Hl7.Fhir.Model.Patient patient => patient.ManagingOrganization?.Reference,
            Encounter encounter => encounter.ServiceProvider?.Reference,
            _ => null
        };
    }

    private static string? ExtractPractitionerReference(Resource resource)
    {
        return resource switch
        {
            Hl7.Fhir.Model.Patient patient => patient.GeneralPractitioner?.FirstOrDefault()?.Reference,
            Observation observation => observation.Performer?.FirstOrDefault()?.Reference,
            Procedure procedure => procedure.Performer?.FirstOrDefault()?.Actor?.Reference,
            Condition condition => condition.Asserter?.Reference,
            MedicationRequest medicationRequest => medicationRequest.Requester?.Reference,
            DiagnosticReport diagnosticReport => diagnosticReport.Performer?.FirstOrDefault()?.Reference,
            Immunization immunization => immunization.Performer?.FirstOrDefault()?.Actor?.Reference,
            AllergyIntolerance allergyIntolerance => allergyIntolerance.Recorder?.Reference,
            _ => null
        };
    }
}
