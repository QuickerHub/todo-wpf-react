using System.ComponentModel.DataAnnotations;

namespace TodoApp.Core.Models;

/// <summary>
/// Todo task entity
/// </summary>
public class TodoTask
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Task title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Task description
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Task completion status
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Task priority (1=Low, 2=Medium, 3=High)
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Due date (optional)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Associated tags
    /// </summary>
    public List<TodoTag> Tags { get; set; } = new();
}
