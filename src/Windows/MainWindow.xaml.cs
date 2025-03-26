using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TFLitePoseTrainer.DataSources;

namespace TFLitePoseTrainer.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainDataSource _dataSource;

    public MainWindow()
    {
        InitializeComponent();
        _dataSource = (MainDataSource)DataContext;

        _dataSource.Poses.Add(new() { Data = new(""), Label = "Pose 1"});
        _dataSource.Poses.Add(new() { Data = new(""), Label = "Pose 2"});
        _dataSource.Poses.Add(new() { Data = new(""), Label = "Pose 3"});
    }
}
