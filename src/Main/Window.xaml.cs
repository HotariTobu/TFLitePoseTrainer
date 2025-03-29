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

namespace TFLitePoseTrainer.Main;

public partial class Window : System.Windows.Window
{
    private readonly static string PoseLabelFormat = "Pose {0}";
    private readonly static Regex PoseLabelRegex = new(@"Pose (\d+)");

    private readonly DataSource _dataSource;
    private readonly Record.Window _recordWindow;

    public Window()
    {
        InitializeComponent();
        _dataSource = (DataSource)DataContext;

        _recordWindow = new();
        _recordWindow.OnPoseRecorded += OnPoseRecorded;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _recordWindow.CanClose = true;
        _recordWindow.Close();
    }

    private void OnAddPoseButtonClicked(object sender, RoutedEventArgs e)
    {
        _recordWindow.Show();
        _recordWindow.Activate();
    }

    private void OnPoseRecorded(Data.PoseData poseData)
    {
        var poseItem = new PoseItem(poseData)
        {
            Label = GetNextPoseLabel()
        };

        _dataSource.Poses.Add(poseItem);
    }

    private string GetNextPoseLabel()
    {
        var lastPose = _dataSource.Poses.LastOrDefault(p => PoseLabelRegex.IsMatch(p.Label));
        var lastPoseIndex = 0;

        if (lastPose is not null)
        {
            var match = PoseLabelRegex.Match(lastPose.Label);
            lastPoseIndex= int.Parse(match.Groups[1].Value);
        }

        return string.Format(PoseLabelFormat, lastPoseIndex + 1);
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

        _dataSource.Poses.Remove(poseItem);
    }
}
