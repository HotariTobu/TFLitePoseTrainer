using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TFLitePoseTrainer.Controls;

public class SkeletonViewer : Viewbox
{
    static SkeletonViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SkeletonViewer),
            new FrameworkPropertyMetadata(typeof(SkeletonViewer)));
    }

    public SkeletonViewer()
    {
    }
}
