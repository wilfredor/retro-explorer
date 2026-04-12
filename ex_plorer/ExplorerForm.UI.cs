using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer;

public partial class ExplorerForm
{
	private void SetUpUI(bool showStatusBar, View viewMode)
	{
		itemsCount = new StatusBarPanel { Width = 140 };
		locationPanel = new StatusBarPanel { AutoSize = StatusBarPanelAutoSize.Spring };
		statusBar.Panels.AddRange(new StatusBarPanel[2] { itemsCount, locationPanel });
		statusBar.Visible = showStatusBar;

		statusBarMenuItem = new MenuItem("&Status Bar", ToggleStatusBar) { Checked = showStatusBar };
		toolbarMenuItem = new MenuItem("&Toolbar", ToggleToolbar) { Checked = true };
		treeMenuItem = new MenuItem("&Folders", ToggleFolders) { Checked = true };

		viewModeItems = new MenuItem[4]
		{
			new MenuItem("Lar&ge Icons", ToggleFolderViewMode(View.LargeIcon)) { RadioCheck = true, Checked = (viewMode == View.LargeIcon) },
			new MenuItem("&Small Icons", ToggleFolderViewMode(View.SmallIcon)) { RadioCheck = true, Checked = (viewMode == View.SmallIcon) },
			new MenuItem("&List", ToggleFolderViewMode(View.List)) { RadioCheck = true, Checked = (viewMode == View.List) },
			new MenuItem("&Details", ToggleFolderViewMode(View.Details)) { RadioCheck = true, Checked = (viewMode == View.Details) }
		};
		arrangeItems = new MenuItem[4]
		{
			new MenuItem("by &Name", SortByColumn(0)) { RadioCheck = true, Checked = true },
			new MenuItem("by &Type", SortByColumn(2)) { RadioCheck = true },
			new MenuItem("by &Size", SortByColumn(1)) { RadioCheck = true },
			new MenuItem("by &Date", SortByColumn(3)) { RadioCheck = true }
		};

		backMenuItem = new MenuItem("&Back", NavigateBack);
		forwardMenuItem = new MenuItem("&Forward", NavigateForward);
		upMenuItem = new MenuItem("&Up One Level", UpOneLevel, Shortcut.CtrlU);
		refreshMenuItem = new MenuItem("&Refresh", RefreshWindow, Shortcut.F5);
		openMenuItem = new MenuItem("&Open", OpenSelectedItem);
		cutMenuItem = new MenuItem("Cu&t", TriggerCut, Shortcut.CtrlX);
		copyMenuItem = new MenuItem("&Copy", TriggerCopy, Shortcut.CtrlC);
		pasteMenuItem = new MenuItem("&Paste", TriggerPaste, Shortcut.CtrlV);
		deleteMenuItem = new MenuItem("&Delete", TriggerDelete, Shortcut.Del);
		renameMenuItem = new MenuItem("&Rename", TriggerRename, Shortcut.F2);
		propertiesMenuItem = new MenuItem("Propert&ies", ShowProperties);

		selectionDependentItems.AddRange(new MenuItem[8]
		{
			openMenuItem, cutMenuItem, copyMenuItem, deleteMenuItem,
			renameMenuItem, propertiesMenuItem,
			new MenuItem("&Open", OpenSelectedItem),
			new MenuItem("&Delete", TriggerDelete)
		});
		clipboardDependentItems.Add(pasteMenuItem);

		MenuItem menuItem = new MenuItem("&File", new MenuItem[10]
		{
			new MenuItem("&New Window", OpenCurrentFolderInNewWindow, Shortcut.CtrlN),
			new MenuItem("&Go To...", GoToPrompt, Shortcut.CtrlG),
			new MenuItem("&New Folder", TriggerNewFolder),
			new MenuItem("-"),
			openMenuItem,
			deleteMenuItem,
			renameMenuItem,
			propertiesMenuItem,
			new MenuItem("-"),
			new MenuItem("&Close", delegate { Close(); })
		});
		menuItem.Popup += UpdateSelectionDependentMenu;

		MenuItem menuItem2 = new MenuItem("&Edit", new MenuItem[10]
		{
			cutMenuItem,
			copyMenuItem,
			pasteMenuItem,
			new MenuItem("-"),
			new MenuItem("&Delete", TriggerDelete),
			new MenuItem("&Rename", TriggerRename),
			new MenuItem("-"),
			new MenuItem("Select &All", SelectAll, Shortcut.CtrlA),
			new MenuItem("&Invert Selection", InvertSelection),
			new MenuItem("&Copy Path", delegate { Clipboard.SetText(CurrentPath); })
		});
		menuItem2.Popup += UpdateSelectionDependentMenu;
		menuItem2.Popup += UpdateClipboardDependentMenu;

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
			new MenuItem("&Refresh Tree", delegate
			{
				ReloadDriveTree();
				SelectTreeNodeForPath(CurrentPath);
			})
		});
		MenuItem menuItem5 = new MenuItem("&Help", new MenuItem[1]
		{
			new MenuItem("&About ex_plorer", ShowAboutDialog)
		});
		base.Menu = new MainMenu(new MenuItem[5] { menuItem, menuItem2, menuItem3, menuItem4, menuItem5 });

		fileContextMenu = new ContextMenu(new MenuItem[8]
		{
			selectionDependentItems[6],
			new MenuItem("-"),
			new MenuItem("Cu&t", TriggerCut),
			new MenuItem("&Copy", TriggerCopy),
			new MenuItem("&Paste", TriggerPaste),
			selectionDependentItems[7],
			new MenuItem("&Rename", TriggerRename),
			new MenuItem("Propert&ies", ShowProperties)
		});
		fileContextMenu.Popup += ContextMenu_Popup;
		folderView.ContextMenu = fileContextMenu;

		UpdateNavigationControls();
		UpdateSelectionDependentMenu(this, EventArgs.Empty);
		UpdateClipboardDependentMenu(this, EventArgs.Empty);
	}

	private EventHandler ToggleFolderViewMode(View view)
	{
		return delegate(object sender, EventArgs e)
		{
			folderView.View = view;
			UpdateViewMenuChecks(view);
		};
	}

	private EventHandler SortByColumn(int column)
	{
		return delegate
		{
			sortColumn = column;
			sortOrder = SortOrder.Ascending;
			ApplySort();
		};
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
		int num = ((sortColumn == 0) ? 0 : ((sortColumn == 2) ? 1 : ((sortColumn == 1) ? 2 : 3)));
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
		LoadCurrentDirectoryAsync();
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
			watcherTimer?.Dispose();
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
		navigationBar.ButtonClick += NavigationBar_ButtonClick;
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
		addressBar.DrawItem += addressBar_DrawItem;
		addressBar.SelectionChangeCommitted += addressBar_SelectionChangeCommitted;
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
		folderTree.BeforeExpand += folderTree_BeforeExpand;
		folderTree.AfterCollapse += folderTree_AfterCollapse;
		folderTree.AfterSelect += folderTree_AfterSelect;
		folderTree.NodeMouseClick += folderTree_NodeMouseClick;
		folderTree.DragEnter += DragTarget_DragEnter;
		folderTree.DragOver += DragTarget_DragOver;
		folderTree.DragDrop += folderTree_DragDrop;
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
		folderView.AfterLabelEdit += folderView_AfterLabelEdit;
		folderView.ItemActivate += folderView_ItemActivate;
		folderView.SelectedIndexChanged += folderView_SelectedIndexChanged;
		folderView.ColumnClick += folderView_ColumnClick;
		folderView.MouseDown += folderView_MouseDown;
		folderView.ItemDrag += folderView_ItemDrag;
		folderView.DragEnter += DragTarget_DragEnter;
		folderView.DragOver += DragTarget_DragOver;
		folderView.DragDrop += folderView_DragDrop;
		colName.Text = "Name"; colName.Width = 190;
		colSize.Text = "Size"; colSize.TextAlign = HorizontalAlignment.Right; colSize.Width = 90;
		colType.Text = "Type"; colType.Width = 120;
		colModified.Text = "Modified"; colModified.Width = 140;
		statusBar.Dock = DockStyle.Bottom; statusBar.ShowPanels = true; statusBar.SizingGrip = true;
		watcherTimer.Interval = 450;
		watcherTimer.Tick += watcherTimer_Tick;
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
		Activated += ExplorerForm_Activated;
		FormClosed += ExplorerForm_FormClosed;
		ResumeLayout(false);
	}
}
