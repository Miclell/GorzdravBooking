namespace StatefulMenu.Core.Interfaces;

public interface IConsoleInputService
{
    Task<T?> ReadModelAsync<T>(CancellationToken ct = default);
}