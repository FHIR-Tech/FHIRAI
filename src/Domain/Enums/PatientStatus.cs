namespace FHIRAI.Domain.Enums;

/// <summary>
/// Patient status
/// Compliant with healthcare standards and FHIR Patient resource status
/// </summary>
public enum PatientStatus
{
    /// <summary>
    /// Active patient
    /// </summary>
    Active = 1,

    /// <summary>
    /// Inactive patient
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Deceased patient
    /// </summary>
    Deceased = 3,

    /// <summary>
    /// Unknown status
    /// </summary>
    Unknown = 4,

    /// <summary>
    /// Transferred to another facility
    /// </summary>
    Transferred = 5,

    /// <summary>
    /// Discharged
    /// </summary>
    Discharged = 6,

    /// <summary>
    /// Patient is in emergency care
    /// </summary>
    Emergency = 7,

    /// <summary>
    /// Patient is in intensive care
    /// </summary>
    IntensiveCare = 8,

    /// <summary>
    /// Patient is in surgery
    /// </summary>
    InSurgery = 9,

    /// <summary>
    /// Patient is in recovery
    /// </summary>
    Recovery = 10,

    /// <summary>
    /// Patient is in rehabilitation
    /// </summary>
    Rehabilitation = 11,

    /// <summary>
    /// Patient is in palliative care
    /// </summary>
    PalliativeCare = 12,

    /// <summary>
    /// Patient is in hospice care
    /// </summary>
    HospiceCare = 13,

    /// <summary>
    /// Patient is in long-term care
    /// </summary>
    LongTermCare = 14,

    /// <summary>
    /// Patient is in outpatient care
    /// </summary>
    Outpatient = 15,

    /// <summary>
    /// Patient is in home care
    /// </summary>
    HomeCare = 16,

    /// <summary>
    /// Patient is in mental health treatment
    /// </summary>
    MentalHealthTreatment = 17,

    /// <summary>
    /// Patient is in substance abuse treatment
    /// </summary>
    SubstanceAbuseTreatment = 18,

    /// <summary>
    /// Patient is in maternity care
    /// </summary>
    MaternityCare = 19,

    /// <summary>
    /// Patient is in pediatric care
    /// </summary>
    PediatricCare = 20,

    /// <summary>
    /// Patient is in geriatric care
    /// </summary>
    GeriatricCare = 21,

    /// <summary>
    /// Patient is in chronic disease management
    /// </summary>
    ChronicDiseaseManagement = 22,

    /// <summary>
    /// Patient is in preventive care
    /// </summary>
    PreventiveCare = 23,

    /// <summary>
    /// Patient is in end-of-life care
    /// </summary>
    EndOfLifeCare = 24,

    /// <summary>
    /// Patient is in clinical trial
    /// </summary>
    ClinicalTrial = 25,

    /// <summary>
    /// Patient is in research study
    /// </summary>
    ResearchStudy = 26,

    /// <summary>
    /// Patient is in quarantine
    /// </summary>
    Quarantine = 27,

    /// <summary>
    /// Patient is in isolation
    /// </summary>
    Isolation = 28,

    /// <summary>
    /// Patient is in observation
    /// </summary>
    Observation = 29,

    /// <summary>
    /// Patient is in temporary suspension
    /// </summary>
    TemporarilySuspended = 30
}
