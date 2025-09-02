using FHIRAI.Domain.Enums;

namespace FHIRAI.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }
    
    /// <summary>
    /// Current user display name
    /// </summary>
    string? UserDisplayName { get; }

    /// <summary>
    /// Current tenant ID
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// User's IP address
    /// </summary>
    string? UserIpAddress { get; }

    /// <summary>
    /// User's FHIR scopes
    /// </summary>
    IEnumerable<string> Scopes { get; }

    /// <summary>
    /// Check if user has specific scope
    /// </summary>
    /// <param name="scope">Scope to check</param>
    /// <returns>True if user has scope, false otherwise</returns>
    bool HasScope(string scope);

    /// <summary>
    /// Check if user has any of the specified scopes
    /// </summary>
    /// <param name="scopes">Scopes to check</param>
    /// <returns>True if user has any scope, false otherwise</returns>
    bool HasAnyScope(params string[] scopes);

    /// <summary>
    /// Current user role
    /// </summary>
    UserRole UserRole { get; }

    /// <summary>
    /// Current user practitioner ID
    /// </summary>
    string? PractitionerId { get; }

    /// <summary>
    /// Check if current user is system administrator
    /// </summary>
    /// <returns>True if system administrator, false otherwise</returns>
    bool IsSystemAdministrator();

    /// <summary>
    /// Check if current user is healthcare provider
    /// </summary>
    /// <returns>True if healthcare provider, false otherwise</returns>
    bool IsHealthcareProvider();

    /// <summary>
    /// Check if current user is patient
    /// </summary>
    /// <returns>True if patient, false otherwise</returns>
    bool IsPatient();

    /// <summary>
    /// Check if current user can access specific patient
    /// </summary>
    /// <param name="patientId">Patient ID to check</param>
    /// <returns>True if can access, false otherwise</returns>
    Task<bool> CanAccessPatientAsync(string patientId);
}
