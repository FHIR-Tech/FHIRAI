namespace FHIRAI.Application.Common.Security;

/// <summary>
/// Specifies the FHIR scope required to access a resource
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class FhirScopeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FhirScopeAttribute"/> class
    /// </summary>
    /// <param name="scope">Required FHIR scope (e.g., "patient/*", "user/*", "system/*")</param>
    public FhirScopeAttribute(string scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// Gets the required FHIR scope
    /// </summary>
    public string Scope { get; }

    /// <summary>
    /// Gets or sets the resource type for patient-specific scopes
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// Gets or sets the operation type (read, write, delete)
    /// </summary>
    public string? Operation { get; set; }

    /// <summary>
    /// Gets or sets whether this scope requires patient access validation
    /// </summary>
    public bool RequiresPatientAccess { get; set; } = false;

    /// <summary>
    /// Gets or sets the patient ID parameter name for validation
    /// </summary>
    public string? PatientIdParameter { get; set; }
}
