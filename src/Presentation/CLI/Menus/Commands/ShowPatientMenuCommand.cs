using Application.DTOs.Patient;
using Application.Services;
using Application.Services.Implementation;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;
using StatefulMenu.Infrastructure.Services;

namespace CLI.Menus.Commands;

public class ShowPatientMenuCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => "Пациенты";

    public async Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        var provider = serviceProvider.GetRequiredService<PatientMenuProvider>();
        return MenuResult.Push(provider.CreateMenuAsync(ct).Result);
    }
}

public class PatientMenuProvider(IServiceProvider serviceProvider, IConsoleInputService input, IDataService data, IPatientRepository patients, PatientService patientService) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        var items = new List<MenuItem>
        {
            new("Показать список", async _ =>
            {
                Console.WriteLine();
                Console.WriteLine("Пациенты:");
                var currentUserId = data.TryGet<Guid>("userId", out var uid) ? uid : Guid.Empty;
                var list = await patients.GetByUserIdAsync(currentUserId, ct);
                foreach (var p in list) Console.WriteLine($"{p.Id} | {p.PatientLastName} {p.PatientFirstName} {p.PatientMiddleName}");
                Console.ReadKey(true);
                return MenuResult.None();
            }),
            new("Создать", async _ =>
            {
                var model = await input.ReadModelAsync<CreatePatientDto>(ct);
                if (model is null) return MenuResult.None();
                var res = await patientService.Create(model, ct);
                Console.WriteLine(res.IsSuccess ? "Создано" : "Ошибка");
                Console.ReadKey(true);
                return MenuResult.None();
            }),
            new("Удалить", async _ =>
            {
                Console.Write("Id пациента: ");
                if (Guid.TryParse(Console.ReadLine(), out var pid))
                {
                    await patients.DeleteAsync(pid, ct);
                    Console.WriteLine("Удалено");
                }
                else Console.WriteLine("Неверный Id");
                Console.ReadKey(true);
                return MenuResult.None();
            }),
            new("Временные предпочтения", _ =>
            {
                var provider = serviceProvider.GetRequiredService<TimePreferencesMenuProvider>();
                return Task.FromResult(MenuResult.Push(provider.CreateMenuAsync(ct).Result));
            }),
            new("Назад", _ => Task.FromResult(MenuResult.Pop()))
        };
        return new MenuState("Пациенты", items);
    }
}


