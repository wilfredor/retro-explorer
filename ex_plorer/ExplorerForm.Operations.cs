using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ex_plorer;

public partial class ExplorerForm
{
	private void TriggerNewFolder(object sender, EventArgs e)
	{
		try
		{
			string text = "New folder";
			string path = Path.Combine(CurrentPath, text);
			int num = 0;
			while (Directory.Exists(path))
			{
				num++;
				path = Path.Combine(CurrentPath, text + $" ({num})");
			}
			DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
			LoadCurrentDirectoryAsync();
			BeginInvoke((MethodInvoker)delegate
			{
				SelectItemByPath(directoryInfo.FullName, beginEdit: true);
			});
		}
		catch (Exception ex)
		{
			ShowOperationError("Unable to create the folder.", ex);
		}
	}

	private void TriggerCut(object sender, EventArgs e)
	{
		SetClipboardFromSelection(ClipboardFileOperation.Cut);
	}

	private void TriggerCopy(object sender, EventArgs e)
	{
		SetClipboardFromSelection(ClipboardFileOperation.Copy);
	}

	private void SetClipboardFromSelection(ClipboardFileOperation operation)
	{
		List<string> selectedPaths = GetSelectedPaths();
		if (selectedPaths.Count == 0)
		{
			return;
		}
		try
		{
			ClipboardHelper.SetPaths(selectedPaths, operation);
			UpdateClipboardDependentMenu(this, EventArgs.Empty);
		}
		catch (Exception ex)
		{
			ShowOperationError("Unable to copy the selected item(s).", ex);
		}
	}

	private void TriggerPaste(object sender, EventArgs e)
	{
		if (!ClipboardHelper.TryGetPaths(out List<string> paths, out ClipboardFileOperation operation))
		{
			return;
		}
		OperationResult operationResult = TransferItems(paths, CurrentPath, operation);
		if (operation == ClipboardFileOperation.Cut && !operationResult.HasErrors)
		{
			ClipboardHelper.ClearIfOwnedCutOperation();
		}
		LoadCurrentDirectoryAsync();
		ShowBatchResult(operation == ClipboardFileOperation.Cut ? "move" : "copy", operationResult);
	}

	private OperationResult TransferItems(IEnumerable<string> sourcePaths, string destinationDirectory, ClipboardFileOperation operation)
	{
		List<string> list = new List<string>();
		int num = 0;
		foreach (string item in sourcePaths.Where(static path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				string path = BuildDestinationPath(item, destinationDirectory);
				if (PathsEqual(item, path))
				{
					throw new IOException("Source and destination are the same.");
				}
				if (operation == ClipboardFileOperation.Cut)
				{
					MoveFileSystemItem(item, path);
				}
				else
				{
					CopyFileSystemItem(item, path);
				}
				num++;
			}
			catch (Exception ex)
			{
				list.Add(Path.GetFileName(item.TrimEnd('\\')) + ": " + GetFriendlyExceptionText(ex));
			}
		}
		return new OperationResult(num, list);
	}

	private static string BuildDestinationPath(string sourcePath, string destinationDirectory)
	{
		string fileName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		return Path.Combine(destinationDirectory, fileName);
	}

	private static void CopyFileSystemItem(string sourcePath, string destinationPath)
	{
		if (File.Exists(sourcePath))
		{
			new FileInfo(sourcePath).CopyTo(destinationPath, overwrite: false);
		}
		else if (Directory.Exists(sourcePath))
		{
			CopyDirectoryRecursive(sourcePath, destinationPath);
		}
		else
		{
			throw new FileNotFoundException("The source item no longer exists.", sourcePath);
		}
	}

	private static void MoveFileSystemItem(string sourcePath, string destinationPath)
	{
		if (File.Exists(sourcePath))
		{
			if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
			{
				throw new IOException("Destination already exists.");
			}
			try
			{
				File.Move(sourcePath, destinationPath);
			}
			catch (IOException)
			{
				File.Copy(sourcePath, destinationPath, overwrite: false);
				File.Delete(sourcePath);
			}
			return;
		}

		if (!Directory.Exists(sourcePath))
		{
			throw new DirectoryNotFoundException(sourcePath);
		}
		if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
		{
			throw new IOException("Destination already exists.");
		}
		try
		{
			Directory.Move(sourcePath, destinationPath);
		}
		catch (IOException)
		{
			CopyDirectoryRecursive(sourcePath, destinationPath);
			Directory.Delete(sourcePath, recursive: true);
		}
	}

