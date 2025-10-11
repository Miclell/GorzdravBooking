using ConsoleUI.Core.Interfaces;

namespace ConsoleUI.Core;

public class BackCommand : IMenuCommand
{
    public string Title => "Назад";
    
    public Task<MenuState?> ExecuteAsync()
    {
        return Task.FromResult<MenuState?>(null);
    }
}