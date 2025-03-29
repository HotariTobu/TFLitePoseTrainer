using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
}
