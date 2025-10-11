using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.ShowPatientsFlow.Commands;

public class ShowPatientsCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Просмотр и редактирование пациентов";
    public async Task<MenuState?> ExecuteAsync()
    {
        var showPatientsProvider = serviceProvider.GetRequiredService<ShowPatientsProvider>();
        return await showPatientsProvider.CreateMenuAsync();
    }
}