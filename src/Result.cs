namespace TFLitePoseTrainer;

sealed class Result
{
    readonly Exception? _exception;

    public bool HasException => _exception is not null;

    Result(Exception? exception) { _exception = exception; }

    public Exception GetException() => _exception ?? throw new($"Method {nameof(GetException)} is callable only when {nameof(HasException)} is true");

    public static implicit operator Result(Exception? exception) => new(exception);
}

sealed class Result<T>
{
    readonly T _value;
    readonly Exception? _exception;

    Result(T value) { _value = value; _exception = null; }
    Result(Exception exception) { _value = default!; _exception = exception; }

    public bool TryGetValue(out T value)
    {
        value = _value;
        return _exception is null;
    }

    public Exception GetException() => _exception ?? throw new($"Method {nameof(GetException)} is callable only when {nameof(TryGetValue)} returns false");

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Exception exception) => new(exception);
}
