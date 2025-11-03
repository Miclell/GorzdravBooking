using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu;

public class ShowAppointmentMenuCommand(ShowAppointmentMenuProvider showAppointmentMenuProvider) : IMenuCommand
{
    public string Title { get; } = "Меню записи";
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        MenuResult.Push(await showAppointmentMenuProvider.CreateMenuAsync(cancellationToken));
}