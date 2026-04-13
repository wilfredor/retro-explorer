using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
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
                    path = Path.Combine(CurrentPath, text + " (" + num.ToString() + ")");
                }
                DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
                LoadCurrentDirectory();
                BeginInvoke((MethodInvoker)delegate
                {
                    SelectItemByPath(directoryInfo.FullName, true);
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
            List<string> paths;
            ClipboardFileOperation operation;
            if (!ClipboardHelper.TryGetPaths(out paths, out operation))
            {
                return;
            }
            OperationResult operationResult = TransferItems(paths, CurrentPath, operation);
            if (operation == ClipboardFileOperation.Cut && !operationResult.HasErrors)
            {
                ClipboardHelper.ClearIfOwnedCutOperation();
            }
            LoadCurrentDirectory();
            ShowBatchResult(operation == ClipboardFileOperation.Cut ? "move" : "copy", operationResult);
        }

        private OperationResult TransferItems(IEnumerable<string> sourcePaths, string destinationDirectory, ClipboardFileOperation operation)
        {
            List<string> list = new List<string>();
            int num = 0;
            Dictionary<string, bool> seen = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (string item in sourcePaths)
            {
                if (Utils.IsNullOrWhiteSpace(item)) continue;
                if (seen.ContainsKey(item)) continue;
                seen[item] = true;

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
                new FileInfo(sourcePath).CopyTo(destinationPath, false);
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
                    File.Copy(sourcePath, destinationPath, false);
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
                Directory.Delete(sourcePath, true);
            }
        }

        private static void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath)
        {
            if (File.Exists(destinationDirectoryPath) || Directory.Exists(destinationDirectoryPath))
            {
                throw new IOException("Destination already exists.");
            }
            Directory.CreateDirectory(destinationDirectoryPath);
            string[] dirs = Directory.GetDirectories(sourceDirectoryPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < dirs.Length; i++)
            {
                string text = dirs[i].Substring(sourceDirectoryPath.Length).TrimStart(Path.DirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(destinationDirectoryPath, text));
            }
            string[] files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string text2 = files[i].Substring(sourceDirectoryPath.Length).TrimStart(Path.DirectorySeparatorChar);
                string destPath = Path.Combine(destinationDirectoryPath, text2);
                string directoryName = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                File.Copy(files[i], destPath, false);
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
            string text;
            if (selectedPaths.Count == 1)
            {
                text = Path.GetFileName(selectedPaths[0].TrimEnd('\\'));
            }
            else
            {
                text = selectedPaths.Count.ToString() + " item(s)";
            }
            string text2 = flag ? "permanently delete" : "send to the Recycle Bin";
            if (MessageBox.Show("Are you sure you want to " + text2 + " " + text + "?", "Delete File", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
            {
                return;
            }
            List<string> errorList = new List<string>();
            int num = 0;
            for (int i = 0; i < selectedPaths.Count; i++)
            {
                try
                {
                    if (flag)
                    {
                        if (File.Exists(selectedPaths[i]))
                        {
                            File.Delete(selectedPaths[i]);
                        }
                        else if (Directory.Exists(selectedPaths[i]))
                        {
                            Directory.Delete(selectedPaths[i], true);
                        }
                    }
                    else
                    {
                        ShellFileOperations.SendToRecycleBin(selectedPaths[i]);
                    }
                    num++;
                }
                catch (Exception ex)
                {
                    errorList.Add(Path.GetFileName(selectedPaths[i].TrimEnd('\\')) + ": " + GetFriendlyExceptionText(ex));
                }
            }
            LoadCurrentDirectory();
            ShowBatchResult("delete", new OperationResult(num, errorList));
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
            if (Utils.IsNullOrWhiteSpace(e.Label))
            {
                e.CancelEdit = true;
                return;
            }
            try
            {
                FileSystemInfo fileSystemInfo = listViewItem.Tag as FileSystemInfo;
                FileInfo fileInfo = fileSystemInfo as FileInfo;
                DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
                if (fileInfo != null)
                {
                    fileInfo.MoveTo(Path.Combine(CurrentPath, e.Label));
                }
                else if (directoryInfo != null)
                {
                    directoryInfo.MoveTo(Path.Combine(CurrentPath, e.Label));
                }
                LoadCurrentDirectory();
            }
            catch (Exception ex)
            {
                e.CancelEdit = true;
                ShowOperationError("Unable to rename the selected item.", ex);
                LoadCurrentDirectory();
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
            for (int i = 0; i < selectedPaths.Count; i++)
            {
                if (Directory.Exists(selectedPaths[i]))
                {
                    num++;
                }
                else if (File.Exists(selectedPaths[i]))
                {
                    num2++;
                    try
                    {
                        num3 += new FileInfo(selectedPaths[i]).Length;
                    }
                    catch
                    {
                    }
                }
            }
            MessageBox.Show(
                "Selection: " + selectedPaths.Count.ToString() + " item(s)\n" +
                "Folders: " + num.ToString() + "\n" +
                "Files: " + num2.ToString() + "\n" +
                "Size: " + Utils.ReadableFileSize(num3),
                "Properties", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private List<string> GetSelectedPaths()
        {
            List<string> list = new List<string>();
            foreach (ListViewItem selectedItem in folderView.SelectedItems)
            {
                FileSystemInfo fileSystemInfo = selectedItem.Tag as FileSystemInfo;
                if (fileSystemInfo != null)
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
                FileSystemInfo fileSystemInfo = item.Tag as FileSystemInfo;
                if (fileSystemInfo != null && PathsEqual(fileSystemInfo.FullName, path))
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
}
