using System.Text;
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
        _dataSource.Poses.Add(new() {
            Data = poseData,
            Label =  $"Pose {_dataSource.Poses.Count}",
        });
    }
}
