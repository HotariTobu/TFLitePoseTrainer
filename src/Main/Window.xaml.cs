using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

public partial class Window : System.Windows.Window
{
    private static readonly string PoseLabelFormat = "Pose {0}";
    private static readonly Regex PoseLabelRegex = new(@"Pose (\d+)");

    private readonly DataSource _dataSource;
    private readonly Record.Window _recordWindow;

    public Window()
    {
        InitializeComponent();
        _dataSource = (DataSource)DataContext;

        InitializePoseItems();
        InitializeModelItems();

        _recordWindow = new();
        _recordWindow.OnPoseRecorded += OnPoseRecorded;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        _recordWindow.CanClose = true;
        _recordWindow.Close();
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

    private void OnAddPoseButtonClicked(object sender, RoutedEventArgs e)
    {
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

        _dataSource.PoseItems.Remove(poseItem);
    }

    private void OnAddModelButtonClicked(object sender, RoutedEventArgs e)
    {
        var selectedPoseItems = _dataSource.SelectedPoseItems.ToArray();

        // TODO: Implement training logic

        var modelData = ModelData.Create();
        if (modelData is null)
        {
            MessageBox.Show("Failed creating model data");
            return;
        }

        var selectedPoseLabels = selectedPoseItems.Select(p => p.Label);
        var modelItem = new ModelItem(modelData)
        {
            Label = GetInitialModelLabel(selectedPoseLabels)
        };
        _dataSource.ModelItems.Add(modelItem);
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

        _dataSource.ModelItems.Remove(modelItem);
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