	private static void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath)
	{
		if (File.Exists(destinationDirectoryPath) || Directory.Exists(destinationDirectoryPath))
		{
			throw new IOException("Destination already exists.");
		}
		Directory.CreateDirectory(destinationDirectoryPath);
		foreach (string item in Directory.GetDirectories(sourceDirectoryPath, "*", SearchOption.AllDirectories))
		{
			string text = item.Substring(sourceDirectoryPath.Length).TrimStart(Path.DirectorySeparatorChar);
			Directory.CreateDirectory(Path.Combine(destinationDirectoryPath, text));
		}
		foreach (string item2 in Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories))
		{
			string text2 = item2.Substring(sourceDirectoryPath.Length).TrimStart(Path.DirectorySeparatorChar);
			string path = Path.Combine(destinationDirectoryPath, text2);
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.Copy(item2, path, overwrite: false);
		}
	}

	private void TriggerDelete(object sender, EventArgs e)
	{
		List<string> selectedPaths = GetSelectedPaths();
		if (selectedPaths.Count == 0)
		{
			return;
		}
		bool flag = (ModifierKeys & Keys.Shift) == Keys.Shift;
		string text = (selectedPaths.Count == 1) ? Path.GetFileName(selectedPaths[0].TrimEnd('\\')) : $"{selectedPaths.Count} item(s)";
		string text2 = flag ? "permanently delete" : "send to the Recycle Bin";
		if (MessageBox.Show("Are you sure you want to " + text2 + " " + text + "?", "Delete File", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
		{
			return;
		}
		List<string> list = new List<string>();
		int num = 0;
		foreach (string selectedPath in selectedPaths)
		{
			try
			{
				if (flag)
				{
					if (File.Exists(selectedPath))
					{
						File.Delete(selectedPath);
					}
					else if (Directory.Exists(selectedPath))
					{
						Directory.Delete(selectedPath, recursive: true);
					}
				}
				else
				{
					ShellFileOperations.SendToRecycleBin(selectedPath);
				}
				num++;
			}
			catch (Exception ex)
			{
				list.Add(Path.GetFileName(selectedPath.TrimEnd('\\')) + ": " + GetFriendlyExceptionText(ex));
			}
		}
		LoadCurrentDirectoryAsync();
		ShowBatchResult("delete", new OperationResult(num, list));
	}

	private void TriggerRename(object sender, EventArgs e)
	{
		if (folderView.SelectedItems.Count == 1)
		{
			folderView.SelectedItems[0].BeginEdit();
		}
	}

	private void folderView_AfterLabelEdit(object sender, LabelEditEventArgs e)
	{
		if (e.Item < 0)
		{
			return;
		}
		ListViewItem listViewItem = folderView.Items[e.Item];
		if (string.IsNullOrWhiteSpace(e.Label))
		{
			e.CancelEdit = true;
			return;
		}
		try
		{
			FileSystemInfo fileSystemInfo = listViewItem.Tag as FileSystemInfo;
			if (fileSystemInfo is FileInfo fileInfo)
			{
				fileInfo.MoveTo(Path.Combine(CurrentPath, e.Label));
			}
			else if (fileSystemInfo is DirectoryInfo directoryInfo)
			{
				directoryInfo.MoveTo(Path.Combine(CurrentPath, e.Label));
			}
			LoadCurrentDirectoryAsync();
		}
		catch (Exception ex)
		{
			e.CancelEdit = true;
			ShowOperationError("Unable to rename the selected item.", ex);
			LoadCurrentDirectoryAsync();
		}
	}

	private void ShowProperties(object sender, EventArgs e)
	{
		List<string> selectedPaths = GetSelectedPaths();
		if (selectedPaths.Count == 0)
		{
			return;
		}
		if (selectedPaths.Count == 1)
		{
			try
			{
				ShellFileOperations.ShowProperties(selectedPaths[0]);
				return;
			}
			catch
			{
			}
		}
		ShowSelectionSummary(selectedPaths);
	}

	private void ShowSelectionSummary(List<string> selectedPaths)
	{
		int num = 0;
		int num2 = 0;
		long num3 = 0L;
		foreach (string selectedPath in selectedPaths)
		{
			if (Directory.Exists(selectedPath))
			{
				num++;
			}
			else if (File.Exists(selectedPath))
			{
				num2++;
				try
				{
					num3 += new FileInfo(selectedPath).Length;
				}
				catch
				{
				}
			}
		}
		MessageBox.Show($"Selection: {selectedPaths.Count} item(s)\nFolders: {num}\nFiles: {num2}\nSize: {num3.ReadableFileSize()}", "Properties", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private List<string> GetSelectedPaths()
	{
		List<string> list = new List<string>();
		foreach (ListViewItem selectedItem in folderView.SelectedItems)
		{
			if (selectedItem.Tag is FileSystemInfo fileSystemInfo)
			{
				list.Add(fileSystemInfo.FullName);
			}
		}
		return list;
	}

	private void SelectItemByPath(string path, bool beginEdit)
	{
		foreach (ListViewItem item in folderView.Items)
		{
			if (item.Tag is FileSystemInfo fileSystemInfo && PathsEqual(fileSystemInfo.FullName, path))
			{
				item.Selected = true;
				item.EnsureVisible();
				if (beginEdit)
				{
					item.BeginEdit();
				}
				break;
			}
		}
	}
}
