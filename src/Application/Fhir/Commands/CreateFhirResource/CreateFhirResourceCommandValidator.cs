using FluentValidation;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Model;

namespace FHIRAI.Application.Fhir.Commands.CreateFhirResource;

/// <summary>
/// Validator for CreateFhirResourceCommand
/// </summary>
public class CreateFhirResourceCommandValidator : AbstractValidator<CreateFhirResourceCommand>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CreateFhirResourceCommandValidator()
    {
        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .WithMessage("Resource type is required")
            .MaximumLength(100)
            .WithMessage("Resource type cannot exceed 100 characters")
            .Must(BeValidResourceType)
            .WithMessage("Resource type must be a valid FHIR R4 resource type");

        RuleFor(x => x.ResourceJson)
            .NotEmpty()
            .WithMessage("Resource JSON is required")
            .Must(BeValidFhirJson)
            .WithMessage("Resource JSON must be valid FHIR JSON format");

        RuleFor(x => x.FhirId)
            .MaximumLength(255)
            .WithMessage("FHIR ID cannot exceed 255 characters")
            .Matches(@"^[A-Za-z0-9\-\.]{1,64}$")
            .WithMessage("FHIR ID must contain only letters, numbers, hyphens, and dots")
            .When(x => !string.IsNullOrEmpty(x.FhirId));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .WithMessage("Status cannot exceed 50 characters")
            .Must(BeValidStatus)
            .WithMessage("Status must be a valid FHIR resource status");

        RuleFor(x => x.SearchParameters)
            .Must(BeValidJson)
            .WithMessage("Search parameters must be valid JSON format")
            .When(x => !string.IsNullOrEmpty(x.SearchParameters));

        RuleFor(x => x.SecurityLabels)
            .Must(BeValidJson)
            .WithMessage("Security labels must be valid JSON format")
            .When(x => !string.IsNullOrEmpty(x.SecurityLabels));

        RuleFor(x => x.Tags)
            .Must(BeValidJson)
            .WithMessage("Tags must be valid JSON format")
            .When(x => !string.IsNullOrEmpty(x.Tags));
    }

    private static bool BeValidResourceType(string resourceType)
    {
        var validTypes = new[]
        {
            "Patient", "Observation", "Medication", "MedicationRequest", "Condition",
            "Encounter", "Procedure", "DiagnosticReport", "ImagingStudy", "AllergyIntolerance",
            "Immunization", "CarePlan", "Goal", "Questionnaire", "QuestionnaireResponse",
            "DocumentReference", "Composition", "Practitioner", "Organization", "Location",
            "Device", "Substance", "MedicationAdministration", "MedicationDispense",
            "MedicationStatement", "Coverage", "Claim", "ExplanationOfBenefit", "Invoice",
            "PaymentNotice", "PaymentReconciliation", "Account", "ChargeItem", "Contract",
            "Group", "HealthcareService", "InsurancePlan", "Network", "PractitionerRole",
            "ResearchStudy", "ResearchSubject", "Schedule", "Slot", "VerificationResult"
        };
        
        return validTypes.Contains(resourceType);
    }

    private static bool BeValidFhirJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            // Try to parse as FHIR JSON
            var parser = new FhirJsonParser();
            var resource = parser.Parse<Resource>(json);
            
            // Additional validation: check if resource has required fields
            if (resource.Meta == null)
                return false;
                
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidStatus(string status)
    {
        var validStatuses = new[]
        {
            "active", "inactive", "entered-in-error", "draft", "preliminary", "final",
            "amended", "corrected", "cancelled", "entered-in-error", "unknown"
        };
        
        return validStatuses.Contains(status?.ToLower());
    }

    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
