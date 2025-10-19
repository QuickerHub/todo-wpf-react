using System;
using Panacean.Data.Interface;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Panacean.Data;

// Generic JSON item wrapper for entities
public class JsonItemWrapper<TItem, TKey> : ItemWrapper<TKey>
    where TKey : notnull
    where TItem : IEntity<TKey>
{

    public TItem? ToItem()
    {
        try
        {
            return JsonConvert.DeserializeObject<TItem>(Data);
        }
        catch (JsonException ex)
        {
            var dataPreview = Data.Length > 200 ? Data.Substring(0, 200) + "..." : Data;
            Debug.WriteLine($"[JsonItemWrapper] Failed to deserialize JSON for item with Id: {Id}. Data content: {dataPreview}. Exception: {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JsonItemWrapper] Unexpected error while deserializing item with Id: {Id}. Exception: {ex.Message}");
            return default;
        }
    }
}
