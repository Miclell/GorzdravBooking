using Application.Services.Interfaces;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class SelectTimePreferencesProvider(
    IAppSettingsService appSettingsService,
    ITimePreferencesService timePreferencesService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var tps = await timePreferencesService.GetByUserAsync(await appSettingsService.GetDefaultUserIdAsync(),
            cancellationToken);

        if (!tps.Value.Any())
            Console.WriteLine("Не найдено ни одного пресета, создайте пресеты!");

        var commands = tps.Value
            .Select(tp => new SelectTimePreferencesCommand(tp, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите временные предпочтения", items, header: HeaderFactorySetup.SetupHeader());
    }
}