using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Cameras;

internal class Camera(int index) : IDisposable
{
    public readonly DeviceConfiguration DeviceConfig = new()
    {
        CameraFps = FrameRate.Thirty,
        DepthMode = DepthMode.NarrowViewUnbinned,
        ColorResolution = ColorResolution.R720p,
        ColorFormat = ImageFormat.ColorBgra32,
        SynchronizedImagesOnly = true,
    };

    protected readonly Device _device = Device.Open(index);

    private Capture? _lastCapture;

    public static int DeviceCount => Device.InstalledCount;

    public bool IsStarted { get; private set; } = false;
    public bool IsStopped { get => !IsStarted; private set => IsStarted = !value; }

    public Capture? LastCapture
    {
        get => _lastCapture;
        private set
        {
            _lastCapture?.Dispose();
            _lastCapture = value;

            if (_lastCapture is not null)
            {
                _lastCapture.Disposed += (s, e) => _lastCapture = null;
            }
        }
    }

    public virtual void Dispose()
    {
        LastCapture = null;
        _device.Dispose();
    }

    public virtual bool Start()
    {
        if (IsStarted)
        {
            return false;
        }

        try
        {
            _device.StartCameras(DeviceConfig);
            IsStarted = true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to start cameras: {e}");
            return false;
        }

        return true;
    }

    public virtual bool Stop()
    {
        if (IsStopped)
        {
            return false;
        }

        try
        {
            _device.StopCameras();
            IsStopped = true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to stop cameras: {e}");
            return false;
        }

        return true;
    }


    public virtual async Task<bool> Update()
    {
        try
        {
            LastCapture = await Task.Run(_device.GetCapture);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to get capture: {e}");
            return false;
        }

        return true;
    }
}
