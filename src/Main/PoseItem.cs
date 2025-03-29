using System.Windows.Media;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

public class PoseItem(PoseData poseData, Func<string >getFallbackLabel) : SharedWPF.ViewModelBase
{
    private readonly PoseData _poseData = poseData;

    #region === ThumbnailSource ===

    public ImageSource ThumbnailSource { get; init; } = poseData.GetThumbnailSource();

    #endregion
    #region === Label ===

    private string _label = poseData.Label ?? getFallbackLabel();
    public string Label
    {
        get => _label;
        set
        {
            if (_poseData.UpdateLabel(value))
            {
                _label = value;
                RaisePropertyChanged(nameof(Label));
            }
        }
    }

    #endregion
}