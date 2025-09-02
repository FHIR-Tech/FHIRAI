namespace FHIRAI.Domain.Enums;

/// <summary>
/// Defines the different user roles in the healthcare system
/// Compliant with healthcare regulations and role-based access control
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Guest user with no specific permissions
    /// </summary>
    Guest = 0,

    /// <summary>
    /// System administrator with full system access
    /// </summary>
    Administrator = 1,

    /// <summary>
    /// Healthcare provider (doctor, physician)
    /// </summary>
    HealthcareProvider = 2,

    /// <summary>
    /// Nurse with patient care responsibilities
    /// </summary>
    Nurse = 3,

    /// <summary>
    /// Patient with access to their own data
    /// </summary>
    Patient = 4,

    /// <summary>
    /// Family member with limited patient access
    /// </summary>
    FamilyMember = 5,

    /// <summary>
    /// Researcher with access to anonymized data
    /// </summary>
    Researcher = 6,

    /// <summary>
    /// IT support with technical system access
    /// </summary>
    ITSupport = 7,

    /// <summary>
    /// Read-only user with view permissions
    /// </summary>
    ReadOnlyUser = 8,

    /// <summary>
    /// Data analyst with analytics access
    /// </summary>
    DataAnalyst = 9,

    /// <summary>
    /// IT administrator with infrastructure access
    /// </summary>
    ITAdministrator = 10,

    /// <summary>
    /// Pharmacist with medication management access
    /// </summary>
    Pharmacist = 11,

    /// <summary>
    /// Laboratory technician with lab data access
    /// </summary>
    LabTechnician = 12,

    /// <summary>
    /// Radiologist with imaging data access
    /// </summary>
    Radiologist = 13,

    /// <summary>
    /// Social worker with patient support access
    /// </summary>
    SocialWorker = 14,

    /// <summary>
    /// Mental health professional
    /// </summary>
    MentalHealthProvider = 15,

    /// <summary>
    /// Billing specialist with financial data access
    /// </summary>
    BillingSpecialist = 16,

    /// <summary>
    /// Quality assurance specialist
    /// </summary>
    QualityAssurance = 17,

    /// <summary>
    /// Compliance officer
    /// </summary>
    ComplianceOfficer = 18,

    /// <summary>
    /// Emergency responder with emergency access
    /// </summary>
    EmergencyResponder = 19,

    /// <summary>
    /// Case manager
    /// </summary>
    CaseManager = 20
}
