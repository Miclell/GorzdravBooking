using ConsoleUI.Components;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Services;

public class NavigationService(NavigationStack stack, MenuRenderer renderer, IServiceProvider serviceProvider)
{
    public async Task StartAsync()
    {
        var mainMenu = serviceProvider.GetRequiredService<IMenuProvider>();
        var initialState = await mainMenu.CreateMenuAsync();
        stack.Push(initialState);
        
        await RunNavigationLoopAsync();
    }

    public IMenuCommand CreateCommand<T>() where T : IMenuCommand
    {
        return serviceProvider.GetRequiredService<T>();
    }
    
    public void SetSharedData(string key, object value)
    {
        stack.SetData(key, value);
    }
    
    public T GetSharedData<T>(string key)
    {
        return stack.GetData<T>(key);
    }

    public void DeleteSharedData(string key)
    {
        stack.DeleteData(key);
    }
    
    private async Task RunNavigationLoopAsync()
    {
        while (stack.Count > 0)
        {
            var currentState = stack.Peek();
            renderer.Render(currentState);

            if (!int.TryParse(Console.ReadLine(), out var choice) ||
                choice <= 0 || choice > currentState.Commands.Count) continue;
            
            var selectedCommand = currentState.Commands[choice - 1];
            var newState = await selectedCommand.ExecuteAsync();
            
            if (newState != null)
            {
                stack.Push(newState);
            }
            else
            {
                if (stack.Count > 1)
                    stack.Pop();
            }
        }
    }
}