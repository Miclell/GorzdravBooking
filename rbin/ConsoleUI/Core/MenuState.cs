using ConsoleUI.Core.Interfaces;

namespace ConsoleUI.Core;

public class MenuState
{
    public string? Title { get; set; }
    public List<IMenuCommand> Commands { get; set; } = [];
    public object? Data { get; set; }
}