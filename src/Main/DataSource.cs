using System.Collections.ObjectModel;

namespace TFLitePoseTrainer.Main;

internal class DataSource : SharedWPF.ViewModelBase
{
    #region == PoseItems ==

    public ObservableCollection<PoseItem> PoseItems { get; } = [];

    #endregion
    #region == SelectedPoseItems ==

    public ObservableCollection<PoseItem> SelectedPoseItems { get; } = [];

    #endregion
    #region == IsEditingPoses ==

    bool _isEditingPoses;
    public bool IsEditingPoses
    {
        get => _isEditingPoses;
        set
        {
            if (_isEditingPoses != value)
            {
                _isEditingPoses = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion

    #region == TrainingModelItems ==

    public ObservableCollection<TrainingModelItem> TrainingModelItems { get; } = [];

    #endregion

    #region == ModelItems ==

    public ObservableCollection<ModelItem> ModelItems { get; } = [];

    #endregion
    #region == SelectedModelItems ==

    public ObservableCollection<ModelItem> SelectedModelItems { get; } = [];

    #endregion
    #region == IsEditingModels ==

    bool _isEditingModels;
    public bool IsEditingModels
    {
        get => _isEditingModels;
        set
        {
            if (_isEditingModels != value)
            {
                _isEditingModels = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion
}

internal class PoseListBoxSelectionBehavior : Behaviors.ListBoxSelectionBehavior<PoseItem> { }
internal class ModelListBoxSelectionBehavior : Behaviors.ListBoxSelectionBehavior<ModelItem> { }
