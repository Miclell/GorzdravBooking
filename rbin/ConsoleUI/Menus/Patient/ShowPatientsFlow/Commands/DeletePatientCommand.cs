using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Providers;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.ShowPatientsFlow.Commands;

public class DeletePatientCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Удалить пациента";
    public async Task<MenuState?> ExecuteAsync()
    {
        var deletePatientProvider = serviceProvider.GetRequiredService<DeletePatientProvider>();
        return await deletePatientProvider.CreateMenuAsync();
    }
}