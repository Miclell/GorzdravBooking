using CLI.Helpers;
using CLI.Menus.PatientMenu.CreatePatientFlow;
using CLI.Menus.PatientMenu.ShowPatientsFlow;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu;

public class ShowPatientMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<RunCreatePatientFlowCommand>(),
            serviceProvider.GetRequiredService<RunShowPatientsFlowCommand>(),
            serviceProvider.GetRequiredService<BackCommand>()
        };

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return Task.FromResult(new MenuState("Меню пациента", items, header: HeaderFactorySetup.SetupHeader()));
    }
}