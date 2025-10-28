# StatefulMenu

![NuGet](https://img.shields.io/nuget/v/StatefulMenu.svg)
![Tests](https://github.com/Miclell/StatefulMenu/actions/workflows/dotnet.yml/badge.svg)

Минималистичная библиотека для построения «состоящих из экранов» консольных меню на .NET с навигацией по стеку, хоткеями, локализацией и безопасным вводом данных.

## Возможности
- ✅ Интерактивные меню и навигация по стеку экранов
- ✅ Хоткеи, стрелки, выбор по цифрам, Esc для «назад»
- ✅ Ввод моделей из консоли: обязательные/необязательные поля, `nullable`
- ✅ Валидация: RegEx и пользовательские валидаторы/конвертеры
- ✅ Локализация (ru/en) из коробки
- ✅ DI-расширение и авто‑регистрация команд/провайдеров меню

## Установка
```bash
dotnet add package StatefulMenu
```

## Быстрый старт
1) Подключите сервисы в DI и запустите навигацию с корневого меню.

```csharp
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

var services = new ServiceCollection()
    // Сканирует текущее сборочное дерево и регистрирует IMenuProvider/IMenuCommand
    .AddStatefulMenu();

var provider = services.BuildServiceProvider();
var nav = provider.GetRequiredService<INavigationService>();

// Корневой провайдер меню (см. пример ниже)
var root = provider.GetRequiredService<IMenuProvider>();

await nav.RunAsync(root);
```

2) Реализуйте `IMenuProvider` и верните `MenuState` с пунктами `MenuItem`.

```csharp
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

public class HomeMenuProvider : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        var items = new[]
        {
            new MenuItem("Сказать привет", async _ =>
            {
                Console.WriteLine("Привет!");
                return MenuResult.None();
            }, hotkey: ConsoleKey.H),

            new MenuItem("Подменю", async _ =>
            {
                var sub = new MenuState("Подменю", new[]
                {
                    new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop()))
                });
                return MenuResult.Push(sub);
            }) ,

            new MenuItem("Выход", _ => Task.FromResult(MenuResult.Exit()), hotkey: ConsoleKey.E)
        };

        return Task.FromResult(new MenuState("Главное меню", items));
    }
}
```

Навигация управляется значениями `MenuResult`:
- `MenuResult.None()` — остаться на экране
- `MenuResult.Push(state)` — открыть новый экран
- `MenuResult.Replace(state)` — заменить текущий
- `MenuResult.Pop(count)` — вернуться назад на `count`
- `MenuResult.Exit()` — завершить цикл

## Управление с клавиатуры
- **Enter/цифра (1..N)**: выбрать пункт
- **↑/↓**: перемещение по пунктам
- **Esc**: назад (`Pop`)
- **Хоткеи**: если у `MenuItem` задан `ConsoleKey`, нажатие сразу выполнит его действие

## Ввод данных из консоли
Сервис `IConsoleInputService` позволяет запрашивать модель, помечая свойства атрибутом `InputField`.

```csharp
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;

public record CreateUserModel(
    [property: InputField("Email", Pattern = @"^[^@]+@[^@]+\\.[^@]+$", ErrorMessage = "Некорректный email")]
    string Email,

    [property: InputField("Возраст", IsRequired = false)]
    int? Age,

    [property: InputField("Активен")] 
    bool IsActive
);

// Где-то в обработчике меню
var input = provider.GetRequiredService<IConsoleInputService>();
var model = await input.ReadModelAsync<CreateUserModel>();
```

Поддерживается:
- **Обязательные/необязательные поля** (`IsRequired`)
- **Nullable-типы**
- **RegEx** (`Pattern` + `ErrorMessage`)
- **Пользовательские валидаторы/конвертеры** через `Validators`/`Converters` (типы с публичным методом `bool Validate(string input, out string? error)` и конвертеры, возвращающие значение/ошибку)
- **Enum**-поля (с подсказкой допустимых значений)

Локализация сообщений (ru/en) выбирается автоматически по `CultureInfo.CurrentCulture`.

## Общие сервисы
- `INavigationService` — управление стеком экранов, события `Navigating`/`Navigated`
- `IDataService` — простой ключ/значение стор для обмена данными между экранами
- `MenuRenderer` — отрисовка текущего экрана в консоль

