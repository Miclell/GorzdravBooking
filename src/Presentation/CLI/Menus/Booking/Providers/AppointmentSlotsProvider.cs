using Core.Interfaces.Services;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Booking.Providers;

public class AppointmentSlotsProvider(IExternalAppointmentService externalAppointmentService, IDataService data) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        if (!data.TryGet<int>("lpuId", out var lpuId) || !data.TryGet<string>("doctorId", out var doctorId))
            return new MenuState("Слоты", new List<MenuItem> { new("Назад", _ => Task.FromResult(MenuResult.Pop())) });

        var slots = await externalAppointmentService.GetByDoctorAsync(lpuId, doctorId!);
        var items = slots
            .Select(s => new MenuItem($"{s.VisitStart:g} — {s.VisitEnd:t}", _ =>
            {
                data.Set("appointmentId", s.Id);
                return Task.FromResult(MenuResult.Push(new MenuState("Подтверждение", new List<MenuItem>
                {
                    new("Подтвердить", _ => Task.FromResult(MenuResult.Replace(new MenuState("Готово", new List<MenuItem>
                    {
                        new("Назад", _ => Task.FromResult(MenuResult.Pop()))
                    }))))
                })));
            }))
            .Append(new MenuItem("Назад", _ => new BackCommand().ExecuteAsync(ct)))
            .ToList();

        return new MenuState("Доступные слоты", items);
    }
}


