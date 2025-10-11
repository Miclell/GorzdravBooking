using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow.Commands;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow.Providers;

public class AppointmentSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var appointment = navigation.GetSharedData<Appointment>(nameof(Appointment));

        return new MenuState()
        {
            Title = "Отменить выбранную запись",
            Commands = [navigation.CreateCommand<AppointmentSelectionCommand>()]
        };
    }
}