namespace TFLitePoseTrainer;

sealed class Result
{
    internal static readonly Result Success = new();

    readonly Exception? _exception;

    internal bool HasException => _exception is not null;
    internal Exception Exception => _exception ?? throw new($"Property {nameof(Exception)} is accessible only when {nameof(HasException)} is true");

    Result() { _exception = null; }
    Result(Exception exception) { _exception = exception; }

    public static implicit operator Result(Exception exception) => new(exception);
}

sealed class Result<T>
{
    readonly T _value;
    readonly Exception? _exception;

    internal Exception Exception => _exception ?? throw new($"Property {nameof(Exception)} is accessible only when {nameof(TryGetValue)} returns false");

    Result(T value) { _value = value; _exception = null; }
    Result(Exception exception) { _value = default!; _exception = exception; }

    internal bool TryGetValue(out T value)
    {
        value = _value;
        return _exception is null;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Exception exception) => new(exception);
}
