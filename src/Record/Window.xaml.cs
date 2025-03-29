using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Record;

public partial class Window : System.Windows.Window
{
    private readonly DataSource _dataSource;
    public bool CanClose = false;

    public Window()
    {
        InitializeComponent();
        _dataSource = (DataSource)DataContext;
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
        _dataSource.IsRecording = true;

        //await Task.Delay(5000);

        _dataSource.CanStartRecording = true;
        _dataSource.IsRecording = false;

        OnPoseRecorded?.Invoke(new());
    }

    public event Action<PoseData>? OnPoseRecorded;
}