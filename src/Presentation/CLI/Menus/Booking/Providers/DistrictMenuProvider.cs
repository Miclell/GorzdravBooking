using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Booking.Providers;

public class DistrictMenuProvider(IExternalDistrictService externalDistrictService, IDataService data, IServiceProvider sp) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        var districts = await externalDistrictService.GetDistrictsAsync();
        var items = districts
            .Select(d => new MenuItem(d.Name, _ =>
            {
                data.Set("districtId", d.Id);
                var next = sp.GetRequiredService<LpuMenuProvider>();
                return Task.FromResult(MenuResult.Push(next.CreateMenuAsync(ct).Result));
            }))
            .Append(new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop())))
            .ToList();

        return new MenuState("Районы", items);
    }
}


