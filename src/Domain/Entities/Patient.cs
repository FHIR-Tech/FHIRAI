using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FHIRAI.Domain.Common;
using FHIRAI.Domain.Enums;
using FHIRAI.Domain.Events;

namespace FHIRAI.Domain.Entities;

/// <summary>
/// Patient entity for managing basic patient information and access control
/// FHIR-specific data is stored in FhirResource entity
/// </summary>
public class Patient : BaseAuditableEntity
{
    // ========================================
    // FHIR INTEGRATION FIELDS
    // ========================================
    
    /// <summary>
    /// FHIR Patient ID - unique identifier in FHIR system
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FhirPatientId { get; set; } = string.Empty;

    // ========================================
    // CORE IDENTITY FIELDS
    // ========================================
    
    /// <summary>
    /// First name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Middle name or initial
    /// </summary>
    [MaxLength(255)]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gender (male, female, other, unknown)
    /// </summary>
    [MaxLength(10)]
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Marital status
    /// </summary>
    [MaxLength(50)]
    public string? MaritalStatus { get; set; }

    // ========================================
    // CONTACT INFORMATION FIELDS
    // ========================================
    
    /// <summary>
    /// Email address
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>
    /// Address line 1
    /// </summary>
    [MaxLength(500)]
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Address line 2
    /// </summary>
    [MaxLength(500)]
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// State/Province
    /// </summary>
    [MaxLength(100)]
    public string? State { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }

    // ========================================
    // STATUS & CONFIGURATION FIELDS
    // ========================================
    
    /// <summary>
    /// Patient status
    /// </summary>
    [Required]
    public PatientStatus Status { get; set; } = PatientStatus.Active;

    /// <summary>
    /// Patient type (inpatient, outpatient, emergency, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? PatientType { get; set; }

    // ========================================
    // EMERGENCY CONTACT FIELDS
    // ========================================
    
    /// <summary>
    /// Emergency contact name
    /// </summary>
    [MaxLength(255)]
    public string? EmergencyContactName { get; set; }

    /// <summary>
    /// Emergency contact phone
    /// </summary>
    [MaxLength(50)]
    public string? EmergencyContactPhone { get; set; }

    /// <summary>
    /// Emergency contact email
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? EmergencyContactEmail { get; set; }

    /// <summary>
    /// Relationship to emergency contact
    /// </summary>
    [MaxLength(100)]
    public string? EmergencyContactRelationship { get; set; }

    // ========================================
    // ORGANIZATION & PROVIDER FIELDS
    // ========================================
    
    /// <summary>
    /// Primary care provider ID
    /// </summary>
    [MaxLength(255)]
    public string? PrimaryCareProviderId { get; set; }

    /// <summary>
    /// Managing organization ID
    /// </summary>
    [MaxLength(255)]
    public string? ManagingOrganizationId { get; set; }

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Patient's display name (computed property)
    /// </summary>
    [NotMapped]
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Patient's full name including middle name
    /// </summary>
    [NotMapped]
    public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }.Where(x => !string.IsNullOrEmpty(x)));

    /// <summary>
    /// Patient's age (computed property)
    /// </summary>
    [NotMapped]
    public int? Age => DateOfBirth?.Year > 0 ? DateTime.Now.Year - DateOfBirth.Value.Year : null;

    /// <summary>
    /// Whether patient is a minor
    /// </summary>
    [NotMapped]
    public bool IsMinor => Age.HasValue && Age.Value < 18;

    /// <summary>
    /// Whether patient is elderly (65+)
    /// </summary>
    [NotMapped]
    public bool IsElderly => Age.HasValue && Age.Value >= 65;

    /// <summary>
    /// Whether patient is active
    /// </summary>
    [NotMapped]
    public bool IsActive => Status == PatientStatus.Active;

    /// <summary>
    /// Patient's composite key (FhirPatientId)
    /// </summary>
    [NotMapped]
    public string CompositeKey => FhirPatientId;

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    
    /// <summary>
    /// Navigation property for patient access
    /// </summary>
    public virtual ICollection<PatientAccess> PatientAccesses { get; set; } = new List<PatientAccess>();

    /// <summary>
    /// Navigation property for patient consents
    /// </summary>
    public virtual ICollection<PatientConsent> PatientConsents { get; set; } = new List<PatientConsent>();

    // ========================================
    // DOMAIN EVENTS
    // ========================================
    
    /// <summary>
    /// Add domain event when patient is created
    /// </summary>
    public void MarkAsCreated()
    {
        AddDomainEvent(new PatientCreatedEvent(this));
    }

    /// <summary>
    /// Add domain event when patient is updated
    /// </summary>
    public void MarkAsUpdated()
    {
        AddDomainEvent(new PatientUpdatedEvent(this));
    }

    /// <summary>
    /// Add domain event when patient is deleted
    /// </summary>
    public void MarkAsDeleted()
    {
        AddDomainEvent(new PatientDeletedEvent(this));
    }

    // ========================================
    // VALIDATION METHODS
    // ========================================
    
    /// <summary>
    /// Validate if the patient data is complete
    /// </summary>
    public bool IsDataComplete()
    {
        return !string.IsNullOrEmpty(FirstName) && 
               !string.IsNullOrEmpty(LastName) && 
               !string.IsNullOrEmpty(FhirPatientId) &&
               DateOfBirth.HasValue;
    }

    /// <summary>
    /// Validate if the patient has valid contact information
    /// </summary>
    public bool HasValidContactInfo()
    {
        return !string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(Phone);
    }

    /// <summary>
    /// Validate if the patient has emergency contact information
    /// </summary>
    public bool HasEmergencyContact()
    {
        return !string.IsNullOrEmpty(EmergencyContactName) && 
               !string.IsNullOrEmpty(EmergencyContactPhone);
    }
}
