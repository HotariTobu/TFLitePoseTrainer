using System.Runtime.InteropServices;

namespace Assets.Interop
{
    [ComImport]
    [Guid("e6fdd21a-163f-4975-9c8c-a69f1ba37034")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFileDialogCustomize
    {
        void EnableOpenDropDown(int dwIDCtl);
    }
}
