using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace ex_plorer;

internal class DirManager
{
	private static Dictionary<string, string> TypeNameDictionary { get; }

	internal static DriveInfo[] Drives => DriveInfo.GetDrives();

	internal string Path => CurrentDir.FullName;

	internal DirectoryInfo CurrentDir { get; private set; }

	internal static Dictionary<string, IconPair> IconDictionary { get; }

	internal ImageList LargeIcons { get; }

	internal ImageList SmallIcons { get; }

	internal List<string> IconsSet { get; }

	static DirManager()
	{
		IconDictionary = new Dictionary<string, IconPair>();
		TypeNameDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}

	internal DirManager(string path)
	{
		CurrentDir = new DirectoryInfo(path);
		IconsSet = new List<string>();
		LargeIcons = new ImageList
		{
			ImageSize = new Size(32, 32),
			ColorDepth = ColorDepth.Depth32Bit,
		};
		SmallIcons = new ImageList
		{
			ImageSize = new Size(16, 16),
			ColorDepth = ColorDepth.Depth32Bit,
		};
		AddStockIcon(ClassicIcons.FolderClosedKey, ClassicIcons.FolderClosed);
		AddStockIcon(ClassicIcons.FolderOpenKey, ClassicIcons.FolderOpen);
		AddStockIcon(ClassicIcons.FileKey, ClassicIcons.File);
		AddStockIcon(ClassicIcons.AppKey, ClassicIcons.App);
		AddStockIcon(ClassicIcons.DesktopKey, ClassicIcons.Desktop);
		AddStockIcon(ClassicIcons.MyComputerKey, ClassicIcons.MyComputer);
		AddStockIcon(ClassicIcons.FixedDriveKey, ClassicIcons.FixedDrive);
		AddStockIcon(ClassicIcons.RemovableDriveKey, ClassicIcons.RemovableDrive);
		AddStockIcon(ClassicIcons.CdDriveKey, ClassicIcons.CdDrive);
		AddStockIcon(ClassicIcons.NetworkDriveKey, ClassicIcons.NetworkDrive);
	}

	internal void NavigateTo(string path)
	{
		CurrentDir = new DirectoryInfo(path);
	}

	internal IEnumerable<ListViewItem> GetAllFiles()
	{
		return from arg in CurrentDir.EnumerateFileSystemInfos().Select(delegate(FileSystemInfo info)
			{
				ListViewItem item = null;
				bool isDirectory = false;
				try
				{
					if (info is FileInfo file)
					{
						item = GetFileItem(file);
					}
					else if (info is DirectoryInfo dir)
					{
						item = GetDirItem(dir);
						isDirectory = true;
					}
				}
				catch
				{
					item = null;
				}
				return new { isDirectory, item };
			})
			orderby arg.isDirectory descending
			where arg.item != null
			select arg.item;
	}

	internal ListViewItem GetFileItem(FileInfo file)
	{
		ListViewItem listViewItem = new ListViewItem(file.Name);
		listViewItem.SubItems.AddRange(new string[3]
		{
			file.Length.FileSizeInKB(),
			GetFileTypeName(file),
			file.LastWriteTime.ToString()
		});
		listViewItem.ImageKey = GetIconKey(file);
		listViewItem.Tag = file;
		return listViewItem;
	}

	internal ListViewItem GetDirItem(DirectoryInfo dir)
	{
		ListViewItem listViewItem = new ListViewItem(dir.Name);
		listViewItem.SubItems.AddRange(new string[3]
		{
			"",
			"Directory",
			dir.LastWriteTime.ToString()
		});
		listViewItem.ImageKey = ClassicIcons.FolderClosedKey;
		listViewItem.Tag = dir;
		return listViewItem;
	}

	internal string GetIconKey(FileInfo file)
	{
		string extension = file.Extension;
		string text;
		if (file.Extension == "")
		{
			text = ClassicIcons.FileKey;
		}
		else
		{
			switch (extension)
			{
			case ".exe":
			case ".lnk":
			case ".ico":
				text = file.Name;
				ExtractIcon(text, file.FullName);
				break;
			default:
				text = extension;
				if (!IconsSet.Contains(text))
				{
					ExtractIcon(text, file.FullName);
				}
				break;
			}
		}
		return text;
	}

	internal string GetFileTypeName(FileInfo file)
	{
		string iconKey = GetIconKey(file);
		if (TypeNameDictionary.TryGetValue(iconKey, out string value) && !string.IsNullOrWhiteSpace(value))
		{
			return value;
		}

		if (string.IsNullOrWhiteSpace(file.Extension))
		{
			return "File";
		}

		return file.Extension + " File";
	}

	private void ExtractIcon(string key, string path)
	{
		IconsSet.Add(key);
		Icon smallIcon;
		Icon largeIcon;
		if (IconDictionary.TryGetValue(key, out var value))
		{
			smallIcon = value.Small;
			largeIcon = value.Large;
		}
		else
		{
			string iconsAndTypeName = NativeIcon.GetIconsAndTypeName(path, out smallIcon, out largeIcon);
			IconDictionary.Add(key, new IconPair(smallIcon, largeIcon));
			TypeNameDictionary[key] = iconsAndTypeName;
		}
		if (!TypeNameDictionary.ContainsKey(key))
		{
			TypeNameDictionary[key] = string.Empty;
		}
		if (largeIcon != null)
		{
			LargeIcons.Images.Add(key, largeIcon);
		}
		if (smallIcon != null)
		{
			SmallIcons.Images.Add(key, smallIcon);
		}
	}

	private void AddStockIcon(string key, Icon icon)
	{
		LargeIcons.Images.Add(key, ClassicIcons.Sized(icon, 32));
		SmallIcons.Images.Add(key, ClassicIcons.Sized(icon, 16));
	}
}
