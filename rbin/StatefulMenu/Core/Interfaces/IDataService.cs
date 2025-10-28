namespace StatefulMenu.Core.Interfaces;

public interface IDataService
{
    void Set(string key, object value);
    bool TryGet<T>(string key, out T? value);
    bool Remove(string key);
}