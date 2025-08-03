using System.ComponentModel;
using System.Windows;

using K4AdotNet.BodyTracking;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

partial class Window : System.Windows.Window
{
    readonly DataSource _dataSource;
    Record.Window? _recordWindow;
    Review.Window? _reviewWindow;

    internal Window()
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
        _reviewWindow?.CloseWithoutHiding();
    }

    async void InitializeSubWindows()
    {
        _dataSource.IsInitializing = true;

        await Task.WhenAll(WaitForConnection(), CheckRuntime(TrackerProcessingMode.GpuCuda));

        var (captureLoop, trackingLoop) = await CreateLoops();

        _recordWindow = new(captureLoop, trackingLoop);
        _recordWindow.OnPoseRecorded += OnPoseRecorded;

        _reviewWindow = new(captureLoop, trackingLoop);

        _dataSource.IsInitializing = false;
    }

    void OnAddPoseButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_recordWindow is null)
        {
            MessageBox.Show("Record window not initialized.", "Please Try Again Later", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _recordWindow.ShowAndActivate();
    }

    void OnPoseRecorded(PoseData poseData)
    {
        AddPoseItem(poseData);
    }

    void OnDeletePoseButtonClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.Tag is not PoseItem poseItem)
        {
            return;
        }

        var result = RemovePoseItem(poseItem);
        if (result.HasException)
        {
            throw new("Failed deleting pose", result.Exception);
        }
    }

    void OnReviewModelButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_reviewWindow is null)
        {
            MessageBox.Show("Review window not initialized.", "Please Try Again Later", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.Tag is not ModelItem modelItem)
        {
            return;
        }

        _reviewWindow.UpdateModel(modelItem.Label, modelItem.DataPath, modelItem.PoseLabels);
        _reviewWindow.ShowAndActivate();
    }

    void OnAddModelButtonClicked(object sender, RoutedEventArgs e)
    {
        var selectedPoseItems = _dataSource.SelectedPoseItems.ToList();

        if (selectedPoseItems.Count < 2)
        {
            MessageBox.Show("Please select at least two poses to train a model.", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        AddModelItem(selectedPoseItems);
    }

    void OnDeleteModelButtonClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.Tag is not ModelItem modelItem)
        {
            return;
        }

        var result = RemoveModelItem(modelItem);
        if (result.HasException)
        {
            throw new("Failed deleting model", result.Exception);
        }
    }
}
