using System.Windows.Media;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

public class ModelItem(ModelData modelData) : SharedWPF.ViewModelBase
{
    private readonly ModelData _modelData = modelData;

    #region === Label ===

    private string _label = modelData.Label ?? modelData.Id;
    public string Label
    {
        get => _label;
        set
        {
            if (_modelData.UpdateLabel(value))
            {
                _label = value;
                RaisePropertyChanged(nameof(Label));
            }
        }
    }

    #endregion
}
