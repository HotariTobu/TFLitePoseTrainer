using System.Data;
using System.Windows;
using System.Windows.Threading;

using TFLitePoseTrainer.Exceptions;

namespace TFLitePoseTrainer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SetupTrainer();
    }

    private static async void SetupTrainer()
    {
        var exception = await Trainer.Setup();
        if (exception is not null)
        {
            throw new Exception("Failed to setup trainer", exception);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine(e.Exception);
        MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;

        if (e.Exception is FatalException fatalException)
        {
            Shutdown(fatalException.ExitCode);
            return;
        }

        foreach (var window in Windows.Cast<Window>())
        {
            if (window?.IsVisible == true)
            {
                return;
            }
        }

        Shutdown(1);
    }
}
