using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace ex_plorer;

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

	private static readonly Assembly Assembly = typeof(ClassicIcons).Assembly;

	internal static Icon App => Get("ex_plorer.Assets.Icons.classic_app.ico");

	internal static Icon Desktop => Get("ex_plorer.Assets.Icons.classic_desktop.ico");

	internal static Icon MyComputer => Get("ex_plorer.Assets.Icons.classic_mycomputer.ico");

	internal static Icon FolderClosed => Get("ex_plorer.Assets.Icons.classic_folder_closed.ico");

	internal static Icon FolderOpen => Get("ex_plorer.Assets.Icons.classic_folder_open.ico");

	internal static Icon File => Get("ex_plorer.Assets.Icons.classic_file.ico");

	internal static Icon FixedDrive => Get("ex_plorer.Assets.Icons.classic_drive_fixed.ico");

	internal static Icon RemovableDrive => Get("ex_plorer.Assets.Icons.classic_drive_removable.ico");

	internal static Icon CdDrive => Get("ex_plorer.Assets.Icons.classic_drive_cd.ico");

	internal static Icon NetworkDrive => Get("ex_plorer.Assets.Icons.classic_drive_network.ico");

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

	internal static Bitmap LoadBitmapFromEmbeddedIcon(string resourceName, int size = 16)
	{
		using (Icon icon = Sized(Get(resourceName), size))
		{
			return icon.ToBitmap();
		}
	}

	private static Icon Get(string resourceName)
	{
		if (Cache.TryGetValue(resourceName, out Icon value))
		{
			return value;
		}
		using (Stream stream = Assembly.GetManifestResourceStream(resourceName))
		{
			if (stream == null)
			{
				throw new FileNotFoundException("Embedded icon resource was not found.", resourceName);
			}
			value = new Icon(stream);
			Cache[resourceName] = value;
			return value;
		}
	}
}
