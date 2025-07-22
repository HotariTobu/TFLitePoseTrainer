using System.ComponentModel;

namespace TFLitePoseTrainer;

public abstract class SubWindow : System.Windows.Window
{
    bool _canClose;

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (_canClose)
        {
            return;
        }

        e.Cancel = true;
        Hide();
    }

    internal void ShowAndActivate()
    {
        Show();
        Activate();
    }

    internal void CloseWithoutHiding()
    {
        _canClose = true;
        Close();
    }
}
