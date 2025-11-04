using Application.Services.Interfaces;
using CLI.Helpers;
using CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Commands;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Providers;

public class ShowTimePreferencesProvider(
    IServiceProvider serviceProvider,
    ITimePreferencesService timePreferencesService) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var appSettingService = serviceProvider.GetRequiredService<IAppSettingsService>();
        var timePreferences =
            await timePreferencesService.GetByUserAsync(await appSettingService.GetDefaultUserIdAsync(),
                cancellationToken);

        var commands = timePreferences.Value
            .Select(tp => new TimePreferencesSelectionCommand(tp, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите пресет для удаления", items, header: HeaderFactorySetup.SetupHeader());
    }
}