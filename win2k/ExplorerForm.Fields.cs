using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
    public partial class ExplorerForm : Form
    {
        private sealed class OperationResult
        {
            private int _successCount;
            private List<string> _errors;

            internal int SuccessCount
            {
                get { return _successCount; }
            }

            internal List<string> Errors
            {
                get { return _errors; }
            }

            internal bool HasErrors
            {
                get { return _errors.Count > 0; }
            }

            internal OperationResult(int successCount, List<string> errors)
            {
                _successCount = successCount;
                _errors = errors;
            }
        }

        private StatusBar statusBar;
        private StatusBarPanel itemsCount;
        private StatusBarPanel locationPanel;
        private ToolBar navigationBar;
        private ImageList toolbarImages;
        private ToolBarButton backButton;
        private ToolBarButton forwardButton;
        private ToolBarButton upButton;
        private ToolBarButton toolbarSeparator1;
        private ToolBarButton cutButton;
        private ToolBarButton copyButton;
        private ToolBarButton pasteButton;
        private ToolBarButton deleteButton;
        private ToolBarButton propertiesButton;
        private ToolBarButton toolbarSeparator2;
        private ToolBarButton toolbarSeparator3;
        private ToolBarButton foldersButton;
        private ToolBarButton viewsButton;
        private ToolBarButton refreshButton;
        private ToolBarButton newFolderButton;
        private ToolBarButton largeIconsButton;
        private ToolBarButton smallIconsButton;
        private ToolBarButton listButton;
        private ToolBarButton detailsButton;
        private Panel addressPanel;
        private Panel pathPanel;
        private Panel toolbarPanel;
        private ComboBox addressBar;
        private Panel contentPanel;
        private Panel descriptionPanel;
        private Label foldersHeaderLabel;
        private Label contentsHeaderLabel;
        private TreeView folderTree;
        private Splitter treeSplitter;
        private ContextMenu fileContextMenu;
        private ListView folderView;
        private ColumnHeader colName;
        private ColumnHeader colSize;
        private ColumnHeader colType;
        private ColumnHeader colModified;
        private MenuItem[] viewModeItems;
        private MenuItem[] arrangeItems;
        private readonly List<MenuItem> selectionDependentItems = new List<MenuItem>();
        private readonly List<MenuItem> clipboardDependentItems = new List<MenuItem>();
        private readonly Stack<string> backHistory = new Stack<string>();
        private readonly Stack<string> forwardHistory = new Stack<string>();
        private MenuItem backMenuItem;
        private MenuItem forwardMenuItem;
        private MenuItem upMenuItem;
        private MenuItem refreshMenuItem;
        private MenuItem toolbarMenuItem;
        private MenuItem treeMenuItem;
        private MenuItem statusBarMenuItem;
        private MenuItem openMenuItem;
        private MenuItem cutMenuItem;
        private MenuItem copyMenuItem;
        private MenuItem pasteMenuItem;
        private MenuItem deleteMenuItem;
        private MenuItem renameMenuItem;
        private MenuItem propertiesMenuItem;
        private FileSystemWatcher currentWatcher;
        private Timer watcherTimer;
        private bool refreshQueued;
        private bool suppressTreeSelection;
        private int loadVersion;
        private int sortColumn;
        private SortOrder sortOrder = SortOrder.Ascending;
        private DirManager _manager;

        private DirManager Manager
        {
            get { return _manager; }
            set { _manager = value; }
        }

        private string CurrentPath
        {
            get { return _manager.CurrentDir.FullName; }
        }
    }
}
