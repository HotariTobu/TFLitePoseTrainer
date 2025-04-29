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

    public IList<PoseItem> SelectedPoseItems { get; } = [];

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
}

internal class PoseListBoxSelectionBehavior : Behaviors.ListBoxSelectionBehavior<PoseItem> { }
