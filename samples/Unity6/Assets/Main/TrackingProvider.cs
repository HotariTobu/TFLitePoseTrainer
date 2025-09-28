using System;

using K4AdotNet.Sensor;
using K4AdotNet.BodyTracking;

using UnityEngine;

#nullable enable

public class TrackingProvider : MonoBehaviour
{
    [SerializeField] CaptureProvider? _captureProvider;

    [SerializeField] TrackerConfiguration _trackerConfig = TrackerConfiguration.Default;

    public event Action<BodyFrame>? BodyFrameReady;

    private Tracker? _tracker;

    void Start()
    {
        if (_captureProvider is null)
        {
            return;
        }

        var calibration = _captureProvider.Calibration;
        if (calibration is null)
        {
            return;
        }

        _tracker = new(calibration.Data, _trackerConfig);
        _captureProvider.CaptureReady += OnCaptureReady;
    }

    void OnCaptureReady(Capture capture)
    {
        if (_tracker is null)
        {
            return;
        }

        try
        {
            _tracker.EnqueueCapture(capture);
        }
        catch (Exception e)
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
            Debug.Log(bodyFrame.BodyCount);

            using (bodyFrame)
            {
                BodyFrameReady?.Invoke(bodyFrame);
            }
        }
    }

    void OnDestroy()
    {
        _tracker?.Dispose();
        _tracker = null;
    }
}
