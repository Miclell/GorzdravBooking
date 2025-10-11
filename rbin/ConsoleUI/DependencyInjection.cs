using ConsoleUI.Components;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Providers;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow.Providers;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Commands;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Providers;
using ConsoleUI.Menus.Commands;
using ConsoleUI.Menus.Patient.Command;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;
using ConsoleUI.Menus.Patient.Providers;
using ConsoleUI.Menus.Patient.ShowPatientsFlow;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Commands;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Providers;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using ConsoleUI.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using DoctorMenuProvider = ConsoleUI.Menus.Providers.DoctorMenuProvider;
using SelectTimePreferencesCommand = ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands.SelectTimePreferencesCommand;
using SpecialityMenuProvider = ConsoleUI.Menus.Providers.SpecialityMenuProvider;

namespace ConsoleUI;

public static class DependencyInjection
{
    public static IServiceCollection AddMenu(this IServiceCollection services)
    {
        // Base services
        services.AddSingleton<NavigationStack>();
        services.AddSingleton<MenuRenderer>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<IConsoleInputService, ConsoleInputService>();
        
        // Providers
        services.AddTransient<IMenuProvider, MainMenuProvider>();
        
        // Booking Flow
        services.AddTransient<Menus.Providers.DistrictMenuProvider>();
        services.AddTransient<Menus.Patient.CreatePatientFlow.Providers.LpuMenuProvider>();
        services.AddTransient<Menus.Patient.CreatePatientFlow.Providers.DistrictMenuProvider>();
        services.AddTransient<Menus.Providers.LpuMenuProvider>();
        services.AddTransient<SpecialityMenuProvider>();
        services.AddTransient<DoctorMenuProvider>();
        services.AddTransient<AppointmentMenuProvider>();

        services.AddTransient<ShowPatientMenuProvider>();
        services.AddTransient<ShowPatientsProvider>();
        services.AddTransient<CreatePatientProvider>();
        services.AddTransient<PatientSelectionProvider>();

        services.AddTransient<DeletePatientCommand>();
        services.AddTransient<DeletePatientProvider>();

        services.AddTransient<ShowAppointmentMenuCommand>();
        services.AddTransient<ShowAppointmentMenuProvider>();

        services.AddTransient<RunCreateAppointmentFlowCommand>();
        services.AddTransient<RunCreateAppointmentFlowProvider>();
        services.AddTransient<RunShowAppointmentFlowCommand>();
        services.AddTransient<RunShowAppointmentFlowProvider>();
        
        services.AddTransient<ShowActiveAppointmentsCommand>();
        services.AddTransient<ShowActiveAppointmentsProvider>();

        services.AddTransient<ShowUpcomingAppointmentsCommand>();
        services.AddTransient<ShowUpcomingAppointmentsProvider>();

        services.AddTransient<AppointmentCancelProvider>();

        services.AddTransient<SpecialitySelectionProvider>();
        services.AddTransient<Menus.AppointmentMenu.CreateAppointmentFlow.Providers.DoctorMenuProvider>();
        services.AddTransient<SpecialityMenuProvider>();
        services.AddTransient<CreateAppointmentProvider>();

        services.AddTransient<RunTimePreferencesMenuCommand>();
        services.AddTransient<RunTimePreferencesMenuProvider>();

        services.AddTransient<CreateTimePreferencesCommand>();
        services.AddTransient<ShowTimePreferencesCommand>();

        services.AddTransient<CreateTimePreferencesProvider>();
        services.AddTransient<ShowTimePreferencesCommand>();

        services.AddTransient<ShowTimePreferencesProvider>();

        services.AddTransient<Menus.AppointmentMenu.CreateAppointmentFlow.Commands.SelectTimePreferencesCommand>();
        
        services.AddTransient<SelectAppointmentProvider>();

        services.AddTransient<SelectTimePreferencesCommand>();
        services.AddTransient<DeleteAppointmentCommand>();

        services.AddTransient<SelectTimePreferencesProvider>();
        
        // Commands
        services.AddTransient<ShowPatientMenuCommand>();
        services.AddTransient<ShowPatientsCommand>();
        services.AddTransient<RunPatientFlowCommand>();
        
        services.AddTransient<ShowDistrictMenuCommand>();

        services.AddTransient<MainMenuProvider>();
        services.AddTransient<ExitCommand>();
        services.AddTransient<BackCommand>();
        
        return services;
    }
}