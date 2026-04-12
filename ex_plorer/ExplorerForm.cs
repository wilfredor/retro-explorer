using System.Windows.Forms;
namespace ex_plorer;

public partial class ExplorerForm : Form
{
	public ExplorerForm(string path, bool showStatusBar = true, View viewMode = View.Details)
	{
		InitializeComponent();
		SetUpUI(showStatusBar, viewMode);
		base.Icon = ClassicIcons.App;
		Manager = new DirManager(path);
		folderView.LargeImageList = Manager.LargeIcons;
		folderView.SmallImageList = Manager.SmallIcons;
		folderTree.ImageList = Manager.SmallIcons;
		folderView.View = viewMode;
		sortColumn = 0;
		NavigateToInternal(path);
	}
}
