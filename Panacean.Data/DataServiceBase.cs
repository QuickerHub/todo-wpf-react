using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Panacean.Data.Interface;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Panacean.Data
{
    // Abstract base class for data services to reduce code duplication
    public abstract partial class DataServiceBase<TItem, TWrapper> : ObservableObject, IDataService<TItem, string>, IDisposable
        where TItem : class, IEntity<string>
        where TWrapper : JsonItemWrapper<TItem, string>, new()
    {
        protected readonly ILogger _logger;
        protected readonly DataBaseJsonItemCacheProvider<TItem, TWrapper, string> _cacheProvider;
        protected readonly SourceCache<TItem, string> _sourceCache;
        protected readonly IObservableCache<TItem, string> _all;
        protected readonly IDisposable _sort;
        //protected readonly IDisposable _autoSaveSubscription;
        private readonly ReadOnlyObservableCollection<TItem> _items;
        public IObservableCache<TItem, string> All => _all;
        public SourceCache<TItem, string> Cache => _sourceCache;
        public IList<TItem> Items => _items;

        protected DataServiceBase(IServiceProvider serviceProvider, IComparer<TItem>? comparer = null)
        {
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(GetType());

            // Initialize database cache provider
            _cacheProvider = new DataBaseJsonItemCacheProvider<TItem, TWrapper, string>(
                serviceProvider: serviceProvider,
                keySelector: item => item.Id
            );

            _sourceCache = _cacheProvider.Cache;
            _all = _sourceCache.AsObservableCache();

            // Setup sorting and binding
            _sort = _all.Connect()
                .SortAndBind(out _items, comparer ?? 
                    SortExpressionComparer<TItem>.Descending(i => i.UpdateTime))
                .Subscribe();

            _all.Connect()
                .WhereReasonsAre(ChangeReason.Add)
                .ForEachChange(change =>
                {
                    if (change.Current is IConfigItem configItem)
                    {
                        configItem.PostInit();
                    }
                })
                .Subscribe();

            _all.Connect()
                .WhereReasonsAre(ChangeReason.Remove)
                .ForEachChange(change =>
                {
                    if (change.Current is IConfigItem configItem)
                    {
                        configItem.Dispose();
                    }
                })
                .Subscribe();
        }

        public virtual TItem? GetItem(string key)
        {
            var item = _sourceCache.Lookup(key);
            return item.HasValue ? item.Value : null;
        }

        public virtual bool Remove(TItem item)
        {
            try
            {
                _sourceCache.Remove(item);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove item: {ItemId}", item?.Id ?? "Unknown");
                throw ex;
            }
        }

        public virtual bool Remove(IEnumerable<TItem> items)
        {
            try
            {
                _sourceCache.Remove(items);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove items");
                throw ex;
            }
        }

        public virtual void Save(TItem item)
        {
            try
            {
                item.UpdateTime = DateTime.Now;

                // Always use AddOrUpdate to ensure database sync
                // The UI should be resilient to these updates
                _sourceCache.AddOrUpdate(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save item: {ItemId}", item.Id);
                throw ex;
            }
        }

        public virtual void Save(IEnumerable<TItem> items)
        {
            try
            {
                foreach (var item in items)
                {
                    item.UpdateTime = DateTime.Now;
                }
                // Always use AddOrUpdate to ensure database sync
                // The UI should be resilient to these updates
                _sourceCache.AddOrUpdate(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save items");
                throw ex;
            }
        }

        public virtual void Dispose()
        {
            _sort?.Dispose();
            _cacheProvider?.Dispose();
        }
    }
}
