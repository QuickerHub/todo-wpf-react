using System;
using System.Collections.Generic;
using Panacean.Data.Interface;
using Newtonsoft.Json;

namespace Panacean.Data;

// Generic database cache provider for JSON-serialized items
public class DataBaseJsonItemCacheProvider<TItem, TWrapper, TKey>(
    IServiceProvider serviceProvider,
    Func<TItem, TKey> keySelector)
    : DatabaseCacheProvider<TItem, TWrapper, TKey>(serviceProvider, keySelector)
    where TItem : IEntity<TKey>
    where TWrapper : JsonItemWrapper<TItem, TKey>, new()
    where TKey : notnull, IEquatable<TKey>
{
    protected override TWrapper ItemToWrapper(TItem item)
    {
        var wrapper = new TWrapper
        {
            Id = item.Id,
            UpdateTime = item.UpdateTime,
            Data = JsonConvert.SerializeObject(item),
        };
        return wrapper;
    }

    protected override TItem? WrapperToItem(TWrapper wrapper)
    {
        return wrapper.ToItem();
    }
}
