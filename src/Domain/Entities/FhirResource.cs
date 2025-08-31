using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FHIRAI.Domain.Common;
using FHIRAI.Domain.Events;

namespace FHIRAI.Domain.Entities;

/// <summary>
/// Entity for storing FHIR resources in PostgreSQL JSONB format with full audit trail
/// Compliant with FHIR R4 specification and healthcare data standards
/// </summary>
public class FhirResource : BaseAuditableEntity
{
    // ========================================
    // CORE IDENTITY FIELDS
    // ========================================
    
    /// <summary>
    /// FHIR resource type (e.g., Patient, Observation, Medication, etc.)
    /// Must be a valid FHIR R4 resource type
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// FHIR resource ID - unique identifier within the resource type
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FhirId { get; set; } = string.Empty;

    /// <summary>
    /// FHIR resource version for versioning support
    /// </summary>
    public int VersionId { get; set; } = 1;

    // ========================================
    // FHIR DATA FIELDS
    // ========================================
    
    /// <summary>
    /// FHIR resource as JSONB for efficient querying and FHIR compliance
    /// Stores the complete FHIR resource in JSON format
    /// </summary>
    [Required]
    [Column(TypeName = "jsonb")]
    public string ResourceJson { get; set; } = string.Empty;

    // ========================================
    // STATUS & TIMING FIELDS
    // ========================================
    
    /// <summary>
    /// Status of the FHIR resource (active, inactive, entered-in-error, etc.)
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// Last updated timestamp from FHIR resource
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    /// <summary>
    /// When the resource was created in FHIR
    /// </summary>
    public DateTime? FhirCreated { get; set; }

    // ========================================
    // SEARCH & SECURITY FIELDS
    // ========================================
    
    /// <summary>
    /// Search parameters for efficient FHIR querying
    /// Stored as JSONB for flexible search capabilities
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? SearchParameters { get; set; }

    /// <summary>
    /// Security labels for access control and privacy
    /// Compliant with FHIR security labeling
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? SecurityLabels { get; set; }

    /// <summary>
    /// Meta tags for categorization and workflow
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? Tags { get; set; }

    // ========================================
    // REFERENCE & RELATIONSHIP FIELDS
    // ========================================
    
    /// <summary>
    /// Patient reference for patient-specific resources
    /// </summary>
    [MaxLength(255)]
    public string? PatientReference { get; set; }

    /// <summary>
    /// Organization reference for organization-specific resources
    /// </summary>
    [MaxLength(255)]
    public string? OrganizationReference { get; set; }

    /// <summary>
    /// Practitioner reference for practitioner-specific resources
    /// </summary>
    [MaxLength(255)]
    public string? PractitionerReference { get; set; }

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Composite key for efficient querying
    /// </summary>
    [NotMapped]
    public string CompositeKey => $"{ResourceType}/{FhirId}";

    /// <summary>
    /// Whether the resource is active
    /// </summary>
    [NotMapped]
    public bool IsActive => Status?.ToLower() == "active";

    /// <summary>
    /// Whether the resource is in error state
    /// </summary>
    [NotMapped]
    public bool IsInError => Status?.ToLower() == "entered-in-error";

    /// <summary>
    /// Whether the resource has been updated recently (within 24 hours)
    /// </summary>
    [NotMapped]
    public bool IsRecentlyUpdated => LastUpdated.HasValue && 
                                   LastUpdated.Value > DateTime.UtcNow.AddHours(-24);

    // ========================================
    // DOMAIN EVENTS
    // ========================================
    
    /// <summary>
    /// Add domain event when FHIR resource is created
    /// </summary>
    public void MarkAsCreated()
    {
        AddDomainEvent(new FhirResourceCreatedEvent(this));
    }

    /// <summary>
    /// Add domain event when FHIR resource is updated
    /// </summary>
    public void MarkAsUpdated()
    {
        AddDomainEvent(new FhirResourceUpdatedEvent(this));
    }

    /// <summary>
    /// Add domain event when FHIR resource is deleted
    /// </summary>
    public void MarkAsDeleted()
    {
        AddDomainEvent(new FhirResourceDeletedEvent(this));
    }

    // ========================================
    // VALIDATION METHODS
    // ========================================
    
    /// <summary>
    /// Validate if the resource type is a valid FHIR R4 resource type
    /// </summary>
    public bool IsValidResourceType()
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
        
        return validTypes.Contains(ResourceType);
    }

    /// <summary>
    /// Validate if the FHIR ID follows proper format
    /// </summary>
    public bool IsValidFhirId()
    {
        return !string.IsNullOrWhiteSpace(FhirId) && 
               FhirId.Length <= 64 && 
               System.Text.RegularExpressions.Regex.IsMatch(FhirId, @"^[A-Za-z0-9\-\.]{1,64}$");
    }
}
