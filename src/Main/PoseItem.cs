using System.Windows.Media;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

class PoseItem(PoseData poseData) : SharedWPF.ViewModelBase
{
    public string DataPath => poseData.DataPath;

    #region == ThumbnailSource ==

    public ImageSource ThumbnailSource { get; } = poseData.GetThumbnailSource();

    #endregion
    #region == Label ==

    string _label = poseData.Label ?? poseData.Id;
    public string Label
    {
        get => _label;
        set
        {
            var result = poseData.UpdateLabel(value);
            if (result.HasException)
            {
                throw new("Failed updating label", result.Exception);
            }
            else
            {
                _label = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion

    public Result Delete() => poseData.Delete();
}
