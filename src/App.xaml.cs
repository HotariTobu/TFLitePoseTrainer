﻿using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace TFLitePoseTrainer;

public partial class App : Application
{
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine(e.Exception);
        MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;

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
