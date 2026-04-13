using System;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
    public partial class ExplorerForm
    {
        private void LoadCurrentDirectory()
        {
            int num = ++loadVersion;
            string currentPath = CurrentPath;
            itemsCount.Text = "Please wait...";
            locationPanel.Text = currentPath;
            contentsHeaderLabel.Text = "Contents of " + currentPath;
            folderView.BeginUpdate();
            folderView.Items.Clear();
            folderView.EndUpdate();
            try
            {
                System.Collections.Generic.List<ListViewItem> list = Manager.GetAllFiles();
                ListViewItem[] array = list.ToArray();
                if (this.IsDisposed || num != loadVersion || !PathsEqual(CurrentPath, currentPath))
                {
                    return;
                }
                folderView.BeginUpdate();
                folderView.Items.Clear();
                folderView.Items.AddRange(array);
                ApplySort();
                folderView.EndUpdate();
                itemsCount.Text = folderView.Items.Count.ToString() + " object(s)";
            }
            catch (Exception ex)
            {
                itemsCount.Text = "Unable to load folder";
                ShowOperationError("Unable to read this folder.", ex);
            }
        }

        private void NavigateTo(string path, bool recordHistory)
        {
            try
            {
                string fullPath = Path.GetFullPath(path);
                if (recordHistory && !PathsEqual(CurrentPath, fullPath))
                {
                    backHistory.Push(CurrentPath);
                    forwardHistory.Clear();
                }
                NavigateToInternal(fullPath);
            }
            catch (Exception ex)
            {
                ShowOperationError("Unable to open this folder.", ex);
            }
        }

        private void NavigateToInternal(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(path);
            }
            Manager.NavigateTo(path);
            UpdatePathChrome(path);
            UpdateNavigationControls();
            ReloadDriveTree();
            SelectTreeNodeForPath(path);
            ConfigureWatcher();
            LoadCurrentDirectory();
        }

        private void NavigateBack(object sender, EventArgs e)
        {
            if (backHistory.Count == 0)
            {
                return;
            }
            try
            {
                string currentPath = CurrentPath;
                string path = backHistory.Pop();
                forwardHistory.Push(currentPath);
                NavigateToInternal(path);
            }
            catch (Exception ex)
            {
                ShowOperationError("Unable to go back.", ex);
            }
        }

        private void NavigateForward(object sender, EventArgs e)
        {
            if (forwardHistory.Count == 0)
            {
                return;
            }
            try
            {
                string currentPath = CurrentPath;
                string path = forwardHistory.Pop();
                backHistory.Push(currentPath);
                NavigateToInternal(path);
            }
            catch (Exception ex)
            {
                ShowOperationError("Unable to go forward.", ex);
            }
        }

        private void UpdateNavigationControls()
        {
            bool flag = backHistory.Count > 0;
            bool flag2 = forwardHistory.Count > 0;
            bool flag3 = Manager != null && Manager.CurrentDir != null && Manager.CurrentDir.Parent != null;
            backButton.Enabled = flag;
            forwardButton.Enabled = flag2;
            upButton.Enabled = flag3;
            backMenuItem.Enabled = flag;
            forwardMenuItem.Enabled = flag2;
            upMenuItem.Enabled = flag3;
        }

        private void UpOneLevel(object sender, EventArgs e)
        {
            if (Manager.CurrentDir.Parent != null)
            {
                NavigateTo(Manager.CurrentDir.Parent.FullName, true);
            }
        }

        private void OpenCurrentFolderInNewWindow(object sender, EventArgs e)
        {
            try
            {
                ExplorerForm explorerForm = new ExplorerForm(CurrentPath, statusBar.Visible, folderView.View);
                explorerForm.Show();
            }
            catch (Exception ex)
            {
                ShowOperationError("Unable to open a new window.", ex);
            }
        }

        private void GoToPrompt(object sender, EventArgs e)
        {
            GotoForm gotoForm = new GotoForm(CurrentPath);
            try
            {
                gotoForm.ShowDialog(this);
                if (gotoForm.DialogResult == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(gotoForm.Result))
                    {
                        MessageBox.Show("Invalid path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        NavigateTo(gotoForm.Result, true);
                    }
                }
            }
            finally
            {
                gotoForm.Dispose();
            }
        }

        private void OpenSelectedItem(object sender, EventArgs e)
        {
            if (folderView.SelectedItems.Count == 0)
            {
                return;
            }
            try
            {
                FileSystemInfo fileSystemInfo = folderView.SelectedItems[0].Tag as FileSystemInfo;
                DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
                FileInfo fileInfo = fileSystemInfo as FileInfo;
                if (directoryInfo != null)
                {
                    NavigateTo(directoryInfo.FullName, true);
                }
                else if (fileInfo != null)
                {
                    ShellFileOperations.OpenWithShell(fileInfo.FullName);
                }
            }
            catch (Exception ex)
            {
                ShowOperationError("Unable to open the selected item.", ex);
            }
        }

        private void folderView_ItemActivate(object sender, EventArgs e)
        {
            OpenSelectedItem(sender, e);
        }

        private void NavigationBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            if (e.Button == backButton)
            {
                NavigateBack(sender, EventArgs.Empty);
            }
            else if (e.Button == forwardButton)
            {
                NavigateForward(sender, EventArgs.Empty);
            }
            else if (e.Button == upButton)
            {
                UpOneLevel(sender, EventArgs.Empty);
            }
            else if (e.Button == cutButton)
            {
                TriggerCut(sender, EventArgs.Empty);
            }
            else if (e.Button == copyButton)
            {
                TriggerCopy(sender, EventArgs.Empty);
            }
            else if (e.Button == pasteButton)
            {
                TriggerPaste(sender, EventArgs.Empty);
            }
            else if (e.Button == deleteButton)
            {
                TriggerDelete(sender, EventArgs.Empty);
            }
            else if (e.Button == propertiesButton)
            {
                ShowProperties(sender, EventArgs.Empty);
            }
            else if (e.Button == largeIconsButton)
            {
                folderView.View = View.LargeIcon;
                UpdateViewMenuChecks(View.LargeIcon);
            }
            else if (e.Button == smallIconsButton)
            {
                folderView.View = View.SmallIcon;
                UpdateViewMenuChecks(View.SmallIcon);
            }
            else if (e.Button == listButton)
            {
                folderView.View = View.List;
                UpdateViewMenuChecks(View.List);
            }
            else if (e.Button == detailsButton)
            {
                folderView.View = View.Details;
                UpdateViewMenuChecks(View.Details);
            }
        }

        private void CycleFolderViewMode()
        {
            View view = folderView.View;
            if (view == View.LargeIcon)
            {
                folderView.View = View.SmallIcon;
                UpdateViewMenuChecks(View.SmallIcon);
            }
            else if (view == View.SmallIcon)
            {
                folderView.View = View.List;
                UpdateViewMenuChecks(View.List);
            }
            else if (view == View.List)
            {
                folderView.View = View.Details;
                UpdateViewMenuChecks(View.Details);
            }
            else
            {
                folderView.View = View.LargeIcon;
                UpdateViewMenuChecks(View.LargeIcon);
            }
        }

        private void addressGoButton_Click(object sender, EventArgs e)
        {
            NavigateTo(addressBar.Text, true);
        }

        private void addressBar_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (!Utils.IsNullOrWhiteSpace(addressBar.Text))
            {
                NavigateTo(addressBar.Text, true);
            }
        }

        private void addressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                addressGoButton_Click(sender, EventArgs.Empty);
            }
        }

        private void ExplorerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisposeWatcher();
            if (Application.OpenForms.Count == 0)
            {
                Application.Exit();
            }
        }

        private void UpdateStatusForSelection()
        {
            if (folderView.SelectedItems.Count == 1)
            {
                FileSystemInfo fileSystemInfo = folderView.SelectedItems[0].Tag as FileSystemInfo;
                if (fileSystemInfo != null)
                {
                    FileInfo fileInfo = fileSystemInfo as FileInfo;
                    if (fileInfo != null)
                    {
                        locationPanel.Text = fileInfo.FullName + "    " + Utils.FileSizeInKB(fileInfo.Length);
                    }
                    else
                    {
                        locationPanel.Text = fileSystemInfo.FullName;
                    }
                    itemsCount.Text = "1 object selected";
                    return;
                }
            }

            if (folderView.SelectedItems.Count > 1)
            {
                long num = 0L;
                foreach (ListViewItem selectedItem in folderView.SelectedItems)
                {
                    FileInfo fileInfo2 = selectedItem.Tag as FileInfo;
                    if (fileInfo2 != null)
                    {
                        num += fileInfo2.Length;
                    }
                }

                itemsCount.Text = folderView.SelectedItems.Count.ToString() + " object(s) selected";
                locationPanel.Text = (num > 0) ? ("Total size: " + Utils.ReadableFileSize(num)) : CurrentPath;
            }
            else
            {
                itemsCount.Text = folderView.Items.Count.ToString() + " object(s)";
                locationPanel.Text = GetLocationStatusText(CurrentPath);
            }
        }

        private void ExplorerForm_Activated(object sender, EventArgs e)
        {
            ReloadDriveTree();
        }
    }
}
