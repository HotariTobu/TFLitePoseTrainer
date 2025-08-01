namespace TFLitePoseTrainer;

/// <summary>
/// A simple disposable class that allows you to defer actions until the object is disposed.
/// </summary>
/// <example>
/// using var defer = new Defer();
///
/// isStarted = true;
/// defer.Disposed += () => isStarted = false;
/// </example>
sealed class Defer : IDisposable
{
    public void Dispose()
    {
        Disposed?.Invoke();
        Disposed = null;
    }

    internal event Action? Disposed;
}
