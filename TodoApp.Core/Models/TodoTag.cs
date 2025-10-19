using System.ComponentModel.DataAnnotations;

namespace TodoApp.Core.Models;

/// <summary>
/// Todo tag entity
/// </summary>
public class TodoTag
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tag name
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tag color (hex color code)
    /// </summary>
    [StringLength(7)]
    public string Color { get; set; } = "#007bff";

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Associated tasks
    /// </summary>
    public List<TodoTask> Tasks { get; set; } = new();
}
