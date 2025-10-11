using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;
using StatefulMenu.Infrastructure.Services;

namespace CLI.Menus;

public class MainMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken ct = new CancellationToken())
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        var items = new[]
        {
            new MenuItem("Сказать привет", async _ =>
            {
                Console.WriteLine("Привет!");
                return MenuResult.None();
            }, hotkey: ConsoleKey.H),

            new MenuItem("Подменю", async _ =>
            {
                var sub = new MenuState("Подменю", new[]
                {
                    new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop()))
                });
                return MenuResult.Push(sub);
            }) ,

            new MenuItem("Выход", _ => Task.FromResult(MenuResult.Exit()), hotkey: ConsoleKey.E)
        };

        return Task.FromResult(new MenuState("Главное меню", items));
    }
}