using System.Diagnostics;
using System.Windows;

using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

using TFLitePoseTrainer.Data;
using TFLitePoseTrainer.Extensions;
using TFLitePoseTrainer.Helpers;
using TFLitePoseTrainer.Loops;

namespace TFLitePoseTrainer.Record;

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

            Skeleton skeleton;

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

        using var defer = new Defer();
        defer.Disposed += () => _dataSource.CanStartRecording = true;
        defer.Disposed += () => _dataSource.IsRecording = false;
        defer.Disposed += () => _dataSource.ProgressValue = 0.0;

        var frames = new PoseFrame[Constants.PoseFrameCount];

        for (var i = 0; i < Constants.PoseFrameCount;)
        {
            if (!IsVisible)
            {
                return;
            }

            var tcs = new TaskCompletionSource<Skeleton?>();

            void SetResult(BodyFrame bodyFrame)
            {
                if (bodyFrame.BodyCount == 0)
                {
                    tcs.TrySetResult(null);
                }
                else
                {
                    bodyFrame.GetBodySkeleton(0, out var skeleton);
                    tcs.TrySetResult(skeleton);
                }

                _trackingLoop.BodyFrameReady -= SetResult;
            }

            _trackingLoop.BodyFrameReady += SetResult;

            var skeleton = await tcs.Task;
            if (skeleton.HasValue)
            {
                var jointVectors = skeleton.Value.GetNormalizedJointVectors();
                frames[i] = new(jointVectors);
                i++;

                _dataSource.ProgressValue = (double)i / Constants.PoseFrameCount;
            }
        }

        var sample = new PoseSample(frames);
        var poseData = PoseData.Create(_dataSource.CaptureImage, sample);
        if (poseData is null)
        {
            MessageBox.Show("Failed creating pose data");
        }
        else
        {
            OnPoseRecorded?.Invoke(poseData);
        }
    }

    public event Action<PoseData>? OnPoseRecorded;
}
