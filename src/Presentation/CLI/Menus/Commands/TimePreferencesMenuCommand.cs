using Application.DTOs.TimePreferences;
using Application.Services;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;
using StatefulMenu.Infrastructure.Services;

namespace CLI.Menus.Commands;

public class TimePreferencesMenuCommand(Func<TimePreferencesMenuProvider> providerFactory) : IMenuCommand
{
    public string Title => "Временные предпочтения";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        var provider = providerFactory();
        return Task.FromResult(MenuResult.Push(provider.CreateMenuAsync(ct).Result));
    }
}

public class TimePreferencesMenuProvider(IConsoleInputService input, IDataService data, TimePreferencesService service) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        var items = new List<MenuItem>
        {
            new("Создать пресет", async _ =>
            {
                var model = await input.ReadModelAsync<CreateTimePreferenceDto>(ct);
                if (model is null) return MenuResult.None();
                var res = await service.CreateRangeAsync(new[] { model }, ct);
                Console.WriteLine(res.IsSuccess ? "Создано" : "Ошибка");
                Console.ReadKey(true);
                return MenuResult.None();
            }),
            new("Удалить пресет", async _ =>
            {
                var model = await input.ReadModelAsync<DeleteTimePreferencesDto>(ct);
                if (model is null) return MenuResult.None();
                var res = await service.DeleteByPresetAsync(model, ct);
                Console.WriteLine(res.IsSuccess ? "Удалено" : "Ошибка");
                Console.ReadKey(true);
                return MenuResult.None();
            }),
            new("Назад", _ => Task.FromResult(MenuResult.Pop()))
        };
        return Task.FromResult(new MenuState("Временные предпочтения", items));
    }
}


