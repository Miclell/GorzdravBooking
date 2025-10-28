using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;
using StatefulMenu.Infrastructure.Components;

namespace StatefulMenu.Infrastructure.Services;

public class NavigationService(NavigationStack stack, MenuRenderer renderer, IServiceProvider serviceProvider)
    : INavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public event EventHandler<NavigatingEventArgs>? Navigating;
    public event EventHandler<NavigatedEventArgs>? Navigated;

    public void Push(MenuState state)
    {
        var from = stack.Count > 0 ? stack.Peek() : null;
        var args = new NavigatingEventArgs(from, state);
        Navigating?.Invoke(this, args);
        if (args.Cancel) return;
        stack.Push(state);
        Navigated?.Invoke(this, new NavigatedEventArgs(from, state));
    }

    public void Replace(MenuState state)
    {
        var from = stack.Count > 0 ? stack.Pop() : null;
        var args = new NavigatingEventArgs(from, state);
        Navigating?.Invoke(this, args);
        if (args.Cancel)
        {
            if (from != null) stack.Push(from);
            return;
        }

        stack.Push(state);
        Navigated?.Invoke(this, new NavigatedEventArgs(from, state));
    }

    public bool TryPop(int count = 1)
    {
        if (stack.Count < count) return false;
        var from = stack.Peek();
        for (var i = 0; i < count; i++) stack.Pop();
        var to = stack.Count > 0 ? stack.Peek() : null;
        Navigated?.Invoke(this, new NavigatedEventArgs(from, to));
        return true;
    }

    public MenuState? Peek()
    {
        return stack.Count > 0 ? stack.Peek() : null;
    }

    public int Count => stack.Count;

    public async Task RunAsync(IMenuProvider rootProvider, CancellationToken ct = default)
    {
        var initial = await rootProvider.CreateMenuAsync(ct);
        Push(initial);

        var selectedIndex = 0;

        while (Count > 0 && !ct.IsCancellationRequested)
        {
            var current = Peek()!;
            if (current.Items.Count == 0)
            {
                // Nothing to select; wait for any key to go back
                renderer.Render(current, selectedIndex);
                _ = Console.ReadKey(true);
                TryPop();
                continue;
            }

            // Clamp selection
            if (selectedIndex < 0) selectedIndex = 0;
            if (selectedIndex >= current.Items.Count) selectedIndex = current.Items.Count - 1;

            renderer.Render(current, selectedIndex);

            // Read keyboard input with hotkeys, arrows and digits
            var keyInfo = Console.ReadKey(true);

            if (ct.IsCancellationRequested) break;

            if (HandleNavigationKey(keyInfo, current.Items.Count, ref selectedIndex)) continue;

            if (TryResolveHotkey(current, keyInfo, out var hotkeyIndex))
            {
                selectedIndex = hotkeyIndex;
                var hkItem = current.Items[selectedIndex];
                var hkResult = await hkItem.Action(ct);
                await ApplyResultAsync(hkResult, ct);
                continue;
            }

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                var item = current.Items[selectedIndex];
                var result = await item.Action(ct);
                await ApplyResultAsync(result, ct);
                continue;
            }

            // Digit selection (1..N) - only select, don't execute
            if (char.IsDigit(keyInfo.KeyChar))
            {
                var idx = (int) char.GetNumericValue(keyInfo.KeyChar);
                if (idx >= 1 && idx <= current.Items.Count)
                {
                    selectedIndex = idx - 1;
                }
                continue;
            }

            // Escape -> Back
            if (keyInfo.Key == ConsoleKey.Escape) TryPop();
        }
    }

    private static bool HandleNavigationKey(ConsoleKeyInfo keyInfo, int itemsCount, ref int selectedIndex)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
                selectedIndex = (selectedIndex - 1 + itemsCount) % itemsCount;
                return true;
            case ConsoleKey.DownArrow:
                selectedIndex = (selectedIndex + 1) % itemsCount;
                return true;
            case ConsoleKey.Home:
                selectedIndex = 0;
                return true;
            case ConsoleKey.End:
                selectedIndex = Math.Max(0, itemsCount - 1);
                return true;
            default:
                return false;
        }
    }

    private static bool TryResolveHotkey(MenuState state, ConsoleKeyInfo keyInfo, out int index)
    {
        for (var i = 0; i < state.Items.Count; i++)
        {
            var hk = state.Items[i].Hotkey;
            if (hk.HasValue && hk.Value == keyInfo.Key)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private Task ApplyResultAsync(MenuResult result, CancellationToken ct)
    {
        switch (result)
        {
            case NoneResult:
                return Task.CompletedTask;
            case ExitResult:
                while (TryPop())
                {
                }

                return Task.CompletedTask;
            case PopResult pop:
                TryPop(pop.Count);
                return Task.CompletedTask;
            case PushResult push:
                Push(push.State);
                return Task.CompletedTask;
            case ReplaceResult replace:
                Replace(replace.State);
                return Task.CompletedTask;
            default:
                return Task.CompletedTask;
        }
    }
}