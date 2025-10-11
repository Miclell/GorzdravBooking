namespace ConsoleUI.Core;

public class NavigationStack
{
    private readonly Stack<MenuState> _stack = new();
    private readonly Dictionary<string, object> _sharedData = new();
    
    public void Push(MenuState state) => _stack.Push(state);
    public MenuState Pop() 
    {
        var state = _stack.Pop();
        CleanupStateData(state); 
        return state;
    }
    public MenuState Peek() => _stack.Peek();
    public int Count => _stack.Count;
    public void Clear() => _stack.Clear();
    
    public void SetData(string key, object value) => _sharedData[key] = value;
    public T GetData<T>(string key) => (T)_sharedData[key];

    public void DeleteData(string key) => _sharedData.Remove(key);
    
    private void CleanupStateData(MenuState state)
    {
        var keysToRemove = _sharedData
            .Where(x => x.Value.GetType().Name == state.GetType().Name + "Data")
            .Select(x => x.Key)
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _sharedData.Remove(key);
        }
    }
}