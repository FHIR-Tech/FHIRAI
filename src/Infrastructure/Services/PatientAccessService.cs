using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Entities;
using FHIRAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FHIRAI.Infrastructure.Services;

/// <summary>
/// Service for managing patient access control
/// </summary>
public class PatientAccessService : IPatientAccessService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<PatientAccessService> _logger;

    public PatientAccessService(
        IApplicationDbContext context,
        IUser user,
        ILogger<PatientAccessService> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }

    /// <summary>
    /// Check if user can access patient data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="patientId">Patient ID</param>
    /// <param name="scope">Required scope</param>
    /// <returns>True if access is allowed, false otherwise</returns>
    public async Task<bool> CanAccessPatientAsync(string userId, string patientId, string scope)
    {
        try
        {
            // System administrators can access all patients
            if (_user.IsSystemAdministrator())
            {
                _logger.LogDebug("System administrator {UserId} granted access to patient {PatientId}", userId, patientId);
                return true;
            }

            // Check if user has specific patient access
            var patientAccess = await _context.PatientAccesses
                .Where(pa => pa.UserId.ToString() == userId && 
                           pa.PatientId.ToString() == patientId && 
                           pa.IsActive)
                .FirstOrDefaultAsync();

            if (patientAccess != null)
            {
                _logger.LogDebug("User {UserId} has patient access to {PatientId} with level {AccessLevel}", 
                    userId, patientId, patientAccess.AccessLevel);
                return true;
            }

            // Check if user is the patient themselves
            if (_user.IsPatient() && _user.Id == patientId)
            {
                _logger.LogDebug("Patient {PatientId} accessing their own data", patientId);
                return true;
            }

            // Check for emergency access
            var emergencyAccess = await HasEmergencyAccessAsync(userId, patientId);
            if (emergencyAccess)
            {
                _logger.LogDebug("User {UserId} has emergency access to patient {PatientId}", userId, patientId);
                return true;
            }

            _logger.LogDebug("User {UserId} denied access to patient {PatientId}", userId, patientId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking patient access for user {UserId} and patient {PatientId}", userId, patientId);
            return false;
        }
    }

    /// <summary>
    /// Get all patients that user can access
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of accessible patient IDs</returns>
    public async Task<IEnumerable<string>> GetAccessiblePatientsAsync(string userId)
    {
        try
        {
            // System administrators can access all patients
            if (_user.IsSystemAdministrator())
            {
                var allPatients = await _context.FhirResources
                    .Where(r => r.ResourceType == "Patient" && r.Status == "active")
                    .Select(r => r.FhirId)
                    .ToListAsync();
                
                return allPatients;
            }

            // Get patients user has explicit access to
            var accessiblePatients = await _context.PatientAccesses
                .Where(pa => pa.UserId.ToString() == userId && pa.IsActive)
                .Select(pa => pa.PatientId.ToString())
                .ToListAsync();

            // If user is a patient, add their own ID
            if (_user.IsPatient() && !string.IsNullOrEmpty(_user.Id))
            {
                accessiblePatients.Add(_user.Id);
            }

            return accessiblePatients.Distinct();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessible patients for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Grant access to patient for user
    /// </summary>
    public async Task<PatientAccess> GrantPatientAccessAsync(
        string patientId,
        string userId,
        PatientAccessLevel accessLevel,
        string grantedBy,
        string? reason = null,
        DateTime? expiresAt = null,
        bool isEmergencyAccess = false,
        string? emergencyJustification = null)
    {
        try
        {
            var patientAccess = new PatientAccess
            {
                PatientId = Guid.Parse(patientId),
                UserId = Guid.Parse(userId),
                AccessLevel = accessLevel,
                GrantedBy = grantedBy,
                Reason = reason,
                ExpiresAt = expiresAt,
                IsEmergencyAccess = isEmergencyAccess,
                EmergencyJustification = emergencyJustification,
                IsEnabled = true
            };

            _context.PatientAccesses.Add(patientAccess);
            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("Granted patient access: User {UserId} to Patient {PatientId} with level {AccessLevel} by {GrantedBy}",
                userId, patientId, accessLevel, grantedBy);

            return patientAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting patient access for user {UserId} and patient {PatientId}", userId, patientId);
            throw;
        }
    }

    /// <summary>
    /// Revoke access to patient for user
    /// </summary>
    public async Task<bool> RevokePatientAccessAsync(string patientId, string userId, string revokedBy, string? reason = null)
    {
        try
        {
            var patientAccess = await _context.PatientAccesses
                .Where(pa => pa.PatientId.ToString() == patientId && pa.UserId.ToString() == userId)
                .FirstOrDefaultAsync();

            if (patientAccess != null)
            {
                patientAccess.IsEnabled = false;
                patientAccess.LastModifiedBy = revokedBy;
                patientAccess.LastModifiedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync(CancellationToken.None);

                _logger.LogInformation("Revoked patient access: User {UserId} to Patient {PatientId} by {RevokedBy}",
                    userId, patientId, revokedBy);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking patient access for user {UserId} and patient {PatientId}", userId, patientId);
            return false;
        }
    }

    /// <summary>
    /// Get patient access records for user
    /// </summary>
    public async Task<IEnumerable<PatientAccess>> GetPatientAccessesForUserAsync(string userId, string tenantId)
    {
        try
        {
            return await _context.PatientAccesses
                .Where(pa => pa.UserId.ToString() == userId)
                .Include(pa => pa.Patient)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient accesses for user {UserId}", userId);
            return Enumerable.Empty<PatientAccess>();
        }
    }

    /// <summary>
    /// Get patient access records for patient
    /// </summary>
    public async Task<IEnumerable<PatientAccess>> GetPatientAccessesForPatientAsync(string patientId, string tenantId)
    {
        try
        {
            return await _context.PatientAccesses
                .Where(pa => pa.PatientId.ToString() == patientId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient accesses for patient {PatientId}", patientId);
            return Enumerable.Empty<PatientAccess>();
        }
    }

    /// <summary>
    /// Check if user has emergency access to patient
    /// </summary>
    public async Task<bool> HasEmergencyAccessAsync(string userId, string patientId)
    {
        try
        {
            var emergencyAccess = await _context.PatientAccesses
                .Where(pa => pa.UserId.ToString() == userId && 
                           pa.PatientId.ToString() == patientId && 
                           pa.IsEmergencyAccess && 
                           pa.IsActive)
                .FirstOrDefaultAsync();

            return emergencyAccess != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking emergency access for user {UserId} and patient {PatientId}", userId, patientId);
            return false;
        }
    }

    /// <summary>
    /// Create emergency access to patient
    /// </summary>
    public async Task<PatientAccess> CreateEmergencyAccessAsync(
        string patientId,
        string userId,
        string grantedBy,
        string justification,
        DateTime expiresAt)
    {
        return await GrantPatientAccessAsync(
            patientId,
            userId,
            PatientAccessLevel.Emergency,
            grantedBy,
            "Emergency access",
            expiresAt,
            true,
            justification);
    }

    /// <summary>
    /// Get access level for user to patient
    /// </summary>
    public async Task<PatientAccessLevel?> GetAccessLevelAsync(string userId, string patientId)
    {
        try
        {
            var patientAccess = await _context.PatientAccesses
                .Where(pa => pa.UserId.ToString() == userId && 
                           pa.PatientId.ToString() == patientId && 
                           pa.IsActive)
                .OrderByDescending(pa => pa.AccessLevel)
                .FirstOrDefaultAsync();

            return patientAccess?.AccessLevel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access level for user {UserId} and patient {PatientId}", userId, patientId);
            return null;
        }
    }

    /// <summary>
    /// Check if access is expired
    /// </summary>
    public async Task<bool> IsAccessExpiredAsync(string patientId, string userId)
    {
        try
        {
            var patientAccess = await _context.PatientAccesses
                .Where(pa => pa.UserId.ToString() == userId && pa.PatientId.ToString() == patientId)
                .FirstOrDefaultAsync();

            return patientAccess?.IsExpired ?? true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access expiration for user {UserId} and patient {PatientId}", userId, patientId);
            return true;
        }
    }

    /// <summary>
    /// Extend access expiration
    /// </summary>
    public async Task<bool> ExtendAccessAsync(string patientId, string userId, DateTime newExpiresAt, string modifiedBy)
    {
        try
        {
            var patientAccess = await _context.PatientAccesses
                .Where(pa => pa.UserId.ToString() == userId && pa.PatientId.ToString() == patientId)
                .FirstOrDefaultAsync();

            if (patientAccess != null)
            {
                patientAccess.ExpiresAt = newExpiresAt;
                patientAccess.LastModifiedBy = modifiedBy;
                patientAccess.LastModifiedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync(CancellationToken.None);

                _logger.LogInformation("Extended patient access: User {UserId} to Patient {PatientId} until {ExpiresAt} by {ModifiedBy}",
                    userId, patientId, newExpiresAt, modifiedBy);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending access for user {UserId} and patient {PatientId}", userId, patientId);
            return false;
        }
    }

    /// <summary>
    /// Check if user can grant access to patient
    /// </summary>
    public Task<bool> CanGrantAccessAsync(string userId, string patientId, UserRole userRole)
    {
        // System administrators can always grant access
        if (userRole == UserRole.Administrator)
            return Task.FromResult(true);

        // Healthcare providers can grant access to their patients
        if (userRole == UserRole.HealthcareProvider || userRole == UserRole.Nurse)
        {
            // Check if this is their patient (simplified logic)
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Check if user can revoke access to patient
    /// </summary>
    public async Task<bool> CanRevokeAccessAsync(string userId, string accessId, UserRole userRole)
    {
        // System administrators can always revoke access
        if (userRole == UserRole.Administrator)
            return true;

        // Users can revoke access they granted
        var patientAccess = await _context.PatientAccesses
            .Where(pa => pa.Id.ToString() == accessId)
            .FirstOrDefaultAsync();

        return patientAccess?.GrantedBy == userId;
    }

    /// <summary>
    /// Check if user can view patient access records
    /// </summary>
    public async Task<bool> CanViewAccessRecordsAsync(string userId, string? patientId, UserRole userRole)
    {
        // System administrators can view all records
        if (userRole == UserRole.Administrator)
            return true;

        // Patients can view their own access records
        if (userRole == UserRole.Patient && userId == patientId)
            return true;

        // Healthcare providers can view records for their patients
        if (userRole == UserRole.HealthcareProvider || userRole == UserRole.Nurse)
        {
            if (!string.IsNullOrEmpty(patientId))
            {
                return await CanAccessPatientAsync(userId, patientId, "user/*");
            }
        }

        return false;
    }
}
