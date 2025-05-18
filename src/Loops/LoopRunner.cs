using System.Diagnostics;

namespace TFLitePoseTrainer.Loops;

class LoopRunner(Action action) : IDisposable
{
    private readonly Action _action = action;
    private Thread? _thread;
    private volatile bool _isRunning;

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

    public void Start()
    {
        Debug.Assert(_thread is null);
        _thread = new Thread(Run) { IsBackground = true };
        _isRunning = true;
        _thread.Start();
    }

    public void Stop()
    {
        Debug.Assert(_thread is not null);
        _isRunning = false;
        _thread.Join();
        _thread = null;
    }

    private void Run()
    {
        while (_isRunning)
        {
            _action();
        }
    }
}
