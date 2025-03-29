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

    #region == IsEditing ==

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing != value)
            {
                _isEditing = value;
                RaisePropertyChanged(nameof(IsEditing));
            }
        }
    }

    #endregion
}
