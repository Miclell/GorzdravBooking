namespace ConsoleUI.Core.Interfaces;

public interface IMenuProvider
{
    Task<MenuState> CreateMenuAsync();
}