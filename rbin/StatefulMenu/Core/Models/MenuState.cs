namespace StatefulMenu.Core.Models;

public class MenuState
{
    public MenuState(string? title, IEnumerable<MenuItem> items, object? snapshot = null)
    {
        Title = title;
        Items = items is IReadOnlyList<MenuItem> list ? list : new List<MenuItem>(items);
        Snapshot = snapshot;
    }

    public string? Title { get; }
    public IReadOnlyList<MenuItem> Items { get; }
    public object? Snapshot { get; }
}