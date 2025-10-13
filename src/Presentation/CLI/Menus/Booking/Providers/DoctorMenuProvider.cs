using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Booking.Providers;

public class DoctorMenuProvider(IDoctorService doctorService, IDataService data, IServiceProvider sp) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        if (!data.TryGet<int>("lpuId", out var lpuId) || !data.TryGet<string>("specialtyId", out var specialtyId))
            return new MenuState("Врачи", new List<MenuItem> { new("Назад", _ => Task.FromResult(MenuResult.Pop())) });

        var doctors = await doctorService.GetBySpecialtyAsync(lpuId, specialtyId!);
        var items = doctors
            .Select(d => new MenuItem(d.Name, _ =>
            {
                data.Set("doctorId", d.Id);
                var next = sp.GetRequiredService<AppointmentSlotsProvider>();
                return Task.FromResult(MenuResult.Push(next.CreateMenuAsync(ct).Result));
            }))
            .Append(new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop())))
            .ToList();

        return new MenuState("Выбор врача", items);
    }
}


