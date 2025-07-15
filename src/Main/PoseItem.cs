using System.Windows.Media;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

public class PoseItem(PoseData poseData) : SharedWPF.ViewModelBase
{
    public string DataPath => poseData.DataPath;

    #region == ThumbnailSource ==

    public ImageSource ThumbnailSource { get; } = poseData.GetThumbnailSource();

    #endregion
    #region == Label ==

    private string _label = poseData.Label ?? poseData.Id;
    public string Label
    {
        get => _label;
        set
        {
            if (poseData.UpdateLabel(value))
            {
                _label = value;
                RaisePropertyChanged(nameof(Label));
            }
        }
    }

    #endregion

    public Exception? Delete() => poseData.Delete();
}
