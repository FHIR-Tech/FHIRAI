using FHIRAI.Domain.Entities;
using FHIRAI.Domain.Enums;

namespace FHIRAI.Application.Common.Interfaces;

/// <summary>
/// Service for managing patient access control
/// </summary>
public interface IPatientAccessService
{
    /// <summary>
    /// Check if user can access patient data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="patientId">Patient ID</param>
    /// <param name="scope">Required scope</param>
    /// <returns>True if access is allowed, false otherwise</returns>
    Task<bool> CanAccessPatientAsync(string userId, string patientId, string scope);

    /// <summary>
    /// Get all patients that user can access
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of accessible patient IDs</returns>
    Task<IEnumerable<string>> GetAccessiblePatientsAsync(string userId);

    /// <summary>
    /// Grant access to patient for user
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="accessLevel">Access level</param>
    /// <param name="grantedBy">User who granted access</param>
    /// <param name="reason">Reason for granting access</param>
    /// <param name="expiresAt">When access expires (null = no expiration)</param>
    /// <param name="isEmergencyAccess">Whether this is emergency access</param>
    /// <param name="emergencyJustification">Emergency justification (required if emergency access)</param>
    /// <returns>Created patient access</returns>
    Task<PatientAccess> GrantPatientAccessAsync(
        string patientId,
        string userId,
        PatientAccessLevel accessLevel,
        string grantedBy,
        string? reason = null,
        DateTime? expiresAt = null,
        bool isEmergencyAccess = false,
        string? emergencyJustification = null);

    /// <summary>
    /// Revoke access to patient for user
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="revokedBy">User who revoked access</param>
    /// <param name="reason">Reason for revocation</param>
    /// <returns>True if revoked, false otherwise</returns>
    Task<bool> RevokePatientAccessAsync(string patientId, string userId, string revokedBy, string? reason = null);

    /// <summary>
    /// Get patient access records for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Collection of patient access records</returns>
    Task<IEnumerable<PatientAccess>> GetPatientAccessesForUserAsync(string userId, string tenantId);

    /// <summary>
    /// Get patient access records for patient
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Collection of patient access records</returns>
    Task<IEnumerable<PatientAccess>> GetPatientAccessesForPatientAsync(string patientId, string tenantId);

    /// <summary>
    /// Check if user has emergency access to patient
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="patientId">Patient ID</param>
    /// <returns>True if emergency access exists, false otherwise</returns>
    Task<bool> HasEmergencyAccessAsync(string userId, string patientId);

    /// <summary>
    /// Create emergency access to patient
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="grantedBy">User who granted emergency access</param>
    /// <param name="justification">Emergency justification</param>
    /// <param name="expiresAt">When emergency access expires</param>
    /// <returns>Created emergency access</returns>
    Task<PatientAccess> CreateEmergencyAccessAsync(
        string patientId,
        string userId,
        string grantedBy,
        string justification,
        DateTime expiresAt);

    /// <summary>
    /// Get access level for user to patient
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="patientId">Patient ID</param>
    /// <returns>Access level or null if no access</returns>
    Task<PatientAccessLevel?> GetAccessLevelAsync(string userId, string patientId);

    /// <summary>
    /// Check if access is expired
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="userId">User ID</param>
    /// <returns>True if access is expired, false otherwise</returns>
    Task<bool> IsAccessExpiredAsync(string patientId, string userId);

    /// <summary>
    /// Extend access expiration
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="newExpiresAt">New expiration time</param>
    /// <param name="modifiedBy">User who modified the access</param>
    /// <returns>True if extended, false otherwise</returns>
    Task<bool> ExtendAccessAsync(string patientId, string userId, DateTime newExpiresAt, string modifiedBy);

    /// <summary>
    /// Check if user can grant access to patient
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="patientId">Patient ID</param>
    /// <param name="userRole">User role</param>
    /// <returns>True if can grant access, false otherwise</returns>
    Task<bool> CanGrantAccessAsync(string userId, string patientId, UserRole userRole);

    /// <summary>
    /// Check if user can revoke access to patient
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="accessId">Access ID</param>
    /// <param name="userRole">User role</param>
    /// <returns>True if can revoke access, false otherwise</returns>
    Task<bool> CanRevokeAccessAsync(string userId, string accessId, UserRole userRole);

    /// <summary>
    /// Check if user can view patient access records
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="patientId">Patient ID</param>
    /// <param name="userRole">User role</param>
    /// <returns>True if can view records, false otherwise</returns>
    Task<bool> CanViewAccessRecordsAsync(string userId, string? patientId, UserRole userRole);
}
