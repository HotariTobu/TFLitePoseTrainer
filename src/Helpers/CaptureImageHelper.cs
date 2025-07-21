using System.Windows.Media;
using System.Windows.Media.Imaging;

using K4AdotNet.Sensor;

using TFLitePoseTrainer.Extensions;

namespace TFLitePoseTrainer.Helpers;

static class CaptureImageHelper
{
    internal static WriteableBitmap CreateRenderTarget(in DeviceConfiguration deviceConfig, Visual visual)
    {
        var width = deviceConfig.ColorResolution.WidthPixels();
        var height = deviceConfig.ColorResolution.HeightPixels();
        var format = deviceConfig.ColorFormat.ToPixelFormat();

        var dpiScale = VisualTreeHelper.GetDpi(visual);
        double dpiX = dpiScale.PixelsPerInchX;
        double dpiY = dpiScale.PixelsPerInchY;

        return new(width, height, dpiX, dpiY, format, null);
    }
}
