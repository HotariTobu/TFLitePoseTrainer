using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

public class ModelItem(ModelData modelData) : SharedWPF.ViewModelBase
{
    #region == Label ==

    private string _label = modelData.Label ?? modelData.Id;
    public string Label
    {
        get => _label;
        set
        {
            if (modelData.UpdateLabel(value))
            {
                _label = value;
                RaisePropertyChanged(nameof(Label));
            }
        }
    }

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

    public Exception? Delete() => modelData.Delete();
}
