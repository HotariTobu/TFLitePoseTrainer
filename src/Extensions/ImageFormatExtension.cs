using System.Windows.Media;

using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Extensions;

static class ImageFormatExtension
{
    internal static PixelFormat ToPixelFormat(this ImageFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFormat.ColorBgra32 => PixelFormats.Bgra32,
            _ => throw new("Unsupported image format."),
        };
    }
}
