using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ex_plorer;

internal static class NativeIcon
{
	private struct SHFILEINFO
	{
		internal IntPtr hIcon;

		internal IntPtr iIcon;

		internal uint dwAttributes;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		internal string szDisplayName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		internal string szTypeName;
	}

	[Flags]
	private enum SHGFI : uint
	{
		ICON = 0x100u,
		DISPLAYNAME = 0x200u,
		TYPENAME = 0x400u,
		ATTRIBUTES = 0x800u,
		ICONLOCATION = 0x1000u,
		EXETYPE = 0x2000u,
		SYSICONINDEX = 0x4000u,
		LINKOVERLAY = 0x8000u,
		SELECTED = 0x10000u,
		ATTR_SPECIFIED = 0x20000u,
		LARGEICON = 0u,
		SMALLICON = 1u,
		OPENICON = 2u,
		SHELLICONSIZE = 4u,
		PIDL = 8u,
		USEFILEATTRIBUTES = 0x10u,
		ADDOVERLAYS = 0x20u,
		OVERLAYINDEX = 0x40u
	}

	internal static Icon GetSmallIcon(string path)
	{
		return GetIcon(path, SHGFI.ICON | SHGFI.SMALLICON);
	}

	internal static Icon GetLargeIcon(string path)
	{
		return GetIcon(path, SHGFI.ICON);
	}

	private static Icon GetIcon(string path, SHGFI flags)
	{
		SHFILEINFO sfi = default(SHFILEINFO);
		SHGetFileInfo(path, 0u, ref sfi, (uint)Marshal.SizeOf(sfi), flags | SHGFI.USEFILEATTRIBUTES);
		return CloneAndDestroyIcon(sfi.hIcon);
	}

	internal static string GetIconsAndTypeName(string path, out Icon smallIcon, out Icon largeIcon)
	{
		SHFILEINFO sfi = default(SHFILEINFO);
		SHGetFileInfo(path, 128u, ref sfi, (uint)Marshal.SizeOf(sfi), SHGFI.ICON | SHGFI.SMALLICON | SHGFI.USEFILEATTRIBUTES);
		smallIcon = CloneAndDestroyIcon(sfi.hIcon);
		SHGetFileInfo(path, 128u, ref sfi, (uint)Marshal.SizeOf(sfi), SHGFI.ICON | SHGFI.TYPENAME | SHGFI.USEFILEATTRIBUTES);
		largeIcon = CloneAndDestroyIcon(sfi.hIcon);
		return sfi.szTypeName;
	}

	private static Icon CloneAndDestroyIcon(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			return null;
		}

		Icon icon = (Icon)Icon.FromHandle(handle).Clone();
		DestroyIcon(handle);
		return icon;
	}

	[DllImport("shell32.dll")]
	private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO sfi, uint cbFileInfo, SHGFI uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool DestroyIcon(IntPtr hIcon);
}
