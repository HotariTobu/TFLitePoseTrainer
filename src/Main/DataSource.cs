using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

    private bool _isEditingPoses;
    public bool IsEditingPoses
    {
        get => _isEditingPoses;
        set
        {
            if (_isEditingPoses != value)
            {
                _isEditingPoses = value;
                RaisePropertyChanged(nameof(IsEditingPoses));
            }
        }
    }

    #endregion

    #region == ModelItems ==

    public ObservableCollection<ModelItem> ModelItems { get; } = [];

    #endregion

    #region == SelectedModelItems ==

    public ObservableCollection<ModelItem> SelectedModelItems { get; } = [];

    #endregion

    #region == IsEditingModels ==

    private bool _isEditingModels;
    public bool IsEditingModels
    {
        get => _isEditingModels;
        set
        {
            if (_isEditingModels != value)
            {
                _isEditingModels = value;
                RaisePropertyChanged(nameof(IsEditingModels));
            }
        }
    }

    #endregion
}

internal class PoseListBoxSelectionBehavior : Behaviors.ListBoxSelectionBehavior<PoseItem> { }
internal class ModelListBoxSelectionBehavior : Behaviors.ListBoxSelectionBehavior<ModelItem> { }
