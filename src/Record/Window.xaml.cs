using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using K4AdotNet;
using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

using TFLitePoseTrainer.Data;
using TFLitePoseTrainer.Extensions;
using TFLitePoseTrainer.Loops;

namespace TFLitePoseTrainer.Record;

public partial class Window : System.Windows.Window
{
    private static readonly DeviceConfiguration DeviceConfig = new()
    {
        CameraFps = FrameRate.Thirty,
        DepthMode = DepthMode.NarrowViewUnbinned,
        ColorResolution = ColorResolution.R720p,
        ColorFormat = ImageFormat.ColorBgra32,
        SynchronizedImagesOnly = true,
    };

    public bool CanClose = false;

    private readonly DataSource _dataSource;

    private CaptureLoop? _captureLoop;
    private TrackingLoop? _trackingLoop;

    public bool CanShow => _captureLoop is not null && _trackingLoop is not null;

    public Window()
    {
        InitializeComponent();
        _dataSource = (DataSource)DataContext;

        var dpiScale = VisualTreeHelper.GetDpi(this);
        _dataSource.CaptureImage = CreateRenderTarget(DeviceConfig, dpiScale);

        InitializeService(DeviceConfig);
    }

    protected override async void OnActivated(EventArgs e)
    {
        Debug.Assert(_captureLoop is not null);
        Debug.Assert(_trackingLoop is not null);

        base.OnActivated(e);
        await Task.WhenAll(Task.Run(_captureLoop.Start), Task.Run(_trackingLoop.Start));
    }

    protected override async void OnDeactivated(EventArgs e)
    {
        Debug.Assert(_captureLoop is not null);
        Debug.Assert(_trackingLoop is not null);

        base.OnDeactivated(e);
        await Task.WhenAll(Task.Run(_captureLoop.Stop), Task.Run(_trackingLoop.Stop));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (CanClose)
        {
            return;
        }

        e.Cancel = true;
        this.Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _captureLoop?.Dispose();
        _trackingLoop?.Dispose();
    }

    private async void InitializeService(DeviceConfiguration deviceConfig)
    {
        await Task.WhenAll(CheckRuntime(), WaitForConnection());
        _captureLoop = await CreateCaptureLoop(deviceConfig);
        var calibration = await GetCalibration(_captureLoop);
        _trackingLoop = await CreateTrackingLoop(calibration);

        _captureLoop.CaptureReady += _trackingLoop.Enqueue;
        _captureLoop.CaptureReady += UpdateCaptureImage;
        _trackingLoop.BodyFrameReady += UpdateSkeleton;
    }

    private void UpdateCaptureImage(Capture capture)
    {
        Debug.Assert(_dataSource.CaptureImage is not null);

        using var image = capture.ColorImage;
        if (image is null)
        {
            Console.Error.WriteLine("Failed to get color image from capture.");
            return;
        }

        var width = image.WidthPixels;
        var height = image.HeightPixels;

        var bufferSize = image.SizeBytes;
        var stride = image.StrideBytes;

        var buffer = Marshal.AllocHGlobal(bufferSize);
        if (buffer == IntPtr.Zero)
        {
            Console.Error.WriteLine("Failed to allocate memory for image buffer.");
            return;
        }

        unsafe
        {
            var src = image.Buffer.ToPointer();
            var dst = buffer.ToPointer();
            Buffer.MemoryCopy(src, dst, bufferSize, bufferSize);
        }

        Dispatcher.Invoke(() =>
        {
            var rect = new Int32Rect(0, 0, width, height);
            _dataSource.CaptureImage.WritePixels(rect, buffer, bufferSize, stride);

            Marshal.FreeHGlobal(buffer);
        });
    }

    private void UpdateSkeleton(BodyFrame bodyFrame)
    {
        // TODO: Implement skeleton update logic
        // Console.WriteLine(bodyFrame);
        if (bodyFrame.BodyCount > 0)
        {
            Console.WriteLine(bodyFrame);
        }
    }

    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        _dataSource.CanStartRecording = false;
    }

    private void OnCountdownCompleted(object sender, EventArgs e)
    {
        RecordPose();
    }

    private async void RecordPose()
    {
        Debug.Assert(_dataSource.CaptureImage is not null);

        _dataSource.IsRecording = true;


        //await Task.Delay(5000);


        var poseData = PoseData.Create(_dataSource.CaptureImage);
        if (poseData is null)
        {
            MessageBox.Show("Failed creating pose data");
        }
        else
        {
            OnPoseRecorded?.Invoke(poseData);
        }

        _dataSource.CanStartRecording = true;
        _dataSource.IsRecording = false;
    }

    private static async Task CheckRuntime()
    {
        var errorMessage = await Task.Run(() =>
        {
            string? message = null;

            try
            {
                var mode = TrackerProcessingMode.GpuCuda;
                Sdk.TryInitializeBodyTrackingRuntime(mode, out message);
            }
            catch (Exception e)
            {
                message = $"Failed to check body tracking runtime availability: {e}";
                Console.Error.WriteLine(message);
            }

            return message;
        });

        if (errorMessage is not null)
        {
            MessageBox.Show(errorMessage, "Body Tracking Not Available", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown(10);
        }
    }

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

            Application.Current.Shutdown(11);
        }
    }

    private static async Task<CaptureLoop> CreateCaptureLoop(DeviceConfiguration deviceConfig)
    {
        var captureParam = new CaptureLoop.Param(deviceConfig);

        var captureLoop = await Task.Run(() => CaptureLoop.Create(captureParam));
        if (captureLoop is null)
        {
            MessageBox.Show("Failed to create capture loop.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown(12);
        }

        Debug.Assert(captureLoop is not null);

        return captureLoop;
    }

    private static async Task<Calibration> GetCalibration(CaptureLoop captureLoop)
    {
        var calibration = await Task.Run(captureLoop.GetCalibration);
        if (!calibration.HasValue)
        {
            MessageBox.Show("Failed to get calibration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown(13);
        }

        return calibration.Value;
    }

    private static async Task<TrackingLoop> CreateTrackingLoop(Calibration calibration)
    {
        var trackingParam = new TrackingLoop.Param(calibration);

        var trackingLoop = await Task.Run(() => TrackingLoop.Create(trackingParam));
        if (trackingLoop is null)
        {
            MessageBox.Show("Failed to create tracking loop.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown(14);
        }

        Debug.Assert(trackingLoop is not null);

        return trackingLoop;
    }

    private static WriteableBitmap CreateRenderTarget(in DeviceConfiguration deviceConfig, in DpiScale dpiScale)
    {
        var width = deviceConfig.ColorResolution.WidthPixels();
        var height = deviceConfig.ColorResolution.HeightPixels();
        var format = deviceConfig.ColorFormat.ToPixelFormat();

        double dpiX = dpiScale.PixelsPerInchX;
        double dpiY = dpiScale.PixelsPerInchY;

        return new(width, height, dpiX, dpiY, format, null);
    }

    public event Action<PoseData>? OnPoseRecorded;
}
