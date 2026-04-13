using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
    public partial class ExplorerForm
    {
        private void SetUpUI(bool showStatusBar, View viewMode)
        {
            itemsCount = new StatusBarPanel();
            itemsCount.Width = 140;
            locationPanel = new StatusBarPanel();
            locationPanel.AutoSize = StatusBarPanelAutoSize.Spring;
            statusBar.Panels.AddRange(new StatusBarPanel[2] { itemsCount, locationPanel });
            statusBar.Visible = showStatusBar;

            statusBarMenuItem = new MenuItem("&Status Bar", new EventHandler(ToggleStatusBar));
            statusBarMenuItem.Checked = showStatusBar;
            toolbarMenuItem = new MenuItem("&Toolbar", new EventHandler(ToggleToolbar));
            toolbarMenuItem.Checked = true;
            treeMenuItem = new MenuItem("&Folders", new EventHandler(ToggleFolders));
            treeMenuItem.Checked = true;

            MenuItem viewLargeIcons = new MenuItem("Lar&ge Icons", ToggleFolderViewModeHandler(View.LargeIcon));
            viewLargeIcons.RadioCheck = true;
            viewLargeIcons.Checked = (viewMode == View.LargeIcon);
            MenuItem viewSmallIcons = new MenuItem("&Small Icons", ToggleFolderViewModeHandler(View.SmallIcon));
            viewSmallIcons.RadioCheck = true;
            viewSmallIcons.Checked = (viewMode == View.SmallIcon);
            MenuItem viewList = new MenuItem("&List", ToggleFolderViewModeHandler(View.List));
            viewList.RadioCheck = true;
            viewList.Checked = (viewMode == View.List);
            MenuItem viewDetails = new MenuItem("&Details", ToggleFolderViewModeHandler(View.Details));
            viewDetails.RadioCheck = true;
            viewDetails.Checked = (viewMode == View.Details);
            viewModeItems = new MenuItem[4] { viewLargeIcons, viewSmallIcons, viewList, viewDetails };

            MenuItem arrangeByName = new MenuItem("by &Name", SortByColumnHandler(0));
            arrangeByName.RadioCheck = true;
            arrangeByName.Checked = true;
            MenuItem arrangeByType = new MenuItem("by &Type", SortByColumnHandler(2));
            arrangeByType.RadioCheck = true;
            MenuItem arrangeBySize = new MenuItem("by &Size", SortByColumnHandler(1));
            arrangeBySize.RadioCheck = true;
            MenuItem arrangeByDate = new MenuItem("by &Date", SortByColumnHandler(3));
            arrangeByDate.RadioCheck = true;
            arrangeItems = new MenuItem[4] { arrangeByName, arrangeByType, arrangeBySize, arrangeByDate };

            backMenuItem = new MenuItem("&Back", new EventHandler(NavigateBack));
            forwardMenuItem = new MenuItem("&Forward", new EventHandler(NavigateForward));
            upMenuItem = new MenuItem("&Up One Level", new EventHandler(UpOneLevel), Shortcut.CtrlU);
            refreshMenuItem = new MenuItem("&Refresh", new EventHandler(RefreshWindow), Shortcut.F5);
            openMenuItem = new MenuItem("&Open", new EventHandler(OpenSelectedItem));
            cutMenuItem = new MenuItem("Cu&t", new EventHandler(TriggerCut), Shortcut.CtrlX);
            copyMenuItem = new MenuItem("&Copy", new EventHandler(TriggerCopy), Shortcut.CtrlC);
            pasteMenuItem = new MenuItem("&Paste", new EventHandler(TriggerPaste), Shortcut.CtrlV);
            deleteMenuItem = new MenuItem("&Delete", new EventHandler(TriggerDelete), Shortcut.Del);
            renameMenuItem = new MenuItem("&Rename", new EventHandler(TriggerRename), Shortcut.F2);
            propertiesMenuItem = new MenuItem("Propert&ies", new EventHandler(ShowProperties));

            MenuItem contextOpen = new MenuItem("&Open", new EventHandler(OpenSelectedItem));
            MenuItem contextDelete = new MenuItem("&Delete", new EventHandler(TriggerDelete));

            selectionDependentItems.AddRange(new MenuItem[8]
            {
                openMenuItem, cutMenuItem, copyMenuItem, deleteMenuItem,
                renameMenuItem, propertiesMenuItem,
                contextOpen, contextDelete
            });
            clipboardDependentItems.Add(pasteMenuItem);

            MenuItem menuItem = new MenuItem("&File", new MenuItem[10]
            {
                new MenuItem("&New Window", new EventHandler(OpenCurrentFolderInNewWindow), Shortcut.CtrlN),
                new MenuItem("&Go To...", new EventHandler(GoToPrompt), Shortcut.CtrlG),
                new MenuItem("&New Folder", new EventHandler(TriggerNewFolder)),
                new MenuItem("-"),
                openMenuItem,
                deleteMenuItem,
                renameMenuItem,
                propertiesMenuItem,
                new MenuItem("-"),
                new MenuItem("&Close", new EventHandler(delegate(object s, EventArgs ea) { Close(); }))
            });
            menuItem.Popup += new EventHandler(UpdateSelectionDependentMenu);

            MenuItem menuItem2 = new MenuItem("&Edit", new MenuItem[10]
            {
                cutMenuItem,
                copyMenuItem,
                pasteMenuItem,
                new MenuItem("-"),
                new MenuItem("&Delete", new EventHandler(TriggerDelete)),
                new MenuItem("&Rename", new EventHandler(TriggerRename)),
                new MenuItem("-"),
                new MenuItem("Select &All", new EventHandler(SelectAll), Shortcut.CtrlA),
                new MenuItem("&Invert Selection", new EventHandler(InvertSelection)),
                new MenuItem("&Copy Path", new EventHandler(delegate(object s, EventArgs ea) { Clipboard.SetText(CurrentPath); }))
            });
            menuItem2.Popup += new EventHandler(UpdateSelectionDependentMenu);
            menuItem2.Popup += new EventHandler(UpdateClipboardDependentMenu);

            MenuItem menuItem3 = new MenuItem("&View", new MenuItem[11]
            {
                toolbarMenuItem,
                treeMenuItem,
                statusBarMenuItem,
                new MenuItem("-"),
                viewModeItems[0],
                viewModeItems[1],
                viewModeItems[2],
                viewModeItems[3],
                new MenuItem("-"),
                refreshMenuItem,
                new MenuItem("&Arrange Icons", arrangeItems)
            });

            MenuItem menuItem4 = new MenuItem("&Tools", new MenuItem[1]
            {
                new MenuItem("&Refresh Tree", new EventHandler(delegate(object s, EventArgs ea)
                {
                    ReloadDriveTree();
                    SelectTreeNodeForPath(CurrentPath);
                }))
            });
            MenuItem menuItem5 = new MenuItem("&Help", new MenuItem[1]
            {
                new MenuItem("&About ex_plorer", new EventHandler(ShowAboutDialog))
            });
            this.Menu = new MainMenu(new MenuItem[5] { menuItem, menuItem2, menuItem3, menuItem4, menuItem5 });

            fileContextMenu = new ContextMenu(new MenuItem[8]
            {
                contextOpen,
                new MenuItem("-"),
                new MenuItem("Cu&t", new EventHandler(TriggerCut)),
                new MenuItem("&Copy", new EventHandler(TriggerCopy)),
                new MenuItem("&Paste", new EventHandler(TriggerPaste)),
                contextDelete,
                new MenuItem("&Rename", new EventHandler(TriggerRename)),
                new MenuItem("Propert&ies", new EventHandler(ShowProperties))
            });
            fileContextMenu.Popup += new EventHandler(ContextMenu_Popup);
            folderView.ContextMenu = fileContextMenu;

            UpdateNavigationControls();
            UpdateSelectionDependentMenu(this, EventArgs.Empty);
            UpdateClipboardDependentMenu(this, EventArgs.Empty);
        }

        private EventHandler ToggleFolderViewModeHandler(View view)
        {
            return new EventHandler(delegate(object sender, EventArgs e)
            {
                folderView.View = view;
                UpdateViewMenuChecks(view);
            });
        }

        private EventHandler SortByColumnHandler(int column)
        {
            return new EventHandler(delegate(object sender, EventArgs e)
            {
                sortColumn = column;
                sortOrder = SortOrder.Ascending;
                ApplySort();
            });
        }

        private void UpdateViewMenuChecks(View view)
        {
            viewModeItems[0].Checked = view == View.LargeIcon;
            viewModeItems[1].Checked = view == View.SmallIcon;
            viewModeItems[2].Checked = view == View.List;
            viewModeItems[3].Checked = view == View.Details;
            UpdateToolbarViewButtons(view);
        }

        private void UpdateToolbarViewButtons(View view)
        {
            largeIconsButton.Pushed = view == View.LargeIcon;
            smallIconsButton.Pushed = view == View.SmallIcon;
            listButton.Pushed = view == View.List;
            detailsButton.Pushed = view == View.Details;
        }

        private void UpdateArrangeMenuChecks()
        {
            if (arrangeItems == null)
            {
                return;
            }
            int num;
            if (sortColumn == 0) num = 0;
            else if (sortColumn == 2) num = 1;
            else if (sortColumn == 1) num = 2;
            else num = 3;
            for (int i = 0; i < arrangeItems.Length; i++)
            {
                arrangeItems[i].Checked = (i == num);
            }
        }

        private void ToggleToolbar(object sender, EventArgs e)
        {
            toolbarPanel.Visible = !toolbarPanel.Visible;
            pathPanel.Dock = toolbarPanel.Visible ? DockStyle.Left : DockStyle.Fill;
            toolbarMenuItem.Checked = toolbarPanel.Visible;
        }

        private void ToggleFolders(object sender, EventArgs e)
        {
            bool flag = !folderTree.Visible;
            folderTree.Visible = flag;
            treeSplitter.Visible = flag;
            treeMenuItem.Checked = flag;
            foldersButton.Pushed = flag;
            foldersHeaderLabel.Visible = flag;
        }

        private void ToggleStatusBar(object sender, EventArgs e)
        {
            statusBar.Visible = !statusBar.Visible;
            statusBarMenuItem.Checked = statusBar.Visible;
        }

        private void ShowAboutDialog(object sender, EventArgs e)
        {
            MessageBox.Show(this, "ex_plorer\nA Windows 95-inspired Explorer rebuild.\n\nReconstructed from the original ex_plorer.exe and adapted for modern Windows.", "About ex_plorer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SelectAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in folderView.Items)
            {
                item.Selected = true;
            }
        }

        private void InvertSelection(object sender, EventArgs e)
        {
            foreach (ListViewItem item in folderView.Items)
            {
                item.Selected = !item.Selected;
            }
        }

        private void UpdateSelectionDependentMenu(object sender, EventArgs e)
        {
            bool enabled = folderView.SelectedItems.Count > 0;
            foreach (MenuItem selectionDependentItem in selectionDependentItems)
            {
                selectionDependentItem.Enabled = enabled;
            }
            renameMenuItem.Enabled = folderView.SelectedItems.Count == 1;
            cutButton.Enabled = enabled;
            copyButton.Enabled = enabled;
            deleteButton.Enabled = enabled;
            propertiesButton.Enabled = enabled;
        }

        private void UpdateClipboardDependentMenu(object sender, EventArgs e)
        {
            bool enabled = ClipboardHelper.HasFileDropList();
            foreach (MenuItem clipboardDependentItem in clipboardDependentItems)
            {
                clipboardDependentItem.Enabled = enabled;
            }
            pasteButton.Enabled = enabled;
        }

        private void RefreshWindow(object sender, EventArgs e)
        {
            ReloadDriveTree();
            SelectTreeNodeForPath(CurrentPath);
            LoadCurrentDirectory();
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            UpdateSelectionDependentMenu(sender, e);
            UpdateClipboardDependentMenu(sender, e);
            bool flag = folderView.SelectedItems.Count > 0;
            fileContextMenu.MenuItems[2].Enabled = flag;
            fileContextMenu.MenuItems[3].Enabled = flag;
            fileContextMenu.MenuItems[4].Enabled = ClipboardHelper.HasFileDropList();
            fileContextMenu.MenuItems[6].Enabled = folderView.SelectedItems.Count == 1;
            fileContextMenu.MenuItems[7].Enabled = flag;
        }

        private void folderView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectionDependentMenu(sender, e);
            UpdateStatusForSelection();
        }

        private void folderView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            ListViewHitTestInfo listViewHitTestInfo = folderView.HitTest(e.Location);
            if (listViewHitTestInfo.Item == null)
            {
                folderView.SelectedItems.Clear();
            }
            else if (!listViewHitTestInfo.Item.Selected)
            {
                folderView.SelectedItems.Clear();
                listViewHitTestInfo.Item.Selected = true;
            }
        }

        private void folderView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (sortColumn == e.Column)
            {
                sortOrder = (sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sortColumn = e.Column;
                sortOrder = SortOrder.Ascending;
            }
            ApplySort();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeWatcher();
                if (watcherTimer != null) watcherTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            navigationBar = new ToolBar();
            toolbarImages = ToolbarImages.Create();
            backButton = new ToolBarButton();
            forwardButton = new ToolBarButton();
            upButton = new ToolBarButton();
            toolbarSeparator1 = new ToolBarButton();
            cutButton = new ToolBarButton();
            copyButton = new ToolBarButton();
            pasteButton = new ToolBarButton();
            deleteButton = new ToolBarButton();
            propertiesButton = new ToolBarButton();
            toolbarSeparator2 = new ToolBarButton();
            toolbarSeparator3 = new ToolBarButton();
            foldersButton = new ToolBarButton();
            viewsButton = new ToolBarButton();
            refreshButton = new ToolBarButton();
            newFolderButton = new ToolBarButton();
            largeIconsButton = new ToolBarButton();
            smallIconsButton = new ToolBarButton();
            listButton = new ToolBarButton();
            detailsButton = new ToolBarButton();
            addressPanel = new Panel();
            pathPanel = new Panel();
            toolbarPanel = new Panel();
            addressBar = new ComboBox();
            contentPanel = new Panel();
            descriptionPanel = new Panel();
            foldersHeaderLabel = new Label();
            contentsHeaderLabel = new Label();
            folderTree = new TreeView();
            treeSplitter = new Splitter();
            folderView = new ListView();
            colName = new ColumnHeader();
            colSize = new ColumnHeader();
            colType = new ColumnHeader();
            colModified = new ColumnHeader();
            statusBar = new StatusBar();
            watcherTimer = new Timer();
            SuspendLayout();
            navigationBar.Appearance = ToolBarAppearance.Normal;
            navigationBar.ImageList = toolbarImages;
            toolbarSeparator1.Style = ToolBarButtonStyle.Separator;
            toolbarSeparator2.Style = ToolBarButtonStyle.Separator;
            toolbarSeparator3.Style = ToolBarButtonStyle.Separator;
            largeIconsButton.Style = ToolBarButtonStyle.ToggleButton;
            smallIconsButton.Style = ToolBarButtonStyle.ToggleButton;
            listButton.Style = ToolBarButtonStyle.ToggleButton;
            detailsButton.Style = ToolBarButtonStyle.ToggleButton;
            upButton.ImageIndex = ToolbarImages.Up;
            cutButton.ImageIndex = ToolbarImages.Cut;
            copyButton.ImageIndex = ToolbarImages.Copy;
            pasteButton.ImageIndex = ToolbarImages.Paste;
            backButton.ImageIndex = ToolbarImages.Back;
            deleteButton.ImageIndex = ToolbarImages.Delete;
            propertiesButton.ImageIndex = ToolbarImages.Properties;
            largeIconsButton.ImageIndex = ToolbarImages.LargeIcons;
            smallIconsButton.ImageIndex = ToolbarImages.SmallIcons;
            listButton.ImageIndex = ToolbarImages.List;
            detailsButton.ImageIndex = ToolbarImages.Details;
            navigationBar.Buttons.AddRange(new ToolBarButton[14] { upButton, toolbarSeparator1, cutButton, copyButton, pasteButton, backButton, toolbarSeparator2, deleteButton, propertiesButton, toolbarSeparator3, largeIconsButton, smallIconsButton, listButton, detailsButton });
            navigationBar.Dock = DockStyle.Fill;
            navigationBar.Divider = false;
            navigationBar.DropDownArrows = false;
            navigationBar.Wrappable = false;
            navigationBar.ButtonSize = new Size(22, 22);
            navigationBar.ShowToolTips = true;
            navigationBar.ButtonClick += new ToolBarButtonClickEventHandler(NavigationBar_ButtonClick);
            upButton.ToolTipText = "Up";
            cutButton.ToolTipText = "Cut";
            copyButton.ToolTipText = "Copy";
            pasteButton.ToolTipText = "Paste";
            backButton.ToolTipText = "Back";
            deleteButton.ToolTipText = "Delete";
            propertiesButton.ToolTipText = "Properties";
            largeIconsButton.ToolTipText = "Large Icons";
            smallIconsButton.ToolTipText = "Small Icons";
            listButton.ToolTipText = "List";
            detailsButton.ToolTipText = "Details";
            addressPanel.Dock = DockStyle.Top;
            addressPanel.Height = 30;
            addressPanel.Padding = new Padding(3, 2, 3, 2);
            pathPanel.Dock = DockStyle.Left;
            pathPanel.Width = 220;
            pathPanel.BorderStyle = BorderStyle.Fixed3D;
            toolbarPanel.Dock = DockStyle.Fill;
            toolbarPanel.BorderStyle = BorderStyle.Fixed3D;
            addressBar.Dock = DockStyle.Fill;
            addressBar.DropDownStyle = ComboBoxStyle.DropDownList;
            addressBar.DrawMode = DrawMode.OwnerDrawFixed;
            addressBar.FlatStyle = FlatStyle.Standard;
            addressBar.IntegralHeight = false;
            addressBar.MaxDropDownItems = 12;
            addressBar.ItemHeight = 16;
            addressBar.DrawItem += new DrawItemEventHandler(addressBar_DrawItem);
            addressBar.SelectionChangeCommitted += new EventHandler(addressBar_SelectionChangeCommitted);
            pathPanel.Controls.Add(addressBar);
            toolbarPanel.Controls.Add(navigationBar);
            addressPanel.Controls.Add(toolbarPanel);
            addressPanel.Controls.Add(pathPanel);
            contentPanel.Dock = DockStyle.Fill;
            descriptionPanel.Dock = DockStyle.Top;
            descriptionPanel.Height = 20;
            descriptionPanel.BackColor = SystemColors.Control;
            foldersHeaderLabel.Dock = DockStyle.Left;
            foldersHeaderLabel.Width = 194;
            foldersHeaderLabel.Text = "All Folders";
            foldersHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
            foldersHeaderLabel.BorderStyle = BorderStyle.Fixed3D;
            contentsHeaderLabel.Dock = DockStyle.Fill;
            contentsHeaderLabel.Text = "Contents of ";
            contentsHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
            contentsHeaderLabel.BorderStyle = BorderStyle.Fixed3D;
            descriptionPanel.Controls.Add(contentsHeaderLabel);
            descriptionPanel.Controls.Add(foldersHeaderLabel);
            folderTree.AllowDrop = true;
            folderTree.BorderStyle = BorderStyle.Fixed3D;
            folderTree.Dock = DockStyle.Left;
            folderTree.HideSelection = false;
            folderTree.ShowLines = true;
            folderTree.ShowPlusMinus = true;
            folderTree.ShowRootLines = true;
            folderTree.Width = 190;
            folderTree.BeforeExpand += new TreeViewCancelEventHandler(folderTree_BeforeExpand);
            folderTree.AfterCollapse += new TreeViewEventHandler(folderTree_AfterCollapse);
            folderTree.AfterSelect += new TreeViewEventHandler(folderTree_AfterSelect);
            folderTree.NodeMouseClick += new TreeNodeMouseClickEventHandler(folderTree_NodeMouseClick);
            folderTree.DragEnter += new DragEventHandler(DragTarget_DragEnter);
            folderTree.DragOver += new DragEventHandler(DragTarget_DragOver);
            folderTree.DragDrop += new DragEventHandler(folderTree_DragDrop);
            treeSplitter.Dock = DockStyle.Left;
            treeSplitter.Width = 4;
            folderView.AllowDrop = true;
            folderView.BorderStyle = BorderStyle.Fixed3D;
            folderView.Columns.AddRange(new ColumnHeader[4] { colName, colSize, colType, colModified });
            folderView.Dock = DockStyle.Fill;
            folderView.FullRowSelect = true;
            folderView.GridLines = false;
            folderView.HideSelection = false;
            folderView.LabelEdit = true;
            folderView.MultiSelect = true;
            folderView.UseCompatibleStateImageBehavior = false;
            folderView.AfterLabelEdit += new LabelEditEventHandler(folderView_AfterLabelEdit);
            folderView.ItemActivate += new EventHandler(folderView_ItemActivate);
            folderView.SelectedIndexChanged += new EventHandler(folderView_SelectedIndexChanged);
            folderView.ColumnClick += new ColumnClickEventHandler(folderView_ColumnClick);
            folderView.MouseDown += new MouseEventHandler(folderView_MouseDown);
            folderView.ItemDrag += new ItemDragEventHandler(folderView_ItemDrag);
            folderView.DragEnter += new DragEventHandler(DragTarget_DragEnter);
            folderView.DragOver += new DragEventHandler(DragTarget_DragOver);
            folderView.DragDrop += new DragEventHandler(folderView_DragDrop);
            colName.Text = "Name"; colName.Width = 190;
            colSize.Text = "Size"; colSize.TextAlign = HorizontalAlignment.Right; colSize.Width = 90;
            colType.Text = "Type"; colType.Width = 120;
            colModified.Text = "Modified"; colModified.Width = 140;
            statusBar.Dock = DockStyle.Bottom; statusBar.ShowPanels = true; statusBar.SizingGrip = true;
            watcherTimer.Interval = 450;
            watcherTimer.Tick += new EventHandler(watcherTimer_Tick);
            contentPanel.Controls.Add(folderView);
            contentPanel.Controls.Add(treeSplitter);
            contentPanel.Controls.Add(folderTree);
            contentPanel.Controls.Add(descriptionPanel);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Font = SystemInformation.MenuFont;
            ClientSize = new Size(720, 470);
            Controls.Add(contentPanel);
            Controls.Add(addressPanel);
            Controls.Add(statusBar);
            MinimumSize = new Size(520, 320);
            Name = "ExplorerForm";
            Text = "ex_plorer";
            Activated += new EventHandler(ExplorerForm_Activated);
            FormClosed += new FormClosedEventHandler(ExplorerForm_FormClosed);
            ResumeLayout(false);
        }
    }
}
