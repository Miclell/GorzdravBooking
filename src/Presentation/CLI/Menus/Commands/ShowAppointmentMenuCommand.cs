using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Commands;

public class ShowAppointmentMenuCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => "Запись на прием";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        var provider = serviceProvider.GetRequiredService<Booking.Providers.DistrictMenuProvider>();
        return Task.FromResult(MenuResult.Push(provider.CreateMenuAsync(ct).Result));
    }
}


