using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
    public class GotoForm : Form
    {
        private TextBox PathBox;
        private Label label1;
        private Button Go;
        private string _result;

        public string Result
        {
            get { return _result; }
        }

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
                _result = text;
            }
            this.DialogResult = DialogResult.OK;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.PathBox = new TextBox();
            this.label1 = new Label();
            this.Go = new Button();
            this.SuspendLayout();
            this.PathBox.Location = new Point(12, 28);
            this.PathBox.Name = "PathBox";
            this.PathBox.Size = new Size(290, 20);
            this.PathBox.TabIndex = 0;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(87, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Path to directory:";
            this.Go.Location = new Point(227, 56);
            this.Go.Name = "Go";
            this.Go.Size = new Size(75, 23);
            this.Go.TabIndex = 2;
            this.Go.Text = "&Go";
            this.Go.UseVisualStyleBackColor = true;
            this.Go.Click += new EventHandler(Go_Click);
            this.AcceptButton = this.Go;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(314, 91);
            this.Controls.Add(this.Go);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PathBox);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GotoForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Go To...";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
