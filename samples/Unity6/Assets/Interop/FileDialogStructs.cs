using System;
using System.Runtime.InteropServices;

namespace Assets.Interop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct COMDLG_FILTERSPEC
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszSpec;
    }

    [Flags]
    internal enum FileOpenOptions : uint
    {
        NoChangeDir = 0x00000008,
        FileMustExist = 0x00001000,
        DontAddToRecent = 0x02000000
    }
}
