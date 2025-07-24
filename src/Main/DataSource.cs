using System.Collections.ObjectModel;

namespace TFLitePoseTrainer.Main;

internal class DataSource : SharedWPF.ViewModelBase
{
    #region == IsInitializing ==

    bool _isInitializing;
    public bool IsInitializing
    {
        get => _isInitializing;
        set
        {
            if (_isInitializing != value)
            {
                _isInitializing = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion

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
