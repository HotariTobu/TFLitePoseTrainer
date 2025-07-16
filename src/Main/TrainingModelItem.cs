using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

public class TrainingModelItem(ModelData modelData) : SharedWPF.ViewModelBase
{
    #region == Label ==

    public string Label => modelData.Label ?? modelData.Id;

    #endregion
    #region == ProgressValue ==

    private float _progressValue;
    public float ProgressValue
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
