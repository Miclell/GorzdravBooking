using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu;

public class ShowAppointmentMenuCommand : IMenuCommand
{
    public string Title { get; } = "Создание и просмотр записей";
    public Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}