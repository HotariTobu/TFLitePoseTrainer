using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

using TFLitePoseTrainer.Data;
using TFLitePoseTrainer.Extensions;

namespace TFLitePoseTrainer.Record;

public partial class Window : System.Windows.Window
{
    public bool CanClose = false;

    private readonly DataSource _dataSource;
    private readonly Timer _timer;

    public Window()
    {
        InitializeComponent();
        _dataSource = (DataSource)DataContext;

        _dataSource.CaptureImage = new(1920, 1080, 96, 96, PixelFormats.Bgra32, null);

        _timer = new(TimerTick);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _timer.Start(1000 / 30);
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        _timer.Stop();
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
        _timer.Dispose();
    }

    private void TimerTick(object? state)
    {
        Application.Current.Dispatcher.BeginInvoke(UpdateCaptureImage);
    }

    private void UpdateCaptureImage()
    {
        Debug.Assert(_dataSource.CaptureImage is not null);

        var random = new Random();

        var writableBitmap = _dataSource.CaptureImage;
        var maxWidth = writableBitmap.PixelWidth;
        var maxHeight = writableBitmap.PixelHeight;

        var width = random.Next(maxWidth / 100, maxWidth / 4);
        var height = random.Next(maxHeight / 100, maxHeight / 4);

        var r = (byte)random.Next(0, 255);
        var g = (byte)random.Next(0, 255);
        var b = (byte)random.Next(0, 255);

        var pixels = new byte[width * height * 4];
        for (var i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = b; 
            pixels[i + 1] = g;
            pixels[i + 2] = r; 
            pixels[i + 3] = 255; 
        }

        var x = random.Next(0, maxWidth - width);
        var y = random.Next(0, maxHeight - height);

        var rect = new Int32Rect(x, y, width, height);
        writableBitmap.WritePixels(rect, pixels, width * 4, 0);
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
            MessageBox.Show("Failed recording pose");
        }
        else
        {
            OnPoseRecorded?.Invoke(poseData);
        }
            
        _dataSource.CanStartRecording = true;
        _dataSource.IsRecording = false;
    }

    public event Action<PoseData>? OnPoseRecorded;
}