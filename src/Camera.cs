using K4AdotNet.Sensor;

namespace TFLitePoseTrainer;

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
        }
    }

    public virtual void Dispose()
    {
        LastCapture = null;
        _device.Dispose();
    }

    public virtual void Start()
    {
        if (IsStarted)
        {
            return;
        }
        IsStarted = true;

        _device.StartCameras(DeviceConfig);
    }

    public virtual void Stop()
    {
        if (IsStopped)
        {
            return;
        }
        IsStopped = true;

        _device.StopCameras();
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
