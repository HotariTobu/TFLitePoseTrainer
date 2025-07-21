using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;

using K4AdotNet.BodyTracking;

using TFLitePoseTrainer.Data;
using TFLitePoseTrainer.Loops;

namespace TFLitePoseTrainer.Main;

public partial class Window : System.Windows.Window
{
    private static readonly string PoseLabelFormat = "Pose {0}";
    private static readonly Regex PoseLabelRegex = new(@"Pose (\d+)");

    private readonly DataSource _dataSource;
    Record.Window? _recordWindow;

    public Window()
    {
        InitializeComponent();
        _dataSource = (DataSource)DataContext;

        InitializePoseItems();
        InitializeModelItems();
        InitializeSubWindows();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        _recordWindow?.CloseWithoutHiding();
    }

    private async void InitializePoseItems()
    {
        var poseDataList = await Task.Run(PoseData.List);
        if (poseDataList is null)
        {
            MessageBox.Show("Failed loading pose data");
            return;
        }

        var poseItems = _dataSource.PoseItems;

        foreach (var poseData in poseDataList)
        {
            var poseItem = new PoseItem(poseData);
            poseItems.Add(poseItem);
        }
    }

    private async void InitializeModelItems()
    {
        var modelDataList = await Task.Run(ModelData.List);
        if (modelDataList is null)
        {
            MessageBox.Show("Failed loading model data");
            return;
        }

        var modelItems = _dataSource.ModelItems;

        foreach (var modelData in modelDataList)
        {
            var modelItem = new ModelItem(modelData);
            modelItems.Add(modelItem);
        }
    }

    private async void InitializeSubWindows()
    {
        await WaitForConnection();

        var exception = await CheckRuntime(TrackerProcessingMode.GpuCuda);
        if (exception is not null)
        {
            MessageBox.Show(exception.Message, "Body Tracking Not Available", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown(11);
            return;
        }

        var captureParam = new CaptureLoop.Param(DeviceConfig);
        CaptureLoop? captureLoop;
        (captureLoop, exception) = await CaptureLoop.Create(captureParam);
        if (captureLoop is null || exception is not null)
        {
            MessageBox.Show("Failed to create capture loop.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown(12);
            return;
        }

        var trackingParam = new TrackingLoop.Param(captureLoop.Calibration);
        TrackingLoop? trackingLoop;
        (trackingLoop, exception) = TrackingLoop.Create(trackingParam);
        if (trackingLoop is null || exception is not null)
        {
            MessageBox.Show("Failed to create tracking loop.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown(13);
            return;
        }

        captureLoop.CaptureReady += trackingLoop.Enqueue;

        _recordWindow = new(captureLoop, trackingLoop);
        _recordWindow.OnPoseRecorded += OnPoseRecorded;
    }

    private void OnAddPoseButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_recordWindow is null)
        {
            MessageBox.Show("Record window not initialized.");
            return;
        }

        _recordWindow.Show();
        _recordWindow.Activate();
    }

    private void OnPoseRecorded(PoseData poseData)
    {
        var poseLabels = _dataSource.PoseItems.Select(p => p.Label);
        var poseItem = new PoseItem(poseData)
        {
            Label = GetInitialPoseLabel(poseLabels)
        };
        _dataSource.PoseItems.Add(poseItem);
    }

    private void OnDeletePoseButtonClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.Tag is not PoseItem poseItem)
        {
            return;
        }

        var exception = RemovePoseItem(poseItem);
        if (exception is not null)
        {
            throw new Exception($"Failed deleting pose: {exception.Message}", exception);
        }
    }

    private void OnReviewModelButtonClicked(object sender, RoutedEventArgs e)
    {
        var selectedModelItem = _dataSource.SelectedModelItems.ToList();
        ReviewModel(selectedModelItem);
    }

    private void OnAddModelButtonClicked(object sender, RoutedEventArgs e)
    {
        var selectedPoseItems = _dataSource.SelectedPoseItems.ToList();
        AddModelItem(selectedPoseItems);
    }

    private void OnDeleteModelButtonClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.Tag is not ModelItem modelItem)
        {
            return;
        }

        var exception = RemoveModelItem(modelItem);
        if (exception is not null)
        {
            throw new Exception($"Failed deleting model: {exception.Message}", exception);
        }
    }

    private void ReviewModel(IReadOnlyCollection<ModelItem> modelItems)
    {
        if (modelItems.Count != 1)
        {
            MessageBox.Show("Please select only one model to review.");
            return;
        }

        var modelItem = modelItems.First();

        throw new NotImplementedException();
    }

    private async void AddModelItem(IReadOnlyCollection<PoseItem> poseItems)
    {
        if (poseItems.Count < 2)
        {
            MessageBox.Show("Please select at least two poses to train a model.");
            return;
        }

        var poseLabels = poseItems.Select(p => p.Label);

        var (modelData, exception) = ModelData.Create(poseLabels);
        if (modelData is null || exception is not null)
        {
            throw new Exception($"Failed creating model data: {exception?.Message}", exception);
        }

        var initialLabel = GetInitialModelLabel(poseLabels);
        modelData.UpdateLabel(initialLabel);

        var trainingModelItem = new TrainingModelItem(modelData);
        _dataSource.TrainingModelItems.Add(trainingModelItem);

        var poseDataPaths = poseItems.Select(p => p.DataPath);
        exception = await Trainer.Train(modelData.DataPath, poseDataPaths,
         (progress) => trainingModelItem.ProgressValue = progress);

        _dataSource.TrainingModelItems.Remove(trainingModelItem);

        if (exception is null)
        {
            var modelItem = new ModelItem(modelData);
            _dataSource.ModelItems.Add(modelItem);
        }
        else
        {
            var deleteException = modelData.Delete();
            if (deleteException is null)
            {
                throw new Exception($"Failed training model: {exception.Message}", exception);
            }
            else
            {
                throw new AggregateException($"Failed training model: {exception.Message}, and deleting model: {deleteException.Message}", exception, deleteException);
            }
        }
    }

    private Exception? RemovePoseItem(PoseItem poseItem)
    {
        var exception = poseItem.Delete();
        if (exception is not null)
        {
            return exception;
        }

        _dataSource.PoseItems.Remove(poseItem);

        return null;
    }

    private Exception? RemoveModelItem(ModelItem modelItem)
    {
        var exception = modelItem.Delete();
        if (exception is not null)
        {
            return exception;
        }

        _dataSource.ModelItems.Remove(modelItem);

        return null;
    }

    private static string GetInitialPoseLabel(IEnumerable<string> poseLabels)
    {
        var lastPoseLabel = poseLabels.LastOrDefault(PoseLabelRegex.IsMatch);
        var lastPoseIndex = 0;

        if (lastPoseLabel is not null)
        {
            var match = PoseLabelRegex.Match(lastPoseLabel);
            lastPoseIndex = int.Parse(match.Groups[1].Value);
        }

        return string.Format(PoseLabelFormat, lastPoseIndex + 1);
    }

    private static string GetInitialModelLabel(IEnumerable<string> selectedPoseLabels)
    {
        var poseNames = selectedPoseLabels.Select(label =>
        {
            var match = PoseLabelRegex.Match(label);
            if (match.Success)
            {
                var poseIndex = int.Parse(match.Groups[1].Value);
                return poseIndex.ToString();
            }
            else
            {
                return label;
            }

        });

        return $"Pose: {string.Join(", ", poseNames)}";
    }
}
