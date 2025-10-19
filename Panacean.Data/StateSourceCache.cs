using DynamicData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace Panacean.Data;

/// <summary>
/// 轻量级状态管理缓存，用于简单状态的持久化存储
/// 只在首次启动时加载文件，之后只保存状态变更
/// 内置节流功能，防止频繁写入文件
/// </summary>
/// <typeparam name="TObject">状态对象类型</typeparam>
/// <typeparam name="TKey">状态对象的唯一标识符类型</typeparam>
public abstract class StateSourceCache<TObject, TKey> : IDisposable
    where TObject : notnull
    where TKey : notnull
{
    public SourceCache<TObject, TKey> SourceCache => _cache;
    protected readonly SourceCache<TObject, TKey> _cache;
    protected readonly string _dir;
    protected readonly string _filter;
    private readonly ConcurrentDictionary<TKey, DateTime> _lastObjectUpdateTime = new();
    private readonly ConcurrentDictionary<TKey, IDisposable> _saveThrottleSubscriptions = new();
    private readonly int _delayMs;
    private readonly ILogger _logger;

    protected StateSourceCache(string dir, ILoggerFactory loggerFactory, string filter = "*json", int delayMs = 50)
    {
        _cache = new(Object2Key);
        _dir = dir;
        _filter = filter;
        _delayMs = delayMs;
        _logger = loggerFactory.CreateLogger<StateSourceCache<TObject, TKey>>();

        // 确保目录存在
        Directory.CreateDirectory(dir);

        // 加载现有文件
        LoadAllFiles();

        // 监听对象变更
        _cache.Connect()
            .WhereReasonsAre(ChangeReason.Add, ChangeReason.Update)
            .Subscribe(changes =>
            {
                foreach (var change in changes)
                {
                    SetupThrottledSave(change.Current);
                }
            });

        // 监听删除
        _cache.Connect()
            .WhereReasonsAre(ChangeReason.Remove)
            .Subscribe(changes =>
            {
                foreach (var change in changes)
                {
                    RemoveFile(change.Current);
                    var key = Object2Key(change.Current);
                    if (_saveThrottleSubscriptions.TryRemove(key, out var subscription))
                    {
                        subscription.Dispose();
                    }
                }
            });
    }

    private void LoadAllFiles()
    {
        var objects = new List<TObject>();
        foreach (var file in Directory.GetFiles(_dir, _filter))
        {
            var obj = File2Object(file);
            if (obj != null)
            {
                objects.Add(obj);
                _lastObjectUpdateTime[Object2Key(obj)] = Object2UpdateTime(obj);
            }
        }

        if (objects.Count > 0)
        {
            _cache.AddOrUpdate(objects);
        }
    }

    /// <summary>
    /// 设置对象的节流保存
    /// </summary>
    private void SetupThrottledSave(TObject obj)
    {
        var key = Object2Key(obj);

        // 如果已经有了一个节流订阅，则先取消
        if (_saveThrottleSubscriptions.TryRemove(key, out var existingSubscription))
        {
            existingSubscription.Dispose();
        }

        // 创建一个subject用于触发保存
        var saveSubject = new Subject<TObject>();

        // 添加节流保存逻辑
        var subscription = saveSubject
            .Throttle(TimeSpan.FromMilliseconds(_delayMs))
            .Subscribe(o =>
            {
                SaveObjectToFile(o);
                if (_saveThrottleSubscriptions.TryRemove(key, out var sub))
                {
                    sub.Dispose();
                }
            });

        _saveThrottleSubscriptions[key] = subscription;

        // 触发保存操作
        saveSubject.OnNext(obj);
    }

    private void SaveObjectToFile(TObject obj)
    {
        var key = Object2Key(obj);

        if (_lastObjectUpdateTime.TryGetValue(key, out var lastTime)
            && lastTime == Object2UpdateTime(obj))
        {
            return; // 对象未更改，无需保存
        }

        _lastObjectUpdateTime[key] = Object2UpdateTime(obj);

        // 保存到文件
        var file = KeyToFile(key);
        try
        {
            SaveObject2File(obj, file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存状态到文件 {File} 时出错: {Message}", file, ex.Message);
        }
    }

    private void RemoveFile(TObject obj)
    {
        var key = Object2Key(obj);
        var file = KeyToFile(key);

        _lastObjectUpdateTime.TryRemove(key, out _);

        try
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除文件 {File} 时出错: {Message}", file, ex.Message);
        }
    }

    #region Abstract Methods

    /// <summary>
    /// 获取对象的唯一标识符
    /// </summary>
    protected abstract TKey Object2Key(TObject obj);

    /// <summary>
    /// 从文件加载对象
    /// </summary>
    protected abstract TObject? File2Object(string file);

    /// <summary>
    /// 从标识符生成文件路径
    /// </summary>
    protected abstract string KeyToFile(TKey key);

    /// <summary>
    /// 从文件名获取标识符
    /// </summary>
    protected abstract TKey File2Key(string file);

    /// <summary>
    /// 将对象保存到文件
    /// </summary>
    protected abstract void SaveObject2File(TObject obj, string file);

    /// <summary>
    /// 获取对象的最后更新时间
    /// </summary>
    protected abstract DateTime Object2UpdateTime(TObject obj);

    #endregion

    #region Public Methods

    /// <summary>
    /// 保存或更新状态对象
    /// </summary>
    public void Save(TObject obj) => _cache.AddOrUpdate(obj);

    /// <summary>
    /// 移除状态对象
    /// </summary>
    public void Remove(TKey key) => _cache.Remove(key);

    /// <summary>
    /// 获取所有状态对象
    /// </summary>
    public IEnumerable<TObject> GetAll() => _cache.Items;

    #endregion

    public virtual void Dispose()
    {
        // 取消所有节流订阅
        foreach (var subscription in _saveThrottleSubscriptions.Values)
        {
            subscription.Dispose();
        }
        _saveThrottleSubscriptions.Clear();

        _cache?.Dispose();
    }
}

/// <summary>
/// 使用JSON格式存储状态的StateSourceCache实现
/// </summary>
/// <typeparam name="TObject">状态对象类型</typeparam>
public abstract class JsonStateSourceCache<TObject>(string dir, ILoggerFactory loggerFactory, int delayMs = 50) : StateSourceCache<TObject, string>(dir, loggerFactory, "*.json", delayMs)
    where TObject : notnull
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<JsonStateSourceCache<TObject>>();

    protected override string File2Key(string file) => Path.GetFileNameWithoutExtension(file);

    protected override TObject? File2Object(string file)
    {
        try
        {
            if (File.Exists(file))
            {
                return Json2Object(File.ReadAllText(file));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从文件 {File} 加载JSON时出错: {Message}", file, ex.Message);
        }
        return default;
    }

    protected override string KeyToFile(string key) => Path.Combine(_dir, $"{key}.json");

    protected override void SaveObject2File(TObject obj, string file)
    {
        var content = Object2Json(obj);
        File.WriteAllText(file, content);
    }

    /// <summary>
    /// 将JSON字符串转换为对象
    /// </summary>
    protected abstract TObject? Json2Object(string json);

    /// <summary>
    /// 将对象转换为JSON字符串
    /// </summary>
    protected abstract string Object2Json(TObject obj);
}