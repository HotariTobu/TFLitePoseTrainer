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

    public Exception? Delete() => modelData.Delete();
}
