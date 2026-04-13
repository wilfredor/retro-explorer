using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace ex_plorer
{
    internal static class ClassicIcons
    {
        internal const string FolderClosedKey = "$dir";
        internal const string FolderOpenKey = "$dir_open";
        internal const string FileKey = "$file";
        internal const string AppKey = "$explorer";
        internal const string DesktopKey = "$desktop";
        internal const string MyComputerKey = "$mycomputer";
        internal const string FixedDriveKey = "$drive_fixed";
        internal const string RemovableDriveKey = "$drive_removable";
        internal const string CdDriveKey = "$drive_cd";
        internal const string NetworkDriveKey = "$drive_network";

        private static readonly Dictionary<string, Icon> Cache = new Dictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);
        private static readonly Assembly _assembly = typeof(ClassicIcons).Assembly;

        internal static Icon App
        {
            get { return Get("ex_plorer.Assets.Icons.classic_app.ico"); }
        }

        internal static Icon Desktop
        {
            get { return Get("ex_plorer.Assets.Icons.classic_desktop.ico"); }
        }

        internal static Icon MyComputer
        {
            get { return Get("ex_plorer.Assets.Icons.classic_mycomputer.ico"); }
        }

        internal static Icon FolderClosed
        {
            get { return Get("ex_plorer.Assets.Icons.classic_folder_closed.ico"); }
        }

        internal static Icon FolderOpen
        {
            get { return Get("ex_plorer.Assets.Icons.classic_folder_open.ico"); }
        }

        internal static Icon File
        {
            get { return Get("ex_plorer.Assets.Icons.classic_file.ico"); }
        }

        internal static Icon FixedDrive
        {
            get { return Get("ex_plorer.Assets.Icons.classic_drive_fixed.ico"); }
        }

        internal static Icon RemovableDrive
        {
            get { return Get("ex_plorer.Assets.Icons.classic_drive_removable.ico"); }
        }

        internal static Icon CdDrive
        {
            get { return Get("ex_plorer.Assets.Icons.classic_drive_cd.ico"); }
        }

        internal static Icon NetworkDrive
        {
            get { return Get("ex_plorer.Assets.Icons.classic_drive_network.ico"); }
        }

        internal static Icon GetDriveIcon(DriveType driveType)
        {
            switch (driveType)
            {
            case DriveType.Removable:
                return RemovableDrive;
            case DriveType.Network:
                return NetworkDrive;
            case DriveType.CDRom:
                return CdDrive;
            default:
                return FixedDrive;
            }
        }

        internal static Icon Sized(Icon icon, int size)
        {
            return new Icon(icon, size, size);
        }

        internal static Bitmap LoadBitmapFromEmbeddedIcon(string resourceName, int size)
        {
            Icon sized = Sized(Get(resourceName), size);
            try
            {
                return sized.ToBitmap();
            }
            finally
            {
                sized.Dispose();
            }
        }

        private static Icon Get(string resourceName)
        {
            Icon value;
            if (Cache.TryGetValue(resourceName, out value))
            {
                return value;
            }
            Stream stream = _assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new FileNotFoundException("Embedded icon resource was not found.", resourceName);
            }
            try
            {
                value = new Icon(stream);
                Cache[resourceName] = value;
                return value;
            }
            finally
            {
                stream.Dispose();
            }
        }
    }
}
