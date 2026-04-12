using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ex_plorer;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		Application.EnableVisualStyles();
		Application.VisualStyleState = VisualStyleState.NoneEnabled;
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);

		string text = ((args.Length != 0) ? args[0] : Directory.GetCurrentDirectory());
		if (!Directory.Exists(text))
		{
			MessageBox.Show("Invalid or inaccessible directory:\n" + text, "ex_plorer", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}

		Application.Run(new ExplorerForm(text));
	}
}
