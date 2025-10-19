using System;
using System.Collections.Generic;
using DynamicData;

namespace Panacean.Data.Interface;

public interface IDataService<TItem, TKey>
    where TItem : notnull
    where TKey : notnull
{
    SourceCache<TItem, TKey> Cache { get; }
    IObservableCache<TItem, TKey> All { get; }
    TItem? GetItem(TKey key);
    bool Remove(TItem item);
    bool Remove(IEnumerable<TItem> items);
    void Save(TItem item);
    void Save(IEnumerable<TItem> item);
    IList<TItem> Items { get; }
}
