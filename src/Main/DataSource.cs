using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TFLitePoseTrainer.Main;

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
    #region == Timestamp ==

    private string? _timestamp;
    public string? Timestamp
    {
        get => _timestamp;
        set
        {
            if (_timestamp != value)
            {
                _timestamp = value;
                RaisePropertyChanged(nameof(Timestamp));
            }
        }
    }

    #endregion

    #region == Poses ==

    public ObservableCollection<PoseItem> Poses { get; } = [];

    #endregion

    #region == IsEditing ==

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing != value)
            {
                _isEditing = value;
                RaisePropertyChanged(nameof(IsEditing));
            }
        }
    }

    #endregion
}
