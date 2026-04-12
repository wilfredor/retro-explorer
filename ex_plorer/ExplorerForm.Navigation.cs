using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_plorer;

public partial class ExplorerForm
{
	private async void LoadCurrentDirectoryAsync()
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
			ListViewItem[] array = await Task.Run((Func<ListViewItem[]>)Manager.GetAllFiles().ToArray);
			if (base.IsDisposed || num != loadVersion || !PathsEqual(CurrentPath, currentPath))
			{
				return;
			}
			folderView.BeginUpdate();
			folderView.Items.Clear();
			folderView.Items.AddRange(array);
			ApplySort();
			folderView.EndUpdate();
			itemsCount.Text = $"{folderView.Items.Count} object(s)";
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
		LoadCurrentDirectoryAsync();
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
		bool flag3 = Manager?.CurrentDir?.Parent != null;
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
			NavigateTo(Manager.CurrentDir.Parent.FullName, recordHistory: true);
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
		using (GotoForm gotoForm = new GotoForm(CurrentPath))
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
					NavigateTo(gotoForm.Result, recordHistory: true);
				}
			}
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
			if (fileSystemInfo is DirectoryInfo directoryInfo)
			{
				NavigateTo(directoryInfo.FullName, recordHistory: true);
			}
			else if (fileSystemInfo is FileInfo fileInfo)
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
		NavigateTo(addressBar.Text, recordHistory: true);
	}

	private void addressBar_SelectionChangeCommitted(object sender, EventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(addressBar.Text))
		{
			NavigateTo(addressBar.Text, recordHistory: true);
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
		if (folderView.SelectedItems.Count == 1 && folderView.SelectedItems[0].Tag is FileSystemInfo fileSystemInfo)
		{
			if (fileSystemInfo is FileInfo fileInfo)
			{
				locationPanel.Text = fileInfo.FullName + "    " + fileInfo.Length.FileSizeInKB();
			}
			else
			{
				locationPanel.Text = fileSystemInfo.FullName;
			}
			itemsCount.Text = "1 object selected";
		}
		else if (folderView.SelectedItems.Count > 1)
		{
			long num = 0L;
			foreach (ListViewItem selectedItem in folderView.SelectedItems)
			{
				if (selectedItem.Tag is FileInfo fileInfo2)
				{
					num += fileInfo2.Length;
				}
			}

			itemsCount.Text = folderView.SelectedItems.Count + " object(s) selected";
			locationPanel.Text = (num > 0) ? ("Total size: " + num.ReadableFileSize()) : CurrentPath;
		}
		else
		{
			itemsCount.Text = folderView.Items.Count + " object(s)";
			locationPanel.Text = GetLocationStatusText(CurrentPath);
		}
	}

	private void ExplorerForm_Activated(object sender, EventArgs e)
	{
		ReloadDriveTree();
	}
}
