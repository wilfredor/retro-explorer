using System;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
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

        private TreeNode CreateDirectoryNode(string path, string text)
        {
            string displayText = text;
            if (displayText == null)
            {
                displayText = Path.GetFileName(path.TrimEnd('\\'));
            }
            TreeNode treeNode = new TreeNode(displayText);
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
            SetExpandedNodeIcon(e.Node, true);
            PopulateTreeNode(e.Node);
        }

        private void folderTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            SetExpandedNodeIcon(e.Node, false);
        }

        private void folderTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!suppressTreeSelection && e.Node != null)
            {
                string text = e.Node.Tag as string;
                if (text != null)
                {
                    NavigateTo(text, true);
                }
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
                DirectoryInfo[] dirs = new DirectoryInfo((string)node.Tag).GetDirectories();
                Array.Sort(dirs, delegate(DirectoryInfo a, DirectoryInfo b)
                {
                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });
                for (int i = 0; i < dirs.Length; i++)
                {
                    try
                    {
                        node.Nodes.Add(CreateDirectoryNode(dirs[i].FullName, null));
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
                    for (int idx = 0; idx < array.Length; idx++)
                    {
                        PopulateTreeNode(treeNode);
                        text = Path.Combine(text, array[idx]);
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
                string[] dirs = Directory.GetDirectories(path);
                return dirs.Length > 0;
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
            if (node == null) return;
            string text = node.Tag as string;
            if (text == null || IsDriveRootPath(text))
            {
                return;
            }
            node.ImageKey = expanded ? ClassicIcons.FolderOpenKey : ClassicIcons.FolderClosedKey;
        }
    }
}
