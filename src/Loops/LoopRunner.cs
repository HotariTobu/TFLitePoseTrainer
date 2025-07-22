namespace TFLitePoseTrainer.Loops;

class LoopRunner(Action action) : IDisposable
{
    Thread? _thread;
    volatile bool _isRunning;

    public void Dispose()
    {
        if (_thread is null)
        {
            return;
        }

        if (_isRunning)
        {
            Stop();
        }
    }

    internal void Start()
    {
        if (_thread is not null || _isRunning)
        {
            return;
        }

        _thread = new Thread(Run) { IsBackground = true };
        _isRunning = true;
        _thread.Start();
    }

    internal void Stop()
    {
        if (_thread is null || !_isRunning)
        {
            return;
        }

        _isRunning = false;
        _thread.Join();
        _thread = null;
    }

    void Run()
    {
        while (_isRunning)
        {
            action();
        }
    }
}
