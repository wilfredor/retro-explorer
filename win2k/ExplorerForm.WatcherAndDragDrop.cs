using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
    public partial class ExplorerForm
    {
        private void ConfigureWatcher()
        {
            DisposeWatcher();
            try
            {
                currentWatcher = new FileSystemWatcher(CurrentPath);
                currentWatcher.IncludeSubdirectories = false;
                currentWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
                currentWatcher.Created += new FileSystemEventHandler(Watcher_Changed);
                currentWatcher.Deleted += new FileSystemEventHandler(Watcher_Changed);
                currentWatcher.Changed += new FileSystemEventHandler(Watcher_Changed);
                currentWatcher.Renamed += new RenamedEventHandler(Watcher_Renamed);
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
                currentWatcher.Created -= new FileSystemEventHandler(Watcher_Changed);
                currentWatcher.Deleted -= new FileSystemEventHandler(Watcher_Changed);
                currentWatcher.Changed -= new FileSystemEventHandler(Watcher_Changed);
                currentWatcher.Renamed -= new RenamedEventHandler(Watcher_Renamed);
                currentWatcher.Dispose();
                currentWatcher = null;
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            OnWatcherEvent();
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            OnWatcherEvent();
        }

        private void OnWatcherEvent()
        {
            refreshQueued = true;
            if (watcherTimer != null && !this.IsDisposed)
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
            if (refreshQueued && !this.IsDisposed)
            {
                refreshQueued = false;
                LoadCurrentDirectory();
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
            for (int i = 0; i < selectedPaths.Count; i++)
            {
                stringCollection.Add(selectedPaths[i]);
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
            List<string> paths;
            ClipboardFileOperation operation;
            if (!TryGetDragDropPayload(e, out paths, out operation))
            {
                return;
            }
            OperationResult operationResult = TransferItems(paths, ResolveListViewDropTarget(e), operation);
            LoadCurrentDirectory();
            ShowBatchResult(operation == ClipboardFileOperation.Cut ? "move" : "copy", operationResult);
        }

        private void folderTree_DragDrop(object sender, DragEventArgs e)
        {
            List<string> paths;
            ClipboardFileOperation operation;
            if (!TryGetDragDropPayload(e, out paths, out operation))
            {
                return;
            }
            Point pt = folderTree.PointToClient(new Point(e.X, e.Y));
            TreeNode nodeAt = folderTree.GetNodeAt(pt);
            string destinationDirectory;
            if (nodeAt != null)
            {
                string nodeTag = nodeAt.Tag as string;
                destinationDirectory = (nodeTag != null) ? nodeTag : CurrentPath;
            }
            else
            {
                destinationDirectory = CurrentPath;
            }
            OperationResult operationResult = TransferItems(paths, destinationDirectory, operation);
            LoadCurrentDirectory();
            ShowBatchResult(operation == ClipboardFileOperation.Cut ? "move" : "copy", operationResult);
        }

        private string ResolveListViewDropTarget(DragEventArgs e)
        {
            Point pt = folderView.PointToClient(new Point(e.X, e.Y));
            ListViewHitTestInfo listViewHitTestInfo = folderView.HitTest(pt);
            if (listViewHitTestInfo.Item != null)
            {
                DirectoryInfo directoryInfo = listViewHitTestInfo.Item.Tag as DirectoryInfo;
                if (directoryInfo != null)
                {
                    return directoryInfo.FullName;
                }
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
}
