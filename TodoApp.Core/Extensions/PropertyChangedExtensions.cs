using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TodoApp.Core.Extensions;

/// <summary>
/// 属性变更扩展方法
/// </summary>
public static class PropertyChangedExtensions
{
    /// <summary>
    /// 观察属性变更
    /// </summary>
    /// <param name="source">源对象</param>
    /// <returns>属性变更的可观察序列</returns>
    public static IObservable<object> ObserveChange(this INotifyPropertyChanged source)
    {
        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
            handler => source.PropertyChanged += handler,
            handler => source.PropertyChanged -= handler)
            .Select(_ => source);
    }
}
