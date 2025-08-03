using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

class ModelItem(ModelData modelData) : SharedWPF.ViewModelBase
{
    public string DataPath => modelData.DataPath;
    public IReadOnlyList<string> PoseLabels => modelData.PoseLabels;

    #region == Label ==

    string _label = modelData.Label ?? modelData.Id;
    public string Label
    {
        get => _label;
        set
        {
            var result = modelData.UpdateLabel(value);
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

    public Result Delete() => modelData.Delete();
    public Result Export(string destinationPath) => modelData.Export(destinationPath);
}
