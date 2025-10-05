using System;
using System.Linq;
using System.Runtime.InteropServices;

using Assets.Interop;

#nullable enable

public class FileOpenDialog : IDisposable
{
    private readonly IFileOpenDialog _dialog;

    public FileOpenDialog()
    {
        _dialog = (IFileOpenDialog)new Assets.Interop.FileOpenDialog();
        _dialog.SetOptions(FileOpenOptions.FileMustExist | FileOpenOptions.NoChangeDir | FileOpenOptions.DontAddToRecent);

        var dialogCustomize = (IFileDialogCustomize)_dialog;
        dialogCustomize.EnableOpenDropDown(0);
    }

    public void Dispose()
    {
        Marshal.ReleaseComObject(_dialog);
    }

    public void SetFileTypes(FileType[] fileTypes)
    {
        var filterSpec = from fileType in fileTypes
                         select new COMDLG_FILTERSPEC { pszName = fileType.Name, pszSpec = fileType.Spec };
        _dialog.SetFileTypes((uint)fileTypes.Length, filterSpec.ToArray());
    }

    public string? Show()
    {
        if (_dialog.Show(IntPtr.Zero) != 0)
        {
            return null;
        }

        _dialog.GetResult(out var result);
        result.GetDisplayName(ShellItemDisplayName.FileSysPath, out var filePath);
        Marshal.ReleaseComObject(result);

        return string.IsNullOrEmpty(filePath) ? null : filePath;
    }

    public record FileType(string Name, string Spec);
}
