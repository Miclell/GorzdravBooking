using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;

public class RunPatientFlowCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Создать пациента";
    public async Task<MenuState?> ExecuteAsync()
    {
        var districtMenuProvider = serviceProvider.GetRequiredService<DistrictMenuProvider>();

        return await districtMenuProvider.CreateMenuAsync();
    }
}