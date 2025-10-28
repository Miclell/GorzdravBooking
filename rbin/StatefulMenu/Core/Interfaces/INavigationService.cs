using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace StatefulMenu.Core.Interfaces;

public interface INavigationService
{
    int Count { get; }
    event EventHandler<NavigatingEventArgs> Navigating;
    event EventHandler<NavigatedEventArgs> Navigated;

    void Push(MenuState state);
    void Replace(MenuState state);
    bool TryPop(int count = 1);
    MenuState? Peek();
    Task RunAsync(IMenuProvider rootProvider, CancellationToken ct = default);
}

public sealed class NavigatingEventArgs : EventArgs
{
    public NavigatingEventArgs(MenuState? from, MenuState? to)
    {
        From = from;
        To = to;
    }

    public MenuState? From { get; }
    public MenuState? To { get; }
    public bool Cancel { get; set; }
}

public sealed class NavigatedEventArgs : EventArgs
{
    public NavigatedEventArgs(MenuState? from, MenuState? to)
    {
        From = from;
        To = to;
    }

    public MenuState? From { get; }
    public MenuState? To { get; }
}