## DI и авто‑регистрация
`services.AddStatefulMenu(params Assembly[] scanAssemblies)`:
- Регистрирует локализатор, рендерер, ввод, навигацию и стор
- Сканирует указанные сборки (или вызывающую по умолчанию) на реализации `IMenuProvider` и `IMenuCommand` и регистрирует их

Чтобы контролировать сборки для сканирования:
```csharp
services.AddStatefulMenu(typeof(HomeMenuProvider).Assembly);
```

## Советы по структуре
- Держите каждый экран как `IMenuProvider`
- Возвращайте `MenuResult` из действий пунктов меню
- Для сложных сценариев храните данные в `IDataService`

## Требования
- .NET 8.0+

## Лицензия
MIT — см. `LICENSE`.

# Как правильно использовать StatefulMenu

## Проблемы, которые были исправлены

### 1. Баг с вводом цифр
**Проблема:** При вводе цифры сразу выполнялось действие, а не просто выбирался пункт.

**Исправление:** Теперь цифры только выбирают пункт, а для выполнения нужно нажать Enter.

### 2. Сложная архитектура
**Проблема:** Непонятная система с `IMenuCommand` и `IMenuProvider` была слишком сложной для простых случаев.

**Решение:** Используйте прямой подход через `IMenuProvider` с созданием `MenuItem` прямо в коде.

## Правильный способ создания меню

### Простой подход (рекомендуется)

```csharp
public class MyMenuProvider : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        var items = new[]
        {
            // Простое действие
            new MenuItem("Сказать привет", async _ =>
            {
                Console.WriteLine("Привет!");
                Console.ReadKey(true);
                return MenuResult.None(); // Остаемся на экране
            }, hotkey: ConsoleKey.H), // Хоткей H

            // Открытие подменю
            new MenuItem("Подменю", async _ =>
            {
                var submenu = new MenuState("Подменю", new[]
                {
                    new MenuItem("Действие 1", async _ =>
                    {
                        Console.WriteLine("Действие 1");
                        return MenuResult.None();
                    }),
                    new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop()))
                });
                return MenuResult.Push(submenu); // Открываем новое меню
            }),

            // Выход
            new MenuItem("Выход", _ => Task.FromResult(MenuResult.Exit()), hotkey: ConsoleKey.E)
        };

        return Task.FromResult(new MenuState("Главное меню", items));
    }
}
```

### Запуск приложения

```csharp
var services = new ServiceCollection()
    .AddStatefulMenu(typeof(MyMenuProvider).Assembly);

var provider = services.BuildServiceProvider();
var nav = provider.GetRequiredService<INavigationService>();
var root = provider.GetRequiredService<MyMenuProvider>();

await nav.RunAsync(root);
```

## Управление

- **Цифры 1-9:** выбрать пункт (НЕ выполнять)
- **Enter:** выполнить выбранное действие
- **Стрелки ↑↓:** навигация по пунктам
- **Esc:** назад (Pop)
- **Хоткеи:** если у пункта есть хоткей, нажатие сразу выполнит действие

## Типы результатов (MenuResult)

- `MenuResult.None()` - остаться на текущем экране
- `MenuResult.Push(state)` - открыть новый экран
- `MenuResult.Replace(state)` - заменить текущий экран
- `MenuResult.Pop(count)` - вернуться назад на count экранов
- `MenuResult.Exit()` - выйти из приложения

## Динамические меню

Для создания меню с динамическими данными:

```csharp
public Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
{
    var animals = new[] { "Кот", "Собака", "Птица" };
    
    var items = animals
        .Select(animal => new MenuItem($"Выбрать {animal}", async _ =>
        {
            Console.WriteLine($"Вы выбрали: {animal}");
            return MenuResult.None();
        }))
        .Append(new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop())))
        .ToArray();

    return Task.FromResult(new MenuState("Животные", items));
}
```

## Ввод данных

Для ввода данных используйте `IConsoleInputService`:

```csharp
public class MyCommand : IMenuCommand
{
    private readonly IConsoleInputService _inputService;
    
    public async Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        var model = await _inputService.ReadModelAsync<MyModel>();
        // Обработка модели...
        return MenuResult.None();
    }
}
```

