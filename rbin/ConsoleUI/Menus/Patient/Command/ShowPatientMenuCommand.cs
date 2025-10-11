using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.Command;

public class ShowPatientMenuCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Пациент";
    public async Task<MenuState?> ExecuteAsync()
    {
        var showPatientMenuProvider = serviceProvider.GetRequiredService<ShowPatientMenuProvider>();
        return await showPatientMenuProvider.CreateMenuAsync();
    }
}