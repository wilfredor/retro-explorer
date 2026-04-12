using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ex_plorer;

public partial class ExplorerForm
{
	private void ReloadDriveTree()
	{
		string currentPath = CurrentPath;
		string pathRoot = Path.GetPathRoot(currentPath);
		folderTree.BeginUpdate();
		try
		{
			folderTree.Nodes.Clear();
			if (string.IsNullOrEmpty(pathRoot))
			{
				return;
			}
			TreeNode treeNode = CreateDirectoryNode(pathRoot, GetTreeRootDisplayName(pathRoot));
			string driveIconKey = GetDriveIconKey(pathRoot);
			treeNode.ImageKey = driveIconKey;
			treeNode.SelectedImageKey = driveIconKey;
			treeNode.Expand();
			folderTree.Nodes.Add(treeNode);
		}
		finally
		{
			folderTree.EndUpdate();
		}
		SelectTreeNodeForPath(currentPath);
	}

	private TreeNode CreateDirectoryNode(string path, string text = null)
	{
		TreeNode treeNode = new TreeNode(text ?? System.IO.Path.GetFileName(path.TrimEnd('\\')));
		treeNode.Tag = path;
		if (IsDriveRootPath(path))
		{
			string driveIconKey = GetDriveIconKey(path);
			treeNode.ImageKey = driveIconKey;
			treeNode.SelectedImageKey = driveIconKey;
		}
		else
		{
			treeNode.ImageKey = ClassicIcons.FolderClosedKey;
			treeNode.SelectedImageKey = ClassicIcons.FolderOpenKey;
		}
		if (DirectoryHasChildren(path))
		{
			treeNode.Nodes.Add(new TreeNode());
		}
		return treeNode;
	}

	private void folderTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
	{
		SetExpandedNodeIcon(e.Node, expanded: true);
		PopulateTreeNode(e.Node);
	}

	private void folderTree_AfterCollapse(object sender, TreeViewEventArgs e)
	{
		SetExpandedNodeIcon(e.Node, expanded: false);
	}

	private void folderTree_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (!suppressTreeSelection && e.Node?.Tag is string text)
		{
			NavigateTo(text, recordHistory: true);
		}
	}

	private void folderTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
	{
		folderTree.SelectedNode = e.Node;
	}

	private void PopulateTreeNode(TreeNode node)
	{
		if (node == null || node.Tag == null)
		{
			return;
		}
		if (node.Nodes.Count != 1 || node.Nodes[0].Tag != null)
		{
			return;
		}
		node.Nodes.Clear();
		try
		{
			foreach (DirectoryInfo item in new DirectoryInfo((string)node.Tag).EnumerateDirectories().OrderBy(static dir => dir.Name, StringComparer.OrdinalIgnoreCase))
			{
				try
				{
					node.Nodes.Add(CreateDirectoryNode(item.FullName));
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private void SelectTreeNodeForPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		string pathRoot = Path.GetPathRoot(path);
		TreeNode treeNode = (folderTree.Nodes.Count > 0) ? folderTree.Nodes[0] : null;
		if (treeNode == null || !PathsEqual((string)treeNode.Tag, pathRoot))
		{
			return;
		}
		treeNode.Expand();
		suppressTreeSelection = true;
		try
		{
			treeNode.Expand();
			if (!PathsEqual(pathRoot, path))
			{
				string[] array = path.Substring(pathRoot.Length).Split(new char[2] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
				string text = pathRoot;
				foreach (string path2 in array)
				{
					PopulateTreeNode(treeNode);
					text = Path.Combine(text, path2);
					TreeNode treeNode2 = null;
					foreach (TreeNode node2 in treeNode.Nodes)
					{
						if (PathsEqual((string)node2.Tag, text))
						{
							treeNode2 = node2;
							break;
						}
					}
					if (treeNode2 == null)
					{
						break;
					}
					treeNode = treeNode2;
					treeNode.Expand();
				}
			}
			folderTree.SelectedNode = treeNode;
			treeNode.EnsureVisible();
		}
		finally
		{
			suppressTreeSelection = false;
		}
	}

	private static bool DirectoryHasChildren(string path)
	{
		try
		{
			return Directory.EnumerateDirectories(path).Any();
		}
		catch
		{
			return false;
		}
	}

	private static string GetTreeRootDisplayName(string path)
	{
		return "(" + path.TrimEnd('\\') + ")";
	}

	private static bool IsDriveRootPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		string pathRoot = Path.GetPathRoot(path);
		return !string.IsNullOrEmpty(pathRoot) && PathsEqual(pathRoot, path);
	}

	private static string GetDriveIconKey(string path)
	{
		try
		{
			DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(path));
			switch (driveInfo.DriveType)
			{
			case DriveType.Removable:
				return ClassicIcons.RemovableDriveKey;
			case DriveType.Network:
				return ClassicIcons.NetworkDriveKey;
			case DriveType.CDRom:
				return ClassicIcons.CdDriveKey;
			default:
				return ClassicIcons.FixedDriveKey;
			}
		}
		catch
		{
			return ClassicIcons.FixedDriveKey;
		}
	}

	private static void SetExpandedNodeIcon(TreeNode node, bool expanded)
	{
		if (node?.Tag is not string text || IsDriveRootPath(text))
		{
			return;
		}
		node.ImageKey = expanded ? ClassicIcons.FolderOpenKey : ClassicIcons.FolderClosedKey;
	}
}