С атрибутами на модели:

```csharp
public record MyModel(
    [property: InputField("Имя", IsRequired = true)] string Name,
    [property: InputField("Возраст", IsRequired = false)] int? Age
);
```


# SimpleMenuDemo — добавление команд и динамических пунктов

## Структура
- `Program.cs` — DI и запуск навигации
- `Providers/HomeMenuProvider.cs` — корневой поставщик экрана (`IMenuProvider` создаёт `MenuState`)
- `Commands/*.cs` — команды (`IMenuCommand`), отображаются как пункты меню

`AddStatefulMenu(...)` автоматически сканирует сборку и регистрирует все `IMenuProvider` и `IMenuCommand`.

## Добавление команды
Создайте класс в `Commands`, реализующий `IMenuCommand` — он автоматически появится в меню (если провайдер собирает команды из DI):

```csharp
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace SimpleMenuDemo.Commands;

public class MyFeatureCommand : IMenuCommand
{
    public string Title => "Моя команда";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        Console.WriteLine();
        Console.WriteLine("Выполняю мою команду...");
        Console.ReadKey(true);
        return Task.FromResult(MenuResult.None());
    }
}
```

`HomeMenuProvider` превращает команды в `MenuItem`:

```csharp
var items = _commands
    .Select(cmd => new MenuItem(cmd.Title, _ => cmd.ExecuteAsync(ct)))
    .ToList();
return new MenuState("Главное меню (через команды)", items);
```

## Динамические списки (например, животные)
Два способа:

### А) Через отдельные командные классы
Аналогично `animals.Select(a => new SelectAnimalCommand(a))`:

```csharp
public class SelectAnimalCommand : IMenuCommand
{
    private readonly string _animal;
    public SelectAnimalCommand(string animal) => _animal = animal;
    public string Title => _animal;
    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        Console.WriteLine($"Вы выбрали: {_animal}");
        Console.ReadKey(true);
        return Task.FromResult(MenuResult.None());
    }
}
```

Регистрация статического списка:
```csharp
services.AddTransient<IMenuCommand>(_ => new SelectAnimalCommand("Птица"));
services.AddTransient<IMenuCommand>(_ => new SelectAnimalCommand("Рыба"));
services.AddTransient<IMenuCommand>(_ => new SelectAnimalCommand("Лев"));
```

Для реально динамических данных (БД/файл) удобнее собирать пункты в провайдере.

### Б) Прямо в провайдере без классов-команд

```csharp
public class AnimalsMenuProvider : IMenuProvider
{
    private readonly IEnumerable<string> _animals;
    public AnimalsMenuProvider(IEnumerable<string> animals) => _animals = animals;

    public Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        var items = _animals
            .Select(animal => new MenuItem(
                title: animal,
                action: _ =>
                {
                    Console.WriteLine($"Вы выбрали: {animal}");
                    Console.ReadKey(true);
                    return Task.FromResult(MenuResult.None());
                }))
            .Append(new MenuItem("Назад", _ => Task.FromResult(MenuResult.Pop())))
            .ToList();

        return Task.FromResult(new MenuState("Животные", items));
    }
}
```

Открывайте этот экран через команду верхнего уровня, возвращающую `MenuResult.Push(state)`.

## Сложный пример (через команды)

Покрывает сценарии:
- Динамический список с переходом в подменю: `OpenProductsCommand`
- Replace текущего экрана: `ReplaceWithInfoCenterCommand`
- Форма ввода с `InputField` и `IDataService`: `OpenUserFormCommand` + `UserForm`
- Хоткеи и стандартные команды: `SayHelloCommand`, `ExitCommand`, `SubmenuCommand`

Запуск:
1. Соберите проект демо
2. В `Program.cs` выберите пункт `2` — «Сложный пример с командами»

Архитектура:
- Команды (`Commands/*`) реализуют `IMenuCommand` и инкапсулируют логику действий
- Провайдер (`Providers/HomeMenuProvider.cs`) собирает команды из DI и строит `MenuState`
- Навигация управляется через возвращаемые `MenuResult` (`Push`, `Replace`, `Pop`, `Exit`)