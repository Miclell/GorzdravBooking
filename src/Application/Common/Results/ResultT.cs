using System.Diagnostics.CodeAnalysis;

namespace Application.Common.Results;

public record Result<TValue> : Result
{
    private readonly TValue _value;

    protected internal Result(TValue value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value =>
        !IsSuccess ? throw new InvalidOperationException("Cannot access value of a failed result") : _value;

    [DoesNotReturn]
    public static implicit operator Result<TValue>(Error error)
    {
        return Failure<TValue>(error);
    }

    public static implicit operator Result<TValue>(TValue value)
    {
        return Success(value);
    }

    public void Deconstruct(out TValue value, out Error error)
    {
        value = _value;
        error = Error;
    }
}