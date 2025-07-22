using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SharedWPF;

/*
 Refer to https://qiita.com/tricogimmick/items/f07ef53dea817d198475
 */
abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    #region == INotifyPropertyChanged ==

    #region == PropertyChanged ==

    event PropertyChangedEventHandler? PropertyChanged;

    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
    }

    #endregion

    protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region == INotifyDataErrorInfo ==

    readonly Dictionary<string, List<object?>> _errors = [];

    bool INotifyDataErrorInfo.HasErrors => _errors.Count > 0;

    #region == ErrorsChanged ==

    event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    event EventHandler<DataErrorsChangedEventArgs>? INotifyDataErrorInfo.ErrorsChanged
    {
        add => ErrorsChanged += value;
        remove => ErrorsChanged -= value;
    }

    #endregion

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
        if (propertyName is not null && _errors.TryGetValue(propertyName, out var errors))
        {
            return errors;
        }
        else
        {
            return Enumerable.Empty<object>();
        }
    }

    protected void AddError(object? error = null, [CallerMemberName] string propertyName = "")
    {
        if (_errors.TryGetValue(propertyName, out var errors))
        {
            errors.Add(error);
        }
        else
        {
            _errors.Add(propertyName, [error]);
        }

        RaiseErrorsChanged(propertyName);
    }

    protected void ClearErrors([CallerMemberName] string propertyName = "")
    {
        _errors.Remove(propertyName);
        RaiseErrorsChanged(propertyName);
    }

    void RaiseErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    #endregion

    #region == ICommand Helper ==

    protected class DelegateCommand(Action<object?> command, Func<object?, bool>? canExecute = null) : ICommand
    {
        readonly Action<object?> _command = command;
        readonly Func<object?, bool>? _canExecute = canExecute;

        void ICommand.Execute(object? parameter)
        {
            _command(parameter);
        }

        bool ICommand.CanExecute(object? parameter)
        {
            return _canExecute is null || _canExecute(parameter);
        }

        event EventHandler? ICommand.CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    #endregion
}
