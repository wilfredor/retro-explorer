using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ex_plorer
{
    internal static class ShellFileOperations
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            internal IntPtr hwnd;
            internal uint wFunc;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pFrom;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pTo;

            internal ushort fFlags;

            [MarshalAs(UnmanagedType.Bool)]
            internal bool fAnyOperationsAborted;

            internal IntPtr hNameMappings;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string lpszProgressTitle;
        }

        private const uint FO_DELETE = 3u;
        private const ushort FOF_ALLOWUNDO = 64;
        private const ushort FOF_NOCONFIRMATION = 16;
        private const ushort FOF_SILENT = 4;
        private const ushort FOF_NOERRORUI = 1024;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        internal static void OpenWithShell(string path)
        {
            ProcessStartInfo psi = new ProcessStartInfo(path);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        internal static void ShowProperties(string path)
        {
            ProcessStartInfo psi = new ProcessStartInfo(path);
            psi.UseShellExecute = true;
            psi.Verb = "properties";
            Process.Start(psi);
        }

        internal static void SendToRecycleBin(string path)
        {
            string text = path + '\0' + '\0';
            SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT();
            lpFileOp.wFunc = FO_DELETE;
            lpFileOp.pFrom = text;
            lpFileOp.fFlags = (ushort)(FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT | FOF_NOERRORUI);
            int num = SHFileOperation(ref lpFileOp);
            if (num != 0 || lpFileOp.fAnyOperationsAborted)
            {
                throw new InvalidOperationException("The shell delete operation did not complete successfully.");
            }
        }
    }
}
