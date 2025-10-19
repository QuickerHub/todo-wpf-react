using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using Panacean.Data.Interface;
using CommunityToolkit.Mvvm.ComponentModel;
using TodoApp.Core.Extensions;

namespace TodoApp.Core.Models;

/// <summary>
/// 自动保存项基类，实现属性变更时自动保存
/// </summary>
/// <typeparam name="TItem">项类型</typeparam>
[INotifyPropertyChanged]
public abstract partial class AutoSaveItem<TItem> : EntityBase<string>, IConfigItem
    where TItem : notnull
{
    /// <summary>
    /// 默认构造函数，自动生成 ID
    /// </summary>
    public AutoSaveItem() => Id = Guid.NewGuid().ToBase62();

    /// <summary>
    /// JSON 构造函数
    /// </summary>
    /// <param name="id">项 ID</param>
    [JsonConstructor]
    public AutoSaveItem(string id) => Id = id;

    /// <summary>
    /// 获取数据服务
    /// </summary>
    /// <returns>数据服务实例</returns>
    public abstract IDataService<TItem, string> GetDataService();

    private IDisposable? _observer;
    
    /// <summary>
    /// 配置初始化完成后调用
    /// </summary>
    public void PostInit()
    {
        _observer ??= this.ObserveChange()
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ => GetDataService().Save((TItem)(object)this));
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _observer?.Dispose();
    }
}
