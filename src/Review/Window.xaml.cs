using System.Diagnostics;

using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

using TFLitePoseTrainer.Extensions;
using TFLitePoseTrainer.Helpers;
using TFLitePoseTrainer.Loops;

namespace TFLitePoseTrainer.Review;

partial class Window : SubWindow
{
    readonly CaptureLoop _captureLoop;
    readonly TrackingLoop _trackingLoop;

    readonly DataSource _dataSource;

    Classifier? _classifier;
    IReadOnlyList<string> _poseLabels = [];

    internal Window(CaptureLoop captureLoop, TrackingLoop trackingLoop)
    {
        _captureLoop = captureLoop;
        _trackingLoop = trackingLoop;

        InitializeComponent();

        _dataSource = (DataSource)DataContext;
        _dataSource.Calibration = _captureLoop.Calibration;
        _dataSource.CaptureImage = CaptureImageHelper.CreateRenderTarget(_captureLoop.DeviceConfig, this);

        _captureLoop.ErrorOccurred += Console.Error.WriteLine;
        _trackingLoop.ErrorOccurred += Console.Error.WriteLine;
    }

    protected override async void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        await Task.WhenAll(Task.Run(_captureLoop.Start), Task.Run(_trackingLoop.Start));

        _captureLoop.CaptureReady += UpdateCaptureImage;
        _trackingLoop.BodyFrameReady += UpdateSkeleton;
        _trackingLoop.BodyFrameReady += UpdateInferredPoseLabels;
    }

    protected override async void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        _captureLoop.CaptureReady -= UpdateCaptureImage;
        _trackingLoop.BodyFrameReady -= UpdateSkeleton;
        _trackingLoop.BodyFrameReady -= UpdateInferredPoseLabels;

        await Task.WhenAll(Task.Run(_captureLoop.Stop), Task.Run(_trackingLoop.Stop));
    }

    internal void UpdateModel(string label, string dataPath, IReadOnlyList<string> poseLabels)
    {
        _dataSource.WindowTitle = label;

        _classifier = new Classifier(dataPath);
        _poseLabels = poseLabels;
    }

    void UpdateCaptureImage(Capture capture)
    {
        Debug.Assert(_dataSource.CaptureImage is not null);

        using var image = capture.ColorImage;
        if (image is null)
        {
            Console.Error.WriteLine("Failed to get color image from capture.");
            return;
        }

        var rendererResult = CaptureImageHelper.Renderer.Create(image);
        if (!rendererResult.TryGetValue(out var renderer))
        {
            Console.Error.WriteLine($"Failed to create renderer: {rendererResult.Exception}");
            return;
        }

        Dispatcher.Invoke(() =>
        {
            renderer.Render(_dataSource.CaptureImage);
            renderer.Dispose();
        });
    }

    void UpdateSkeleton(BodyFrame bodyFrame)
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

    void UpdateInferredPoseLabels(BodyFrame bodyFrame)
    {
        if (_classifier is null)
        {
            return;
        }

        var inferredPoseLabels = _dataSource.InferredPoseLabels;
        var newInferredPoseLabels = new List<string>();

        for (var bodyIndex = 0; bodyIndex < bodyFrame.BodyCount; bodyIndex++)
        {
            Skeleton skeleton;

            try
            {
                bodyFrame.GetBodySkeleton(bodyIndex, out skeleton);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to get skeleton for body {bodyIndex}: {e}");
                continue;
            }

            var jointNormalizedVectors = skeleton.GetNormalizedJointVectors();
            var poseLabelIndex = _classifier.Classify(jointNormalizedVectors);
            if (!poseLabelIndex.HasValue)
            {
                Console.Error.WriteLine($"Failed to classify pose for body {bodyIndex}");
                continue;
            }

            if (poseLabelIndex.Value < 0 || poseLabelIndex.Value >= _poseLabels.Count)
            {
                Console.Error.WriteLine($"Invalid pose label index: {poseLabelIndex.Value}");
                continue;
            }

            var poseLabel = _poseLabels[poseLabelIndex.Value];
            newInferredPoseLabels.Add(poseLabel);
        }

        Dispatcher.Invoke(() =>
        {
            inferredPoseLabels.Clear();

            foreach (var poseLabel in newInferredPoseLabels)
            {
                inferredPoseLabels.Add(poseLabel);
            }
        });
    }
}
