using System.Runtime.InteropServices;
using System.Windows;
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

    internal class Renderer : IDisposable
    {
        readonly int _width;
        readonly int _height;
        readonly int _bufferSize;
        readonly int _stride;
        readonly IntPtr _buffer;

        internal static (Renderer?, Exception?) Create(Image colorImage)
        {
            try
            {
                return (new(colorImage), null);
            }
            catch (Exception e)
            {
                return (null, e);
            }
        }

        Renderer(Image colorImage)
        {
            _width = colorImage.WidthPixels;
            _height = colorImage.HeightPixels;
            _bufferSize = colorImage.SizeBytes;
            _stride = colorImage.StrideBytes;

            _buffer = Marshal.AllocHGlobal(_bufferSize);
            if (_buffer == IntPtr.Zero)
            {
                throw new Exception("Failed to allocate memory for image buffer.");
            }

            unsafe
            {
                var src = colorImage.Buffer.ToPointer();
                var dst = _buffer.ToPointer();
                Buffer.MemoryCopy(src, dst, _bufferSize, _bufferSize);
            }
        }

        internal void Render(WriteableBitmap renderTarget)
        {
            var rect = new Int32Rect(0, 0, _width, _height);
            renderTarget.WritePixels(rect, _buffer, _bufferSize, _stride);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_buffer);
        }
    }
}
