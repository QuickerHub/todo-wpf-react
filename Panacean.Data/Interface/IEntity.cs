using System;
using FreeSql.DataAnnotations;

namespace Panacean.Data.Interface
{
    /// <summary>
    /// 实体基类，定义所有可持久化实体的共同特性
    /// </summary>
    public abstract class EntityBase<TKey> : IEntity<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// 实体唯一标识符
        /// </summary>
        [Column(IsIdentity = true, IsPrimary = true)]
        public TKey Id { get; set; } = default!;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 实体接口，用于泛型约束（保留向后兼容性）
    /// </summary>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// 实体唯一标识符
        /// </summary>
        TKey Id { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        DateTime UpdateTime { get; set; }
    }
}