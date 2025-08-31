using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FHIRAI.Domain.Common;
using FHIRAI.Domain.Enums;

namespace FHIRAI.Domain.Entities;

/// <summary>
/// Patient consent entity for managing patient consent and authorization
/// </summary>
public class PatientConsent : BaseAuditableEntity
{
    // ========================================
    // FOREIGN KEY FIELDS
    // ========================================
    
    /// <summary>
    /// Patient ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    // ========================================
    // CORE CONSENT FIELDS
    // ========================================
    
    /// <summary>
    /// Type of consent
    /// </summary>
    [Required]
    public ConsentType ConsentType { get; set; }

    /// <summary>
    /// Whether consent is currently active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    // ========================================
    // TIMING FIELDS
    // ========================================
    
    /// <summary>
    /// When consent was granted
    /// </summary>
    [Required]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When consent expires (null = no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    // ========================================
    // AUTHORIZATION FIELDS
    // ========================================
    
    /// <summary>
    /// User who granted the consent
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string GrantedBy { get; set; } = string.Empty;

    // ========================================
    // REVOCATION FIELDS
    // ========================================
    
    /// <summary>
    /// User who revoked the consent (if applicable)
    /// </summary>
    [MaxLength(255)]
    public string? RevokedBy { get; set; }

    /// <summary>
    /// When consent was revoked (if applicable)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    // ========================================
    // ELECTRONIC CONSENT FIELDS
    // ========================================
    
    /// <summary>
    /// Whether consent was given electronically
    /// </summary>
    public bool IsElectronicConsent { get; set; }

    /// <summary>
    /// IP address where consent was given
    /// </summary>
    [MaxLength(45)]
    public string? ConsentIpAddress { get; set; }

    /// <summary>
    /// User agent/browser information
    /// </summary>
    public string? UserAgent { get; set; }

    // ========================================
    // DETAILS FIELDS
    // ========================================
    
    /// <summary>
    /// Purpose of the consent
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Additional details about the consent
    /// </summary>
    public string? Details { get; set; }

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Whether consent is currently valid
    /// </summary>
    [NotMapped]
    public bool IsValid => IsActive && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow) && RevokedAt == null;

    /// <summary>
    /// Whether consent has expired
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Whether consent has been revoked
    /// </summary>
    [NotMapped]
    public bool IsRevoked => RevokedAt.HasValue;

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    
    /// <summary>
    /// Navigation property for patient
    /// </summary>
    public virtual Patient Patient { get; set; } = null!;
}
