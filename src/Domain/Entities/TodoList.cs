using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FHIRAI.Domain.Common;
using FHIRAI.Domain.ValueObjects;

namespace FHIRAI.Domain.Entities;

/// <summary>
/// Represents a collection of todo items
/// </summary>
public class TodoList : BaseAuditableEntity
{
    // ========================================
    // CORE IDENTITY FIELDS
    // ========================================
    
    /// <summary>
    /// Title of the todo list
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    // ========================================
    // STATUS & CONFIGURATION FIELDS
    // ========================================
    
    /// <summary>
    /// Color theme for the todo list
    /// </summary>
    [Required]
    public Colour Colour { get; set; } = Colour.White;

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Total number of items in the list
    /// </summary>
    [NotMapped]
    public int TotalItems => Items.Count;

    /// <summary>
    /// Number of completed items in the list
    /// </summary>
    [NotMapped]
    public int CompletedItems => Items.Count(x => x.Done);

    /// <summary>
    /// Number of pending items in the list
    /// </summary>
    [NotMapped]
    public int PendingItems => Items.Count(x => !x.Done);

    /// <summary>
    /// Completion percentage of the list
    /// </summary>
    [NotMapped]
    public double CompletionPercentage => TotalItems > 0 ? (double)CompletedItems / TotalItems * 100 : 0;

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    
    /// <summary>
    /// Collection of todo items in this list
    /// </summary>
    public IList<TodoItem> Items { get; private set; } = new List<TodoItem>();
}
