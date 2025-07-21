using System.Windows;

using K4AdotNet;
using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Main;

partial class Window : System.Windows.Window
{
    private static readonly DeviceConfiguration DeviceConfig = new()
    {
        CameraFps = FrameRate.Thirty,
        DepthMode = DepthMode.NarrowViewUnbinned,
        ColorResolution = ColorResolution.R720p,
        ColorFormat = ImageFormat.ColorBgra32,
        SynchronizedImagesOnly = true,
    };

    private static async Task WaitForConnection()
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

    private static Task<Exception?> CheckRuntime(TrackerProcessingMode mode) => Task.Run(() =>
        {
            try
            {
                Sdk.TryInitializeBodyTrackingRuntime(mode, out var message);
                return message is null ? null : new Exception(message);
            }
            catch (Exception e)
            {
                return new Exception($"Failed to check body tracking runtime availability", e);
            }
        });
}
