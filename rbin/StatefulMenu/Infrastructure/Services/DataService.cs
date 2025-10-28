using System.Collections.Concurrent;
using StatefulMenu.Core.Interfaces;

namespace StatefulMenu.Infrastructure.Services;

public class DataService : IDataService
{
    private readonly ConcurrentDictionary<string, object> _data = new();

    public void Set(string key, object value)
    {
        _data[key] = value;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (_data.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public bool Remove(string key)
    {
        return _data.TryRemove(key, out _);
    }
}