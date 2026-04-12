using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer;

public class GotoForm : Form
{
	private TextBox PathBox;

	private Label label1;

	private Button Go;

	public string Result { get; private set; }

	public GotoForm()
		: this(string.Empty)
	{
	}

	public GotoForm(string initialPath)
	{
		InitializeComponent();
		PathBox.Text = initialPath;
		PathBox.SelectAll();
	}

	private void Go_Click(object sender, EventArgs e)
	{
		string text = PathBox.Text;
		if (Path.IsPathRooted(text) && Directory.Exists(text))
		{
			Result = text;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.PathBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.Go = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.PathBox.Location = new System.Drawing.Point(12, 28);
		this.PathBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
		this.PathBox.Name = "PathBox";
		this.PathBox.Size = new System.Drawing.Size(290, 20);
		this.PathBox.TabIndex = 0;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Path to directory:";
		this.Go.Location = new System.Drawing.Point(227, 56);
		this.Go.Name = "Go";
		this.Go.Size = new System.Drawing.Size(75, 23);
		this.Go.TabIndex = 2;
		this.Go.Text = "&Go";
		this.Go.UseVisualStyleBackColor = true;
		this.Go.Click += new System.EventHandler(Go_Click);
		base.AcceptButton = this.Go;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(314, 91);
		base.Controls.Add(this.Go);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.PathBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "GotoForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Go To...";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
