namespace ConsoleUI.Services.Interfaces;

public interface IConsoleInputService
{
    Task<T?> ReadModelAsync<T>();
    object? ReadModel(Type modelType);
    void RegisterConverter<T>(Func<string, T> converter);
}