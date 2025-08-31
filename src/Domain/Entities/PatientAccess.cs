using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FHIRAI.Domain.Common;
using FHIRAI.Domain.Enums;

namespace FHIRAI.Domain.Entities;

/// <summary>
/// Patient access control entity for managing access to patient data
/// </summary>
public class PatientAccess : BaseAuditableEntity
{
    // ========================================
    // FOREIGN KEY FIELDS
    // ========================================
    
    /// <summary>
    /// Patient ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// User ID who has access
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    // ========================================
    // CORE ACCESS FIELDS
    // ========================================
    
    /// <summary>
    /// Level of access granted
    /// </summary>
    [Required]
    public PatientAccessLevel AccessLevel { get; set; }

    // ========================================
    // TIMING FIELDS
    // ========================================
    
    /// <summary>
    /// When access was granted
    /// </summary>
    [Required]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When access expires (null = no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    // ========================================
    // AUTHORIZATION FIELDS
    // ========================================
    
    /// <summary>
    /// User who granted the access
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string GrantedBy { get; set; } = string.Empty;

    /// <summary>
    /// Reason for granting access
    /// </summary>
    public string? Reason { get; set; }

    // ========================================
    // EMERGENCY ACCESS FIELDS
    // ========================================
    
    /// <summary>
    /// Whether this is an emergency access
    /// </summary>
    public bool IsEmergencyAccess { get; set; }

    /// <summary>
    /// Emergency access justification (required if IsEmergencyAccess = true)
    /// </summary>
    public string? EmergencyJustification { get; set; }

    // ========================================
    // STATUS FIELDS
    // ========================================
    
    /// <summary>
    /// Whether access is manually enabled/disabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Whether access is currently active (computed based on IsEnabled and ExpiresAt)
    /// </summary>
    [NotMapped]
    public bool IsActive => IsEnabled && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);

    /// <summary>
    /// Whether access has expired
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    
    /// <summary>
    /// Navigation property for patient
    /// </summary>
    public virtual Patient Patient { get; set; } = null!;
}
