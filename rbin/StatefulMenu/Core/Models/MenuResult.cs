namespace StatefulMenu.Core.Models;

public abstract record MenuResult
{
    public static MenuResult None()
    {
        return new NoneResult();
    }

    public static MenuResult Push(MenuState state)
    {
        return new PushResult(state);
    }

    public static MenuResult Replace(MenuState state)
    {
        return new ReplaceResult(state);
    }

    public static MenuResult Pop(int count = 1)
    {
        return new PopResult(count);
    }

    public static MenuResult Exit()
    {
        return new ExitResult();
    }
}

public sealed record NoneResult : MenuResult;

public sealed record ExitResult : MenuResult;

public sealed record PopResult(int Count) : MenuResult;

public sealed record PushResult(MenuState State) : MenuResult;

public sealed record ReplaceResult(MenuState State) : MenuResult;