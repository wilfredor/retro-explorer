using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ex_plorer;

public partial class ExplorerForm
{
	private static bool PathsEqual(string first, string second)
	{
		if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
		{
			return false;
		}

		return string.Equals(NormalizePath(first), NormalizePath(second), StringComparison.OrdinalIgnoreCase);
	}

	private static string NormalizePath(string path)
	{
		string fullPath = Path.GetFullPath(path);
		if (fullPath.Length > 3)
		{
			fullPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		return fullPath;
	}

	private static string GetFriendlyExceptionText(Exception ex)
	{
		if (ex is UnauthorizedAccessException)
		{
			return "Access denied.";
		}

		if (ex is PathTooLongException)
		{
			return "The path is too long.";
		}

		if (ex is DirectoryNotFoundException)
		{
			return "The directory was not found.";
		}

		if (ex is FileNotFoundException)
		{
			return "The file was not found.";
		}

		if (ex is Win32Exception win32Exception && !string.IsNullOrWhiteSpace(win32Exception.Message))
		{
			return win32Exception.Message;
		}

		return string.IsNullOrWhiteSpace(ex.Message) ? "The operation could not be completed." : ex.Message;
	}

	private void ShowOperationError(string message, Exception ex)
	{
		MessageBox.Show(message + "\n\n" + GetFriendlyExceptionText(ex), "ex_plorer", MessageBoxButtons.OK, MessageBoxIcon.Hand);
	}

	private void ShowBatchResult(string operationName, OperationResult result)
	{
		if (!result.HasErrors)
		{
			return;
		}

		string text = $"Completed with {result.Errors.Count} error(s) while trying to {operationName} item(s).";
		if (result.SuccessCount > 0)
		{
			text = $"{char.ToUpperInvariant(operationName[0]) + operationName.Substring(1)} completed for {result.SuccessCount} item(s), but some failed.";
		}

		string text2 = string.Join("\n", result.Errors.Take(8).ToArray());
		if (result.Errors.Count > 8)
		{
			text2 += "\n...";
		}

		MessageBox.Show(text + "\n\n" + text2, "ex_plorer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void ApplySort()
	{
		folderView.ListViewItemSorter = new FileSystemItemComparer(sortColumn, sortOrder);
		folderView.Sort();
		UpdateArrangeMenuChecks();
	}

	private void UpdatePathChrome(string path)
	{
		string pathDisplayName = GetPathDisplayName(path);
		Text = "Exploring - " + path;
		UpdateAddressBarItems(path);
		locationPanel.Text = GetLocationStatusText(path);
		contentsHeaderLabel.Text = "Contents of " + pathDisplayName;
	}

	private void UpdateAddressBarItems(string currentPath)
	{
		addressBar.BeginUpdate();
		try
		{
			addressBar.Items.Clear();
			AddAddressBarItem(currentPath);
			foreach (DriveInfo drive in DirManager.Drives)
			{
				if (drive.IsReady)
				{
					AddAddressBarItem(drive.RootDirectory.FullName);
				}
			}
			foreach (string item in backHistory)
			{
				AddAddressBarItem(item);
			}
			int num = addressBar.Items.IndexOf(currentPath);
			addressBar.SelectedIndex = (num >= 0) ? num : 0;
		}
		finally
		{
			addressBar.EndUpdate();
		}
	}

	private void AddAddressBarItem(string path)
	{
		if (!string.IsNullOrWhiteSpace(path) && !addressBar.Items.Contains(path))
		{
			addressBar.Items.Add(path);
		}
	}

	private static string GetPathDisplayName(string path)
	{
		string fileName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		return string.IsNullOrEmpty(fileName) ? path : fileName;
	}

	private static string GetLocationStatusText(string path)
	{
		string text = TryGetDriveFreeSpaceText(path);
		return string.IsNullOrEmpty(text) ? path : path + "    " + text;
	}

	private static string TryGetDriveFreeSpaceText(string path)
	{
		try
		{
			string pathRoot = Path.GetPathRoot(path);
			if (string.IsNullOrEmpty(pathRoot))
			{
				return string.Empty;
			}
			DriveInfo driveInfo = new DriveInfo(pathRoot);
			if (!driveInfo.IsReady)
			{
				return string.Empty;
			}
			return driveInfo.AvailableFreeSpace.ReadableFileSize() + " free";
		}
		catch
		{
			return string.Empty;
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (!addressBar.ContainsFocus)
		{
			if (keyData == Keys.Back)
			{
				UpOneLevel(this, EventArgs.Empty);
				return true;
			}
			if (keyData == (Keys.Alt | Keys.Left))
			{
				NavigateBack(this, EventArgs.Empty);
				return true;
			}
			if (keyData == (Keys.Alt | Keys.Right))
			{
				NavigateForward(this, EventArgs.Empty);
				return true;
			}
		}
		if (keyData == (Keys.Alt | Keys.Return))
		{
			ShowProperties(this, EventArgs.Empty);
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void addressBar_DrawItem(object sender, DrawItemEventArgs e)
	{
		e.DrawBackground();
		if (e.Index < 0 || e.Index >= addressBar.Items.Count)
		{
			return;
		}
		string text = addressBar.Items[e.Index] as string;
		Image image = Manager.SmallIcons.Images[GetDriveIconKey(text)];
		int num = e.Bounds.Top + ((e.Bounds.Height - 16) / 2);
		if (image != null)
		{
			e.Graphics.DrawImage(image, e.Bounds.Left + 2, num, 16, 16);
		}
		TextRenderer.DrawText(e.Graphics, text, e.Font, new Rectangle(e.Bounds.Left + 22, e.Bounds.Top + 1, e.Bounds.Width - 24, e.Bounds.Height - 2), e.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
		e.DrawFocusRectangle();
	}
}
