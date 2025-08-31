using System.ComponentModel.DataAnnotations;

namespace FHIRAI.Domain.Common;

/// <summary>
/// Base entity with audit fields for tracking creation, modification, and deletion
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    // ========================================
    // TIMING FIELDS
    // ========================================
    
    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last modified
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; set; }

    /// <summary>
    /// Who last modified the entity
    /// </summary>
    public string? LastModifiedBy { get; set; }

    // ========================================
    // SOFT DELETE FIELDS
    // ========================================
    
    /// <summary>
    /// Whether the entity is marked as deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// When the entity was deleted
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Who deleted the entity
    /// </summary>
    public string? DeletedBy { get; set; }
}
