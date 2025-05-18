using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

using TFLitePoseTrainer.Extensions;

namespace TFLitePoseTrainer.Loops;

class TrackingLoop : IDisposable
{
    public static readonly TrackerConfiguration DefaultTrackerConfig = TrackerConfiguration.Default;
    public static readonly int Timeout = 500;

    private readonly LoopRunner _loopRunner;
    private readonly Tracker _tracker;

    private volatile bool _isStarting;
    private volatile bool _isStopping;
    private volatile bool _willStart;
    private volatile bool _willStop;

    public static TrackingLoop? Create(Param param)
    {
        try
        {
            return new(param);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to create TrackingLoop: {e}");
            return null;
        }
    }

    private TrackingLoop(Param param)
    {
        _loopRunner = new(LoopAction);
        _tracker = new(param.Calibration, param.TrackerConfig);
    }

    public void Dispose()
    {
        _loopRunner.Dispose();
        _tracker.Dispose();
    }

    public void Start()
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
            Console.Error.WriteLine($"Failed to start cameras: {e}");
        }

        _isStarting = false;

        if (_willStop)
        {
            _willStop = false;
            Stop();
        }
    }

    public void Stop()
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
            Console.Error.WriteLine($"Failed to stop cameras: {e}");
        }

        _isStopping = false;

        if (_willStart)
        {
            _willStart = false;
            Start();
        }
    }

    public void Enqueue(Capture capture)
    {
        try
        {
            _tracker.EnqueueCapture(capture);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to enqueue capture: {e}");
        }
    }

    private void LoopAction()
    {
        BodyFrame? bodyFrame = null;

        try
        {
            _tracker.TryPopResult(out bodyFrame, Timeout);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to get body frame: {e}");
        }

        if (bodyFrame is null)
        {
            return;
        }

        BodyFrameReady?.Invoke(bodyFrame);
        bodyFrame.Dispose();
    }

    public event Action<BodyFrame>? BodyFrameReady;

    public record Param(Calibration Calibration)
    {
        public TrackerConfiguration TrackerConfig { get; init; } = DefaultTrackerConfig;
    }
}
