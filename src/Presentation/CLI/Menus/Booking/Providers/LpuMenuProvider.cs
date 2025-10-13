using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Booking.Providers;

public class LpuMenuProvider(ILpuService lpuService, IDataService data, IServiceProvider sp) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        if (!data.TryGet<string>("districtId", out var districtId))
            return new MenuState("ЛПУ", new List<MenuItem> { new("Назад", _ => Task.FromResult(MenuResult.Pop())) });

        var lpus = await lpuService.GetByDistrictAsync(districtId!);
        var items = lpus
            .Select(lpu => new MenuItem(lpu.LpuShortName, _ =>
            {
                data.Set("lpuId", lpu.Id);
                var next = sp.GetRequiredService<SpecialtyMenuProvider>();
                return Task.FromResult(MenuResult.Push(next.CreateMenuAsync(ct).Result));
            }))
            .Append(new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop())))
            .ToList();

        return new MenuState("Выбор ЛПУ", items);
    }
}


