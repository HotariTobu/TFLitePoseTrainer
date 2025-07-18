using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Record;

internal class DataSource : SharedWPF.ViewModelBase
{
    #region == CaptureImage ==

    private WriteableBitmap? _captureImage;
    public WriteableBitmap? CaptureImage
    {
        get => _captureImage;
        set
        {
            if (_captureImage != value)
            {
                _captureImage = value;
                RaisePropertyChanged(nameof(CaptureImage));
            }
        }
    }

    #endregion
    #region == Calibration ==

    private Calibration _calibration = new();

    public Calibration Calibration
    {
        get => _calibration;
        set
        {
            _calibration = value;
            RaisePropertyChanged(nameof(Calibration));
        }
    }

    #endregion
    #region == SkeletonItems ==

    public ObservableCollection<SkeletonItem> SkeletonItems { get; } = [];

    #endregion

    #region == CanStartRecording ==

    private bool _canStartRecording = true;
    public bool CanStartRecording
    {
        get => _canStartRecording;
        set
        {
            if (_canStartRecording != value)
            {
                _canStartRecording = value;
                RaisePropertyChanged(nameof(CanStartRecording));
            }
        }
    }

    #endregion
    #region == IsRecording ==

    private bool _isRecording;
    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (_isRecording != value)
            {
                _isRecording = value;
                RaisePropertyChanged(nameof(IsRecording));
            }
        }
    }

    #endregion
    #region == ProgressValue ==

    private double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            if (_progressValue != value)
            {
                _progressValue = value;
                RaisePropertyChanged(nameof(ProgressValue));
            }
        }
    }

    #endregion
}
