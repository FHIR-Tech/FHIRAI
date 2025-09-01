using FluentValidation;

namespace FHIRAI.Application.Fhir.Queries.ExportFhirBundle;

/// <summary>
/// Validator for ExportFhirBundleQuery
/// </summary>
public class ExportFhirBundleQueryValidator : AbstractValidator<ExportFhirBundleQuery>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ExportFhirBundleQueryValidator()
    {
        RuleFor(x => x.ResourceType)
            .Must(BeValidFhirResourceType)
            .When(x => !string.IsNullOrEmpty(x.ResourceType))
            .WithMessage("Resource type must be a valid FHIR R4B resource type");

        RuleFor(x => x.BundleType)
            .NotEmpty()
            .WithMessage("Bundle type is required")
            .Must(BeValidFhirBundleType)
            .WithMessage("Bundle type must be a valid FHIR R4B bundle type");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 10000)
            .WithMessage("Page size must be between 1 and 10000");

        RuleFor(x => x.MaxHistoryVersions)
            .InclusiveBetween(1, 100)
            .WithMessage("Max history versions must be between 1 and 100");

        RuleFor(x => x.MaxObservationsPerPatient)
            .InclusiveBetween(1, 1000)
            .When(x => x.MaxObservationsPerPatient.HasValue)
            .WithMessage("Max observations per patient must be between 1 and 1000");

        RuleFor(x => x.TimePeriod)
            .Must(BeValidTimePeriod)
            .When(x => !string.IsNullOrEmpty(x.TimePeriod))
            .WithMessage("Time period must be one of: days, weeks, months, years");

        RuleFor(x => x.TimePeriodCount)
            .GreaterThan(0)
            .When(x => x.TimePeriodCount.HasValue)
            .WithMessage("Time period count must be greater than 0");

        RuleFor(x => x.Format)
            .Must(BeValidFormat)
            .WithMessage("Format must be 'json' or 'xml'");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start date must be before end date");
    }

    private static bool BeValidFhirResourceType(string? resourceType)
    {
        if (string.IsNullOrEmpty(resourceType))
            return false;
            
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

    private static bool BeValidFhirBundleType(string bundleType)
    {
        var validBundleTypes = new[]
        {
            "document", "message", "transaction", "transaction-response", "batch", "batch-response", "history", "searchset", "collection"
        };
        
        return validBundleTypes.Contains(bundleType?.ToLower());
    }

    private static bool BeValidTimePeriod(string? timePeriod)
    {
        if (string.IsNullOrEmpty(timePeriod))
            return false;
            
        var validPeriods = new[] { "days", "weeks", "months", "years" };
        return validPeriods.Contains(timePeriod.ToLower());
    }

    private static bool BeValidFormat(string format)
    {
        var validFormats = new[] { "json", "xml" };
        return validFormats.Contains(format?.ToLower());
    }
}
