using K4AdotNet.Sensor;

using TFLitePoseTrainer.Extensions;

namespace TFLitePoseTrainer.Loops;

class CaptureLoop : IDisposable
{
    public static readonly int DefaultDeviceIndex = 0;
    public static readonly int Timeout = 500;

    private readonly LoopRunner _loopRunner;
    private readonly Device _device;
    private readonly DeviceConfiguration _deviceConfig;

    private volatile bool _isStarting;
    private volatile bool _isStopping;
    private volatile bool _willStart;
    private volatile bool _willStop;

    public static CaptureLoop? Create(Param param)
    {
        try
        {
            return new(param);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to create CaptureLoop: {e}");
            return null;
        }
    }

    private CaptureLoop(Param param)
    {
        _loopRunner = new(LoopAction);
        _device = Device.Open(param.DeviceIndex);
        _deviceConfig = param.DeviceConfig;
    }

    public Calibration? GetCalibration()
    {
        try
        {
            _device.GetCalibration(_deviceConfig.DepthMode, _deviceConfig.ColorResolution, out var calibration);
            return calibration;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to get calibration: {e}");
            return null;
        }
    }

    public void Dispose()
    {
        _loopRunner.Dispose();
        _device.Dispose();
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
            _device.StartCameras(_deviceConfig);
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
            _device.StopCameras();
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

    private void LoopAction()
    {
        Capture? capture = null;

        try
        {
            _device.TryGetCapture(out capture, Timeout);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to get capture: {e}");
        }

        if (capture is null)
        {
            return;
        }

        CaptureReady?.Invoke(capture);
        capture.Dispose();
    }

    public event Action<Capture>? CaptureReady;

    public record Param(DeviceConfiguration DeviceConfig)
    {
        public int DeviceIndex { get; init; } = DefaultDeviceIndex;
    }
}
