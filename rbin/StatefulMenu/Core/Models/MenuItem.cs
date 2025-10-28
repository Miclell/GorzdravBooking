namespace StatefulMenu.Core.Models;

public sealed class MenuItem
{
    public MenuItem(string title, Func<CancellationToken, Task<MenuResult>> action, ConsoleKey? hotkey = null)
    {
        Title = title;
        Action = action;
        Hotkey = hotkey;
    }

    public string Title { get; }
    public Func<CancellationToken, Task<MenuResult>> Action { get; }
    public ConsoleKey? Hotkey { get; }
}