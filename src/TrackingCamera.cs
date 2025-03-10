using K4AdotNet.BodyTracking;

namespace TFLitePoseTrainer;

internal class TrackingCamera(int index) : Camera(index)
{
    public readonly TrackerConfiguration TrackerConfig = TrackerConfiguration.Default;

    private Tracker? _tracker;

    private BodyFrame? _lastBodyFrame;

    public BodyFrame? LastBodyFrame
    {
        get => _lastBodyFrame;
        private set
        {
            _lastBodyFrame?.Dispose();
            _lastBodyFrame = value;
        }
    }

    public override void Dispose()
    {
        LastBodyFrame = null;
        base.Dispose();
    }

    public override void Start()
    {
        base.Start();

        if (_tracker is not null)
        {
            return;
        }

        _device.GetCalibration(DeviceConfig.DepthMode, DeviceConfig.ColorResolution, out var calibration);
        _tracker = new(calibration, TrackerConfig);
    }

    public override void Stop()
    {
        base.Stop();

        if (_tracker is null)
        {
            return;
        }

        _tracker.Shutdown();
        _tracker.Dispose();
        _tracker = null;
    }

    public override async Task<bool> Update()
    {
        await base.Update();

        if (_tracker is null || LastCapture is null)
        {
            return false;
        }

        try
        {
            await Task.Run(() => _tracker.EnqueueCapture(LastCapture));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to enqueue capture: {e}");
            return false;
        }

        try
        {
            LastBodyFrame = await Task.Run(_tracker.PopResult);

        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to pop result: {e}");
            return false;
        }

        return true;
    }
}
