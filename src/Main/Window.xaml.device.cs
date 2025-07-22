using System.Windows;

using K4AdotNet;
using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

using TFLitePoseTrainer.Exceptions;
using TFLitePoseTrainer.Loops;

namespace TFLitePoseTrainer.Main;

partial class Window
{
    static readonly DeviceConfiguration DeviceConfig = new()
    {
        CameraFps = FrameRate.Thirty,
        DepthMode = DepthMode.NarrowViewUnbinned,
        ColorResolution = ColorResolution.R720p,
        ColorFormat = ImageFormat.ColorBgra32,
        SynchronizedImagesOnly = true,
    };

    static async Task WaitForConnection()
    {
        while (true)
        {
            var deviceCount = await Task.Run(() => Device.InstalledCount);
            if (deviceCount > 0)
            {
                break;
            }

            var result = MessageBox.Show("Confirm connecting a tracking device, then press OK.", "Device Not Found", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK);
            if (result == MessageBoxResult.OK)
            {
                continue;
            }

            Application.Current.Shutdown(1);
        }
    }

    static async Task CheckRuntime(TrackerProcessingMode mode)
    {
        var exception = await Task.Run<Exception?>(() =>
        {
            try
            {
                Sdk.TryInitializeBodyTrackingRuntime(mode, out var message);
                return message is null ? null : new(message);
            }
            catch (Exception e)
            {
                return new($"Failed to check body tracking runtime availability", e);
            }
        });

        if (exception is null)
        {
            return;
        }

        throw new FatalException(2, $"Body tracking runtime is not available", exception);
    }

    static async Task<(CaptureLoop, TrackingLoop)> CreateLoops()
    {
        var captureLoopResult = await CaptureLoop.Create(new(DeviceConfig));
        if (!captureLoopResult.TryGetValue(out var captureLoop))
        {
            throw new FatalException(3, $"Failed creating capture loop", captureLoopResult.Exception);
        }

        var trackingLoopResult = TrackingLoop.Create(new(captureLoop.Calibration));
        if (!trackingLoopResult.TryGetValue(out var trackingLoop))
        {
            throw new FatalException(4, $"Failed creating tracking loop", trackingLoopResult.Exception);
        }

        captureLoop.CaptureReady += trackingLoop.Enqueue;

        return (captureLoop, trackingLoop);
    }
}
