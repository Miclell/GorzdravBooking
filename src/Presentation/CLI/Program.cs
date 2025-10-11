using Microsoft.Extensions.DependencyInjection;
using StatefulMenu;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;

namespace CLI;

class Program
{
    // TODO переделать либу в тотал это пизда
    
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddStatefulMenu();
        
        var provider = services.BuildServiceProvider();
        var nav = provider.GetRequiredService<INavigationService>();
        
        var root = provider.GetRequiredService<IMenuProvider>();

        await nav.RunAsync(root);
        
        Console.WriteLine("Hello, World!");
    }
}