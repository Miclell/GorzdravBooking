using StatefulMenu.Core.Models;

namespace StatefulMenu.Infrastructure.Components;

public class MenuRenderer
{
    public void Render(MenuState state, int selectedIndex = 0)
    {
        // Safe clear
        try
        {
            Console.SetCursorPosition(0, 0);
            for (var i = 0; i < Console.WindowHeight; i++) Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
        }
        catch
        {
            Console.Clear();
        }

        Console.WriteLine($"=== {state.Title} ===");
        Console.WriteLine();

        for (var i = 0; i < state.Items.Count; i++)
        {
            var item = state.Items[i];
            var prefix = i == selectedIndex ? "> " : "  ";
            var hotkey = item.Hotkey.HasValue ? $"[{item.Hotkey}] " : string.Empty;
            Console.WriteLine($"{prefix}{i + 1}. {hotkey}{item.Title}");
        }

        Console.WriteLine();
        Console.Write("Enter/цифра — выбрать, ↑/↓ — навигация, Esc — назад");
    }
}