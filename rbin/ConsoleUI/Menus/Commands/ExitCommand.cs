using ConsoleUI.Core.Interfaces;

namespace ConsoleUI.Core;

public class ExitCommand : IMenuCommand
{
    public string Title => "Выход";
    
    public Task<MenuState?> ExecuteAsync()
    {
        Environment.Exit(0);
        return null;
    }
}