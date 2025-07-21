using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Review;

internal class DataSource : SharedWPF.ViewModelBase
{
    #region == WindowTitle ==

    private string _windowTitle = "";
    public string WindowTitle
    {
        get => _windowTitle;
        set
        {
            if (_windowTitle != value)
            {
                _windowTitle = value;
                RaisePropertyChanged(nameof(WindowTitle));
            }
        }
    }

    #endregion

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

    #region == InferredPoseLabels ==

    public ObservableCollection<string> InferredPoseLabels { get; } = [];

    #endregion
}
