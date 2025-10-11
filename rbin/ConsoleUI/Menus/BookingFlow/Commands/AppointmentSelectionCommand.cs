using System.Globalization;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Commands;

public class AppointmentSelectionCommand(Appointment appointment, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => appointment.VisitStart.ToString(CultureInfo.CurrentCulture);
    public Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(Appointment), appointment);

        return null;
    }
}