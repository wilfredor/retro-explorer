using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer;

public partial class ExplorerForm
{
	private void ConfigureWatcher()
	{
		DisposeWatcher();
		try
		{
			currentWatcher = new FileSystemWatcher(CurrentPath)
			{
				IncludeSubdirectories = false,
				NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
			};
			currentWatcher.Created += Watcher_Changed;
			currentWatcher.Deleted += Watcher_Changed;
			currentWatcher.Changed += Watcher_Changed;
			currentWatcher.Renamed += Watcher_Changed;
			currentWatcher.EnableRaisingEvents = true;
		}
		catch
		{
		}
	}

	private void DisposeWatcher()
	{
		if (currentWatcher != null)
		{
			currentWatcher.EnableRaisingEvents = false;
			currentWatcher.Created -= Watcher_Changed;
			currentWatcher.Deleted -= Watcher_Changed;
			currentWatcher.Changed -= Watcher_Changed;
			currentWatcher.Renamed -= Watcher_Changed;
			currentWatcher.Dispose();
			currentWatcher = null;
		}
	}

	private void Watcher_Changed(object sender, FileSystemEventArgs e)
	{
		refreshQueued = true;
		if (watcherTimer != null && !base.IsDisposed)
		{
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					watcherTimer.Stop();
					watcherTimer.Start();
				});
			}
			catch
			{
			}
		}
	}

	private void watcherTimer_Tick(object sender, EventArgs e)
	{
		watcherTimer.Stop();
		if (refreshQueued && !base.IsDisposed)
		{
			refreshQueued = false;
			LoadCurrentDirectoryAsync();
		}
	}

	private void folderView_ItemDrag(object sender, ItemDragEventArgs e)
	{
		List<string> selectedPaths = GetSelectedPaths();
		if (selectedPaths.Count == 0)
		{
			return;
		}
		DataObject dataObject = new DataObject();
		System.Collections.Specialized.StringCollection stringCollection = new System.Collections.Specialized.StringCollection();
		foreach (string selectedPath in selectedPaths)
		{
			stringCollection.Add(selectedPath);
		}
		dataObject.SetFileDropList(stringCollection);
		folderView.DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Move);
	}

	private void DragTarget_DragEnter(object sender, DragEventArgs e)
	{
		e.Effect = GetDropEffect(e);
	}

	private void DragTarget_DragOver(object sender, DragEventArgs e)
	{
		e.Effect = GetDropEffect(e);
	}

	private void folderView_DragDrop(object sender, DragEventArgs e)
	{
		if (!TryGetDragDropPayload(e, out List<string> paths, out ClipboardFileOperation operation))
		{
			return;
		}
		OperationResult operationResult = TransferItems(paths, ResolveListViewDropTarget(e), operation);
		LoadCurrentDirectoryAsync();
		ShowBatchResult(operation == ClipboardFileOperation.Cut ? "move" : "copy", operationResult);
	}

	private void folderTree_DragDrop(object sender, DragEventArgs e)
	{
		if (!TryGetDragDropPayload(e, out List<string> paths, out ClipboardFileOperation operation))
		{
			return;
		}
		Point pt = folderTree.PointToClient(new Point(e.X, e.Y));
		TreeNode nodeAt = folderTree.GetNodeAt(pt);
		string destinationDirectory = ((nodeAt?.Tag as string) ?? CurrentPath);
		OperationResult operationResult = TransferItems(paths, destinationDirectory, operation);
		LoadCurrentDirectoryAsync();
		ShowBatchResult(operation == ClipboardFileOperation.Cut ? "move" : "copy", operationResult);
	}

	private string ResolveListViewDropTarget(DragEventArgs e)
	{
		Point pt = folderView.PointToClient(new Point(e.X, e.Y));
		ListViewHitTestInfo listViewHitTestInfo = folderView.HitTest(pt);
		if (listViewHitTestInfo.Item?.Tag is DirectoryInfo directoryInfo)
		{
			return directoryInfo.FullName;
		}
		return CurrentPath;
	}

	private static bool TryGetDragDropPayload(DragEventArgs e, out List<string> paths, out ClipboardFileOperation operation)
	{
		paths = new List<string>();
		operation = ((e.Effect == DragDropEffects.Move) ? ClipboardFileOperation.Cut : ClipboardFileOperation.Copy);
		if (!e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			return false;
		}
		string[] array = e.Data.GetData(DataFormats.FileDrop) as string[];
		if (array == null || array.Length == 0)
		{
			return false;
		}
		paths.AddRange(array);
		return true;
	}

	private static DragDropEffects GetDropEffect(DragEventArgs e)
	{
		if (!e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			return DragDropEffects.None;
		}
		return ((e.KeyState & 4) == 4) ? DragDropEffects.Move : DragDropEffects.Copy;
	}
}
