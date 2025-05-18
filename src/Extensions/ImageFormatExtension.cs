using System.Windows.Media;

using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Extensions;

public static class ImageFormatExtension
{
    public static PixelFormat ToPixelFormat(this ImageFormat imageFormat)
    {
        switch (imageFormat)
        {
            case ImageFormat.ColorBgra32:
                return PixelFormats.Bgra32;

            default:
                throw new Exception("Unsupported image format.");
        }
    }
}
