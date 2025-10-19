using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using FreeSql;
using Panacean.Data.Interface;

namespace Panacean.Data;

public interface ICacheProvider<TObject, TKey> : IDisposable
    where TObject : notnull
    where TKey : notnull, IEquatable<TKey>
{
    SourceCache<TObject, TKey> Cache { get; }
}


public abstract class DatabaseCacheProvider<TObject, TWrapper, TKey> : ICacheProvider<TObject, TKey>
    where TObject : notnull
    where TWrapper : ItemWrapper<TKey>, new()
    where TKey : notnull, IEquatable<TKey>
{
    // 数据缓存
    private readonly SourceCache<TObject, TKey> _itemCache;
    public SourceCache<TObject, TKey> Cache => _itemCache;

    // FreeSql Repository 实例 - 简化 CRUD 操作
    private readonly IBaseRepository<TWrapper> _repository;
    private readonly ILogger _logger;
    private readonly CompositeDisposable _disposables = [];

    protected DatabaseCacheProvider(IServiceProvider serviceProvider, Func<TObject, TKey> keySelector)
    {
        _itemCache = new SourceCache<TObject, TKey>(keySelector);
        _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<DatabaseCacheProvider<TObject, TWrapper, TKey>>();

        // 获取 FreeSql 数据库实例并创建 Repository
        var dbManager = serviceProvider.GetRequiredService<IDatabaseManager>();
        var db = dbManager.GetDatabase();
        _repository = db.GetRepository<TWrapper>();

        // 确保数据库表结构存在
        db.CodeFirst.SyncStructure<TWrapper>();

        // 在后台异步加载数据，不阻塞构造函数
        Task.Run(async () =>
        {
            var hasData = false;
            try
            {
                hasData = await LoadAllItemsFromDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "后台异步加载数据失败");
            }
            finally
            {
                SetupCacheToDbSync(skipInitial: hasData);
            }
        }).Wait();

        _logger.LogInformation("已初始化数据库缓存提供者: {ItemType}, {WrapperType}", typeof(TObject).Name, typeof(TWrapper).Name);
    }

    /// <summary>
    /// 设置缓存到数据库的单向同步
    /// </summary>
    private void SetupCacheToDbSync(bool skipInitial)
    {
        // 只监听缓存变更同步到数据库 - 移到后台线程执行
        var cacheSubscription = _itemCache.Connect()
            .Skip(skipInitial ? 1 : 0)  // 如果需要跳过初始通知，则跳过1次，否则跳过0次
            .ObserveOn(TaskPoolScheduler.Default) // Move to background thread
            .Subscribe(changes =>
            {
                try
                {
                    SyncCacheToDatabaseBatch(changes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "同步缓存到数据库时发生错误");
                }
            });

        _disposables.Add(cacheSubscription);
    }

    private void SyncCacheToDatabaseSingle(IChangeSet<TObject,TKey> changes)
    {
        try
        {
            foreach (var change in changes)
            {
                switch (change.Reason)
                {
                    case ChangeReason.Add:
                        var addWrapper = ItemToWrapper(change.Current);
                        addWrapper.UpdateTime = DateTime.Now;
                        _repository.InsertOrUpdate(addWrapper);
                        break;

                    case ChangeReason.Update:
                        var updateWrapper = ItemToWrapper(change.Current);
                        updateWrapper.UpdateTime = DateTime.Now;
                        _repository.InsertOrUpdate(updateWrapper);
                        break;

                    case ChangeReason.Remove:
                        _repository.Delete(a => a.Id.Equals(change.Key));
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "单个同步缓存到数据库时发生错误");
        }
    }


    /// <summary>
    /// 同步缓存变更到数据库 - 批量处理优化
    /// </summary>
    private void SyncCacheToDatabaseBatch(IChangeSet<TObject, TKey> changes)
    {
        try
        {
            // 按变更类型分组处理
            var groupedChanges = changes.GroupBy(c => c.Reason);

            foreach (var group in groupedChanges)
            {
                switch (group.Key)
                {
                    case ChangeReason.Add:
                        var addItems = group.Select(ConvertToWrapper).ToList();
                        BatchInsert(addItems);
                        break;

                    case ChangeReason.Update:
                        var updateItems = group.Select(ConvertToWrapper).ToList();
                        BatchUpdate(updateItems);
                        break;

                    case ChangeReason.Remove:
                        var removeKeys = group.Select(c => c.Key).ToList();
                        BatchDelete(removeKeys);
                        break;
                }
            }

            // 局部方法：将变更项转换为包装对象
            TWrapper ConvertToWrapper(Change<TObject, TKey> change)
            {
                var wrapper = ItemToWrapper(change.Current);
                wrapper.UpdateTime = DateTime.Now;
                return wrapper;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量同步缓存到数据库时发生错误");
        }
    }

    /// <summary>
    /// 批量插入实体
    /// </summary>
    private void BatchInsert(List<TWrapper> wrappers)
    {
        if (wrappers.Count == 0) return;

        try
        {
            var affectedRows = _repository.Insert(wrappers);
            _logger.LogDebug("批量插入 {Count} 个实体，影响行数: {AffectedRows}", wrappers.Count, affectedRows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入实体时出错，实体数量: {Count}", wrappers.Count);
            throw;
        }
    }

    /// <summary>
    /// 批量更新实体
    /// </summary>
    private void BatchUpdate(List<TWrapper> wrappers)
    {
        if (wrappers.Count == 0) return;

        try
        {
            var affectedRows = _repository.Update(wrappers);
            _logger.LogDebug("批量更新 {Count} 个实体，影响行数: {AffectedRows}", wrappers.Count, affectedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新实体时出错，实体数量: {Count}", wrappers.Count);
            throw;
        }
    }

    /// <summary>
    /// 批量删除实体
    /// </summary>
    private void BatchDelete(List<TKey> keys)
    {
        if (keys.Count == 0) return;

        try
        {
            var keySet = new HashSet<TKey>(keys);
            var affectedRows = _repository.Delete(a => keySet.Contains(a.Id));
            _logger.LogDebug("批量删除 {Count} 个实体，影响行数: {AffectedRows}", keys.Count, affectedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除实体时出错，实体数量: {Count}", keys.Count);
            throw;
        }
    }



    /// <summary>
    /// 从数据库异步加载所有数据到内存缓存
    /// </summary>
    private async Task<bool> LoadAllItemsFromDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("开始从数据库加载{ItemType}数据...", typeof(TObject).Name);

            // 异步获取数据
            var items = await Task.Run(() =>
            {
                var wrappers = _repository.Select.ToList();
                return wrappers
                    .Select(wrapper => WrapperToItem(wrapper))
                    .Where(item => item != null)
                    .Cast<TObject>()
                    .ToList();
            });

            // 批量添加到缓存
            if (items.Count > 0)
            {
                _itemCache.AddOrUpdate(items);
            }

            _logger.LogInformation("成功从数据库加载了 {Count} 个{ItemType}项目", items.Count, typeof(TObject).Name);
            return items.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从数据库异步加载数据失败: {Message}", ex.Message);
            return false;
        }
    }

    #region 抽象方法 - 子类需要实现对象转换逻辑

    /// <summary>
    /// 将业务对象转换为数据库包装对象
    /// </summary>
    protected abstract TWrapper ItemToWrapper(TObject item);

    /// <summary>
    /// 将数据库包装对象转换为业务对象
    /// </summary>
    protected abstract TObject? WrapperToItem(TWrapper wrapper);

    #endregion

    public void Dispose()
    {
        _disposables.Dispose();
        _itemCache.Dispose();
        // DatabaseManager 负责释放 IFreeSql 实例
        GC.SuppressFinalize(this);
    }
}