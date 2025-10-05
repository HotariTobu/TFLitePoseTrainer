using K4AdotNet.Sensor;

using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace Assets.Main.Tracking
{
    public class CaptureProvider : MonoBehaviour
    {
        [SerializeField] protected int _deviceIndex = 0;

        [SerializeField]
        protected DeviceConfiguration _deviceConfig = new()
        {
            CameraFps = FrameRate.Thirty,
            DepthMode = DepthMode.NarrowViewUnbinned,
            ColorResolution = ColorResolution.Off,
        };

        [SerializeField] protected UnityEvent<Calibration> _onCalibrationReady = default!;
        [SerializeField] protected UnityEvent<Capture> _onCaptureReady = default!;

        private Device? _device;

        void Awake()
        {
            _device = Device.Open(_deviceIndex);

            var calibration = _device.GetCalibration(_deviceConfig.DepthMode, _deviceConfig.ColorResolution);
            _onCalibrationReady.Invoke(calibration);
        }

        void OnEnable()
        {
            _device?.StartCameras(_deviceConfig);
        }

        void OnDisable()
        {
            _device?.StopCameras();
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
                    _onCaptureReady.Invoke(capture);
                }
            }
        }

        void OnDestroy()
        {
            _device?.Dispose();
            _device = null;
        }
    }
}
