namespace ConsoleUI.Core.Interfaces;

public interface IMenuCommand
{
    string Title { get; }
    Task<MenuState?> ExecuteAsync();
}