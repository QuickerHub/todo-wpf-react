using System;
using System.Reactive.Linq;

namespace Panacean.Data.Interface;

public interface IConfigItem : IDisposable
{
    /// <summary>
    /// 配置初始化完成后调用
    /// </summary>
    void PostInit();
}
