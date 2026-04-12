using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer;

internal sealed class FileSystemItemComparer : IComparer
{
	private readonly int column;

	private readonly SortOrder sortOrder;

	internal FileSystemItemComparer(int column, SortOrder sortOrder)
	{
		this.column = column;
		this.sortOrder = sortOrder;
	}

	public int Compare(object x, object y)
	{
		ListViewItem listViewItem = x as ListViewItem;
		ListViewItem listViewItem2 = y as ListViewItem;
		if (listViewItem == null || listViewItem2 == null)
		{
			return 0;
		}

		FileSystemInfo fileSystemInfo = listViewItem.Tag as FileSystemInfo;
		FileSystemInfo fileSystemInfo2 = listViewItem2.Tag as FileSystemInfo;
		int num = CompareDirectoryGroup(fileSystemInfo, fileSystemInfo2);
		if (num != 0)
		{
			return num;
		}

		switch (column)
		{
		case 1:
			num = CompareFileSize(fileSystemInfo, fileSystemInfo2);
			break;
		case 2:
			num = string.Compare(listViewItem.SubItems[column].Text, listViewItem2.SubItems[column].Text, StringComparison.OrdinalIgnoreCase);
			break;
		case 3:
			num = DateTime.Compare(fileSystemInfo.LastWriteTime, fileSystemInfo2.LastWriteTime);
			break;
		default:
			num = string.Compare(listViewItem.Text, listViewItem2.Text, StringComparison.OrdinalIgnoreCase);
			break;
		}

		if (sortOrder == SortOrder.Descending)
		{
			num = -num;
		}

		return num;
	}

	private static int CompareDirectoryGroup(FileSystemInfo first, FileSystemInfo second)
	{
		bool flag = first is DirectoryInfo;
		bool flag2 = second is DirectoryInfo;
		if (flag == flag2)
		{
			return 0;
		}

		return flag ? -1 : 1;
	}

	private static int CompareFileSize(FileSystemInfo first, FileSystemInfo second)
	{
		long num = ((first as FileInfo)?.Length).GetValueOrDefault();
		long num2 = ((second as FileInfo)?.Length).GetValueOrDefault();
		return num.CompareTo(num2);
	}
}
