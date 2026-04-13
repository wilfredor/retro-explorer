using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
    internal class DirManager
    {
        private static Dictionary<string, string> _typeNameDictionary;
        private static Dictionary<string, IconPair> _iconDictionary;
        private DirectoryInfo _currentDir;
        private ImageList _largeIcons;
        private ImageList _smallIcons;
        private List<string> _iconsSet;

        internal static DriveInfo[] Drives
        {
            get { return DriveInfo.GetDrives(); }
        }

        internal string Path
        {
            get { return _currentDir.FullName; }
        }

        internal DirectoryInfo CurrentDir
        {
            get { return _currentDir; }
        }

        internal static Dictionary<string, IconPair> IconDictionary
        {
            get { return _iconDictionary; }
        }

        internal ImageList LargeIcons
        {
            get { return _largeIcons; }
        }

        internal ImageList SmallIcons
        {
            get { return _smallIcons; }
        }

        internal List<string> IconsSet
        {
            get { return _iconsSet; }
        }

        static DirManager()
        {
            _iconDictionary = new Dictionary<string, IconPair>();
            _typeNameDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        internal DirManager(string path)
        {
            _currentDir = new DirectoryInfo(path);
            _iconsSet = new List<string>();
            _largeIcons = new ImageList();
            _largeIcons.ImageSize = new Size(32, 32);
            _largeIcons.ColorDepth = ColorDepth.Depth32Bit;
            _smallIcons = new ImageList();
            _smallIcons.ImageSize = new Size(16, 16);
            _smallIcons.ColorDepth = ColorDepth.Depth32Bit;
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
            _currentDir = new DirectoryInfo(path);
        }

        internal List<ListViewItem> GetAllFiles()
        {
            List<ListViewItem> dirs = new List<ListViewItem>();
            List<ListViewItem> files = new List<ListViewItem>();
            FileSystemInfo[] infos;
            try
            {
                infos = _currentDir.GetFileSystemInfos();
            }
            catch
            {
                return new List<ListViewItem>();
            }
            for (int i = 0; i < infos.Length; i++)
            {
                FileSystemInfo info = infos[i];
                try
                {
                    FileInfo file = info as FileInfo;
                    DirectoryInfo dir = info as DirectoryInfo;
                    if (file != null)
                    {
                        ListViewItem item = GetFileItem(file);
                        if (item != null) files.Add(item);
                    }
                    else if (dir != null)
                    {
                        ListViewItem item = GetDirItem(dir);
                        if (item != null) dirs.Add(item);
                    }
                }
                catch
                {
                }
            }
            List<ListViewItem> result = new List<ListViewItem>(dirs.Count + files.Count);
            result.AddRange(dirs);
            result.AddRange(files);
            return result;
        }

        internal ListViewItem GetFileItem(FileInfo file)
        {
            ListViewItem listViewItem = new ListViewItem(file.Name);
            listViewItem.SubItems.AddRange(new string[3]
            {
                Utils.FileSizeInKB(file.Length),
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
                    if (!_iconsSet.Contains(text))
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
            string value;
            if (_typeNameDictionary.TryGetValue(iconKey, out value) && !Utils.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (Utils.IsNullOrWhiteSpace(file.Extension))
            {
                return "File";
            }

            return file.Extension + " File";
        }

        private void ExtractIcon(string key, string path)
        {
            _iconsSet.Add(key);
            Icon smallIcon;
            Icon largeIcon;
            IconPair value;
            if (_iconDictionary.TryGetValue(key, out value))
            {
                smallIcon = value.Small;
                largeIcon = value.Large;
            }
            else
            {
                string iconsAndTypeName = NativeIcon.GetIconsAndTypeName(path, out smallIcon, out largeIcon);
                _iconDictionary.Add(key, new IconPair(smallIcon, largeIcon));
                _typeNameDictionary[key] = iconsAndTypeName;
            }
            if (!_typeNameDictionary.ContainsKey(key))
            {
                _typeNameDictionary[key] = string.Empty;
            }
            if (largeIcon != null)
            {
                _largeIcons.Images.Add(key, largeIcon);
            }
            if (smallIcon != null)
            {
                _smallIcons.Images.Add(key, smallIcon);
            }
        }

        private void AddStockIcon(string key, Icon icon)
        {
            _largeIcons.Images.Add(key, ClassicIcons.Sized(icon, 32));
            _smallIcons.Images.Add(key, ClassicIcons.Sized(icon, 16));
        }
    }
}
