using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;

namespace ex_plorer;

internal enum ClipboardFileOperation
{
	Copy,
	Cut
}

internal static class ClipboardHelper
{
	private const string ClipboardOperationFormat = "ex_plorer.FileOperation";

	internal static void SetPaths(IEnumerable<string> paths, ClipboardFileOperation operation)
	{
		StringCollection stringCollection = new StringCollection();
		foreach (string path in paths.Where(static path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase))
		{
			stringCollection.Add(path);
		}

		DataObject dataObject = new DataObject();
		dataObject.SetFileDropList(stringCollection);
		dataObject.SetData(ClipboardOperationFormat, operation.ToString());
		Clipboard.SetDataObject(dataObject, copy: true);
	}

	internal static bool TryGetPaths(out List<string> paths, out ClipboardFileOperation operation)
	{
		paths = new List<string>();
		operation = ClipboardFileOperation.Copy;
		if (!Clipboard.ContainsFileDropList())
		{
			return false;
		}

		foreach (string item in Clipboard.GetFileDropList())
		{
			paths.Add(item);
		}

		object data = Clipboard.GetData(ClipboardOperationFormat);
		if (data is string text && string.Equals(text, ClipboardFileOperation.Cut.ToString(), StringComparison.OrdinalIgnoreCase))
		{
			operation = ClipboardFileOperation.Cut;
		}

		return paths.Count > 0;
	}

	internal static bool HasFileDropList()
	{
		return Clipboard.ContainsFileDropList();
	}

	internal static void ClearIfOwnedCutOperation()
	{
		object data = Clipboard.GetData(ClipboardOperationFormat);
		if (data is string text && string.Equals(text, ClipboardFileOperation.Cut.ToString(), StringComparison.OrdinalIgnoreCase))
		{
			Clipboard.Clear();
		}
	}
}
