using System;

using K4AdotNet.Sensor;

using UnityEngine;

#nullable enable

public class CaptureProvider : MonoBehaviour
{
    [SerializeField] int _deviceIndex = 0;

    [SerializeField] DeviceConfiguration _deviceConfig = new()
    {
        CameraFps = FrameRate.Thirty,
        DepthMode = DepthMode.NarrowViewUnbinned,
        ColorResolution = ColorResolution.Off,
    };

    public Calibration? Calibration { get; private set; }

    public event Action<Capture>? CaptureReady;

    private Device? _device;

    void Awake()
    {
        _device = Device.Open(_deviceIndex);
        Calibration = _device.GetCalibration(_deviceConfig.DepthMode, _deviceConfig.ColorResolution);
    }

    void OnEnable()
    {
        _device?.StartCameras(_deviceConfig);
    }

    void Update()
    {
        if (_device is null)
        {
            return;
        }

        var deltaTime = Time.deltaTime * 1000;
        var timeout = new K4AdotNet.Timeout((int)deltaTime);

        if (_device.TryGetCapture(out var capture, timeout))
        {
            using (capture)
            {
                CaptureReady?.Invoke(capture);
            }
        }
    }

    void OnDisable()
    {
        _device?.StopCameras();
    }

    void OnDestroy()
    {
        _device?.Dispose();
        _device = null;
    }
}
