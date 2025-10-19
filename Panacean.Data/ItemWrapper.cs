using System;
using Panacean.Data.Interface;
using FreeSql.DataAnnotations;

namespace Panacean.Data;

/// <summary>
/// Item wrapper base class for database state caching
/// Used to wrap business objects with database persistence metadata
/// </summary>
/// <typeparam name="TKey">Primary key type</typeparam>
public abstract class ItemWrapper<TKey> : EntityBase<TKey> 
    where TKey : notnull
{
    /// <summary>
    /// Business object data in serialized format
    /// Derived classes should implement serialization logic
    /// </summary>
    [Column(StringLength = -1)] // Unlimited length for large objects
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Optional object type identifier for polymorphic scenarios
    /// </summary>
    [Column(StringLength = 100)]
    public string? ObjectType { get; set; }

    /// <summary>
    /// Creation time of the wrapper
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;
}

