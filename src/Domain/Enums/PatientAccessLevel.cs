namespace FHIRAI.Domain.Enums;

/// <summary>
/// Defines the different levels of access a user can have to patient data
/// Compliant with healthcare privacy regulations and role-based access control
/// </summary>
public enum PatientAccessLevel
{
    /// <summary>
    /// No access to patient data
    /// </summary>
    None = 0,

    /// <summary>
    /// Read-only access to patient data
    /// </summary>
    Read = 1,

    /// <summary>
    /// Read and write access to patient data
    /// </summary>
    Write = 2,

    /// <summary>
    /// Full access including administrative functions
    /// </summary>
    Admin = 3,

    /// <summary>
    /// Emergency access with full permissions for limited time
    /// </summary>
    Emergency = 4,

    /// <summary>
    /// Access for research purposes with anonymized data
    /// </summary>
    Research = 5,

    /// <summary>
    /// Access for quality improvement and analytics
    /// </summary>
    Analytics = 6,

    /// <summary>
    /// Access for billing and financial purposes
    /// </summary>
    Billing = 7,

    /// <summary>
    /// Access for laboratory and diagnostic services
    /// </summary>
    Laboratory = 8,

    /// <summary>
    /// Access for pharmacy and medication management
    /// </summary>
    Pharmacy = 9,

    /// <summary>
    /// Access for radiology and imaging services
    /// </summary>
    Radiology = 10,

    /// <summary>
    /// Access for nursing and patient care
    /// </summary>
    Nursing = 11,

    /// <summary>
    /// Access for social work and case management
    /// </summary>
    SocialWork = 12,

    /// <summary>
    /// Access for mental health services
    /// </summary>
    MentalHealth = 13,

    /// <summary>
    /// Access for substance abuse treatment
    /// </summary>
    SubstanceAbuse = 14,

    /// <summary>
    /// Access for rehabilitation services
    /// </summary>
    Rehabilitation = 15,

    /// <summary>
    /// Access for palliative and hospice care
    /// </summary>
    PalliativeCare = 16,

    /// <summary>
    /// Access for family and caregiver support
    /// </summary>
    FamilySupport = 17,

    /// <summary>
    /// Access for legal and compliance purposes
    /// </summary>
    Legal = 18,

    /// <summary>
    /// Access for audit and monitoring purposes
    /// </summary>
    Audit = 19,

    /// <summary>
    /// Access for system administration and technical support
    /// </summary>
    SystemAdmin = 20
}
