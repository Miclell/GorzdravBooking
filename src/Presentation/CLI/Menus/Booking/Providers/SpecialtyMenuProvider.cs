using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Booking.Providers;

public class SpecialtyMenuProvider(ISpecialtyService specialtyService, IDataService data, IServiceProvider sp) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        if (!data.TryGet<int>("lpuId", out var lpuId))
            return new MenuState("Специальности", new List<MenuItem> { new("Назад", _ => Task.FromResult(MenuResult.Pop())) });

        var specs = await specialtyService.GetByLpuAsync(lpuId);
        var items = specs
            .Select(s => new MenuItem(s.Name, _ =>
            {
                data.Set("specialtyId", s.Id);
                var next = sp.GetRequiredService<DoctorMenuProvider>();
                return Task.FromResult(MenuResult.Push(next.CreateMenuAsync(ct).Result));
            }))
            .Append(new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop())))
            .ToList();

        return new MenuState("Специальности", items);
    }
}


