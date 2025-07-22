using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Loops;

class CaptureLoop : IDisposable
{
    internal static readonly int DefaultDeviceIndex = 0;
    internal static readonly int Timeout = 500;

    readonly LoopRunner _loopRunner;
    readonly Device _device;

    volatile bool _isStarting;
    volatile bool _isStopping;
    volatile bool _willStart;
    volatile bool _willStop;

    internal DeviceConfiguration DeviceConfig { get; }
    internal Calibration Calibration { get; }

    internal static Task<Result<CaptureLoop>> Create(Param param) => Task.Run<Result<CaptureLoop>>(() =>
    {
        try
        {
            return new CaptureLoop(param);
        }
        catch (Exception e)
        {
            return e;
        }
    });

    CaptureLoop(Param param)
    {
        _loopRunner = new(LoopAction);
        _device = Device.Open(param.DeviceIndex);
        DeviceConfig = param.DeviceConfig;

        _device.GetCalibration(DeviceConfig.DepthMode, DeviceConfig.ColorResolution, out var calibration);
        Calibration = calibration;
    }

    public void Dispose()
    {
        _loopRunner.Dispose();
        _device.Dispose();
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
            _device.StartCameras(DeviceConfig);
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
            _device.StopCameras();
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

    void LoopAction()
    {
        Capture? capture = null;

        try
        {
            _device.TryGetCapture(out capture, Timeout);
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(new("Failed to get capture", e));
        }

        if (capture is null)
        {
            return;
        }

        CaptureReady?.Invoke(capture);
        capture.Dispose();
    }

    internal event Action<Capture>? CaptureReady;
    internal event Action<Exception>? ErrorOccurred;

    internal record Param(DeviceConfiguration DeviceConfig)
    {
        internal int DeviceIndex { get; init; } = DefaultDeviceIndex;
    }
}
