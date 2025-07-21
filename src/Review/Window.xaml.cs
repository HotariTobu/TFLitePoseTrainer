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
using TFLitePoseTrainer.Helpers;
using TFLitePoseTrainer.Loops;

namespace TFLitePoseTrainer.Review;

partial class Window : SubWindow
{
    private readonly CaptureLoop _captureLoop;
    private readonly TrackingLoop _trackingLoop;

    private readonly DataSource _dataSource;

    internal Window(CaptureLoop captureLoop, TrackingLoop trackingLoop)
    {
        _captureLoop = captureLoop;
        _trackingLoop = trackingLoop;

        InitializeComponent();

        _dataSource = (DataSource)DataContext;
        _dataSource.Calibration = _captureLoop.Calibration;
        _dataSource.CaptureImage = CaptureImageHelper.CreateRenderTarget(_captureLoop.DeviceConfig, this);
    }

    protected override async void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        await Task.WhenAll(Task.Run(_captureLoop.Start), Task.Run(_trackingLoop.Start));

        _captureLoop.CaptureReady += UpdateCaptureImage;
        _trackingLoop.BodyFrameReady += UpdateSkeleton;
    }

    protected override async void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        _captureLoop.CaptureReady -= UpdateCaptureImage;
        _trackingLoop.BodyFrameReady -= UpdateSkeleton;

        await Task.WhenAll(Task.Run(_captureLoop.Stop), Task.Run(_trackingLoop.Stop));
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

        var (renderer, exception) = CaptureImageHelper.Renderer.Create(image);
        if (renderer is null || exception is not null)
        {
            Console.Error.WriteLine($"Failed to create renderer: {exception}");
            return;
        }

        Dispatcher.Invoke(() =>
        {
            renderer.Render(_dataSource.CaptureImage);
            renderer.Dispose();
        });
    }

    private void UpdateSkeleton(BodyFrame bodyFrame)
    {
        var skeletonItems = _dataSource.SkeletonItems;
        var actionDictionary = (
            from item in skeletonItems
            let initialAction = new Action(() => skeletonItems.Remove(item))
            select KeyValuePair.Create(item.BodyId, initialAction)
        ).ToDictionary();

        for (var bodyIndex = 0; bodyIndex < bodyFrame.BodyCount; bodyIndex++)
        {
            var bodyId = bodyFrame.GetBodyId(bodyIndex);
            if (!bodyId.IsValid)
            {
                continue;
            }

            Skeleton skeleton = default;

            try
            {
                bodyFrame.GetBodySkeleton(bodyIndex, out skeleton);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to get skeleton for body {bodyId}: {e}");
                continue;
            }

            var item = skeletonItems.FirstOrDefault(item => item.BodyId == bodyId);
            if (item is null)
            {
                actionDictionary.Add(bodyId, () =>
                {
                    item = new SkeletonItem(bodyId, skeleton);
                    skeletonItems.Add(item);
                });
            }
            else
            {
                actionDictionary[bodyId] = () => item.Skeleton = skeleton;
            }
        }

        Dispatcher.Invoke(() =>
        {
            foreach (var action in actionDictionary.Values)
            {
                action();
            }
        });
    }
}
