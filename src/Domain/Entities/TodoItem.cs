using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FHIRAI.Domain.Common;
using FHIRAI.Domain.Enums;
using FHIRAI.Domain.Events;

namespace FHIRAI.Domain.Entities;

/// <summary>
/// Represents a todo item within a todo list
/// </summary>
public class TodoItem : BaseAuditableEntity
{
    // ========================================
    // FOREIGN KEY FIELDS
    // ========================================
    
    /// <summary>
    /// Foreign key to the parent todo list
    /// </summary>
    [Required]
    public int ListId { get; set; }

    // ========================================
    // CORE IDENTITY FIELDS
    // ========================================
    
    /// <summary>
    /// Title of the todo item
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    // ========================================
    // BASIC INFORMATION FIELDS
    // ========================================
    
    /// <summary>
    /// Additional notes for the todo item
    /// </summary>
    [MaxLength(1000)]
    public string? Note { get; set; }

    // ========================================
    // STATUS & CONFIGURATION FIELDS
    // ========================================
    
    /// <summary>
    /// Priority level of the todo item
    /// </summary>
    [Required]
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    /// <summary>
    /// Whether the todo item is completed
    /// </summary>
    public bool Done
    {
        get => _done;
        set
        {
            if (value && !_done)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }

            _done = value;
        }
    }

    // ========================================
    // TIMING FIELDS
    // ========================================
    
    /// <summary>
    /// Reminder date and time for the todo item
    /// </summary>
    public DateTime? Reminder { get; set; }

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Whether the todo item is overdue
    /// </summary>
    [NotMapped]
    public bool IsOverdue => Reminder.HasValue && Reminder.Value < DateTime.UtcNow && !Done;

    /// <summary>
    /// Whether the todo item is due soon (within 24 hours)
    /// </summary>
    [NotMapped]
    public bool IsDueSoon => Reminder.HasValue && 
                           Reminder.Value > DateTime.UtcNow && 
                           Reminder.Value <= DateTime.UtcNow.AddHours(24) && 
                           !Done;

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    
    /// <summary>
    /// Navigation property to the parent todo list
    /// </summary>
    public virtual TodoList List { get; set; } = null!;

    // Private backing field for Done property
    private bool _done;
}
