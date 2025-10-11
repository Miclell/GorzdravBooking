using ConsoleUI.Core;

namespace ConsoleUI.Components;

public class MenuRenderer
{
    public void Render(MenuState state)
    {
        Console.SetCursorPosition(0, 0);
        for (var i = 0; i < Console.WindowHeight; i++)
        {
            Console.Write(new string(' ', Console.WindowWidth));
        }
        Console.SetCursorPosition(0, 0);
        
        Console.WriteLine($"=== {state.Title} ===");
        Console.WriteLine();
        
        for (var i = 0; i < state.Commands.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {state.Commands[i].Title}");
        }
        
        Console.WriteLine();
        Console.Write("Выберите вариант: ");
    }
}