using System.ComponentModel;

namespace TFLitePoseTrainer;

public abstract class SubWindow : System.Windows.Window
{
    private bool _canClose;

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

    public void ShowAndActivate()
    {
        Show();
        Activate();
    }

    public void CloseWithoutHiding()
    {
        _canClose = true;
        Close();
    }
}
