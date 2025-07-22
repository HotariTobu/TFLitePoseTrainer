using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Loops;

class TrackingLoop : IDisposable
{
    internal static readonly TrackerConfiguration DefaultTrackerConfig = TrackerConfiguration.Default;
    internal static readonly int Timeout = 500;

    readonly LoopRunner _loopRunner;
    readonly Tracker _tracker;

    volatile bool _isStarting;
    volatile bool _isStopping;
    volatile bool _willStart;
    volatile bool _willStop;

    internal static Result<TrackingLoop> Create(Param param)
    {
        try
        {
            return new TrackingLoop(param);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    TrackingLoop(Param param)
    {
        _loopRunner = new(LoopAction);
        _tracker = new(param.Calibration, param.TrackerConfig);
    }

    public void Dispose()
    {
        _loopRunner.Dispose();
        _tracker.Dispose();
    }

    internal void Start()
    {
        if (_isStarting)
        {
            _willStop = false;
            return;
        }

        if (_isStopping)
        {
            _willStart = true;
            return;
        }

        _isStarting = true;

        try
        {
            _loopRunner.Start();
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(new("Failed to start cameras", e));
        }

        _isStarting = false;

        if (_willStop)
        {
            _willStop = false;
            Stop();
        }
    }

    internal void Stop()
    {
        if (_isStopping)
        {
            _willStart = false;
            return;
        }

        if (_isStarting)
        {
            _willStop = true;
            return;
        }

        _isStopping = true;

        try
        {
            _loopRunner.Stop();
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(new("Failed to stop cameras", e));
        }

        _isStopping = false;

        if (_willStart)
        {
            _willStart = false;
            Start();
        }
    }

    internal void Enqueue(Capture capture)
    {
        try
        {
            _tracker.EnqueueCapture(capture);
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(new("Failed to enqueue capture", e));
        }
    }

    void LoopAction()
    {
        BodyFrame? bodyFrame = null;

        try
        {
            _tracker.TryPopResult(out bodyFrame, Timeout);
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(new("Failed to get body frame", e));
        }

        if (bodyFrame is null)
        {
            return;
        }

        BodyFrameReady?.Invoke(bodyFrame);
        bodyFrame.Dispose();
    }

    internal event Action<BodyFrame>? BodyFrameReady;
    internal event Action<Exception>? ErrorOccurred;

    internal record Param(Calibration Calibration)
    {
        internal TrackerConfiguration TrackerConfig { get; init; } = DefaultTrackerConfig;
    }
}
