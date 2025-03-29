using System.Windows.Media;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

public class PoseItem(PoseData poseData) : SharedWPF.ViewModelBase
{
    private readonly PoseData _poseData = poseData;

    public ImageSource ThumbnailSource { get; init; } = poseData.GetThumbnailSource();
    public string Label
    {
        get => _poseData.Label ?? "";
        set
        {
            if (_poseData.UpdateLabel(value))
            {
                RaisePropertyChanged(nameof(Label));
            }
        }
    }
}