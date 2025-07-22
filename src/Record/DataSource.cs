using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Record;

internal class DataSource : SharedWPF.ViewModelBase
{
    #region == CaptureImage ==

    WriteableBitmap? _captureImage;
    public WriteableBitmap? CaptureImage
    {
        get => _captureImage;
        set
        {
            if (_captureImage != value)
            {
                _captureImage = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion
    #region == Calibration ==

    Calibration _calibration = new();

    public Calibration Calibration
    {
        get => _calibration;
        set
        {
            _calibration = value;
            RaisePropertyChanged();
        }
    }

    #endregion
    #region == SkeletonItems ==

    public ObservableCollection<SkeletonItem> SkeletonItems { get; } = [];

    #endregion

    #region == CanStartRecording ==

    bool _canStartRecording = true;
    public bool CanStartRecording
    {
        get => _canStartRecording;
        set
        {
            if (_canStartRecording != value)
            {
                _canStartRecording = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion
    #region == IsRecording ==

    bool _isRecording;
    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (_isRecording != value)
            {
                _isRecording = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion
    #region == ProgressValue ==

    double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            if (_progressValue != value)
            {
                _progressValue = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion
}
