using StatefulMenu.Core.Models;

namespace StatefulMenu.Infrastructure.Components;

public class NavigationStack
{
    private readonly Dictionary<string, object> _sharedData = new();
    private readonly Stack<MenuState> _stack = new();

    public int Count => _stack.Count;

    public void Push(MenuState state)
    {
        _stack.Push(state);
    }

    public MenuState Pop()
    {
        var state = _stack.Pop();
        CleanupStateData(state);
        return state;
    }

    public MenuState Peek()
    {
        return _stack.Peek();
    }

    public void Clear()
    {
        _stack.Clear();
    }

    public void SetData(string key, object value)
    {
        _sharedData[key] = value;
    }

    public bool TryGetData<T>(string key, out T? value)
    {
        if (_sharedData.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public bool DeleteData(string key)
    {
        return _sharedData.Remove(key);
    }

    private void CleanupStateData(MenuState state)
    {
        // No-op by default: explicit cleanup should be controlled by caller using DeleteData
    }
}