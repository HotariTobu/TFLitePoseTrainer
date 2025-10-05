using K4AdotNet.Sensor;
using K4AdotNet.BodyTracking;

using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace Assets.Main.Tracking
{
    public class TrackingProvider : MonoBehaviour
    {
        [SerializeField] protected TrackerConfiguration _trackerConfig = TrackerConfiguration.Default;

        [SerializeField] protected UnityEvent<BodyFrame> _onBodyFrameReady = default!;

        private Tracker? _tracker;

        public void InitializeWith(Calibration calibration)
        {
            if (_tracker is not null)
            {
                return;
            }

            _tracker = new(calibration.Data, _trackerConfig);
        }

        public void EnqueueCapture(Capture capture)
        {
            if (_tracker is null)
            {
                return;
            }

            try
            {
                _tracker.EnqueueCapture(capture);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        void Update()
        {
            if (_tracker is null)
            {
                return;
            }

            var deltaTime = Time.deltaTime * 1000;
            var timeout = new K4AdotNet.Timeout((int)deltaTime);

            if (_tracker.TryPopResult(out var bodyFrame, timeout))
            {
                using (bodyFrame)
                {
                    _onBodyFrameReady.Invoke(bodyFrame);
                }
            }
        }

        void OnDestroy()
        {
            _tracker?.Dispose();
            _tracker = null;
        }
    }
}
