namespace FHIRAI.Domain.Enums;

/// <summary>
/// Types of patient consent
/// Compliant with healthcare privacy regulations and FHIR consent resources
/// </summary>
public enum ConsentType
{
    /// <summary>
    /// Consent for data sharing with healthcare providers
    /// </summary>
    DataSharing = 1,

    /// <summary>
    /// Consent for research participation
    /// </summary>
    ResearchParticipation = 2,

    /// <summary>
    /// Consent for emergency access
    /// </summary>
    EmergencyAccess = 3,

    /// <summary>
    /// Consent for family member access
    /// </summary>
    FamilyAccess = 4,

    /// <summary>
    /// Consent for third-party access
    /// </summary>
    ThirdPartyAccess = 5,

    /// <summary>
    /// Consent for marketing communications
    /// </summary>
    MarketingCommunications = 6,

    /// <summary>
    /// Consent for automated decision making
    /// </summary>
    AutomatedDecisionMaking = 7,

    /// <summary>
    /// Consent for data portability
    /// </summary>
    DataPortability = 8,

    /// <summary>
    /// Consent for data retention
    /// </summary>
    DataRetention = 9,

    /// <summary>
    /// General treatment consent
    /// </summary>
    TreatmentConsent = 10,

    /// <summary>
    /// Consent for telemedicine services
    /// </summary>
    TelemedicineConsent = 11,

    /// <summary>
    /// Consent for genetic testing
    /// </summary>
    GeneticTestingConsent = 12,

    /// <summary>
    /// Consent for organ donation
    /// </summary>
    OrganDonationConsent = 13,

    /// <summary>
    /// Consent for clinical trials
    /// </summary>
    ClinicalTrialConsent = 14,

    /// <summary>
    /// Consent for mental health treatment
    /// </summary>
    MentalHealthConsent = 15,

    /// <summary>
    /// Consent for substance abuse treatment
    /// </summary>
    SubstanceAbuseConsent = 16,

    /// <summary>
    /// Consent for reproductive health services
    /// </summary>
    ReproductiveHealthConsent = 17,

    /// <summary>
    /// Consent for end-of-life care
    /// </summary>
    EndOfLifeConsent = 18,

    /// <summary>
    /// Consent for palliative care
    /// </summary>
    PalliativeCareConsent = 19,

    /// <summary>
    /// Consent for hospice care
    /// </summary>
    HospiceCareConsent = 20,

    /// <summary>
    /// Consent for advanced directives
    /// </summary>
    AdvancedDirectiveConsent = 21
}
