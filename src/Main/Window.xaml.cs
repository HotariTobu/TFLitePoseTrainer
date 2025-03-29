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

namespace TFLitePoseTrainer.Main;

public partial class Window : System.Windows.Window
{
    private readonly DataSource _dataSource;

    public Window()
    {
        InitializeComponent();
        _dataSource = (DataSource)DataContext;

        _dataSource.Poses.Add(new() { Data = new(""), Label = "Pose 1"});
        _dataSource.Poses.Add(new() { Data = new(""), Label = "Pose 2"});
        _dataSource.Poses.Add(new() { Data = new(""), Label = "Pose 3"});
    }
}
