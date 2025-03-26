using K4AdotNet.BodyTracking;

namespace TFLitePoseTrainer.Cameras;

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

    public override bool Start()
    {
        if (!base.Start())
        {
            return false;
        }

        if (_tracker is not null)
        {
            return false;
        }

        try
        {
            _device.GetCalibration(DeviceConfig.DepthMode, DeviceConfig.ColorResolution, out var calibration);
            _tracker = new(calibration, TrackerConfig);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to create tracker: {e}");
            return false;
        }

        return true;
    }

    public override bool Stop()
    {
        if (!base.Stop())
        {
            return false;
        }

        if (_tracker is null)
        {
            return false;
        }

        try
        {
            _tracker.Shutdown();
            _tracker.Dispose();
            _tracker = null;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to shutdown tracker: {e}");
            return false;
        }

        return true;
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
