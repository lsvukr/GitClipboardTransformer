namespace GitClipboardTransformer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            openMenuItem = new ToolStripMenuItem();
            exitMenuItem = new ToolStripMenuItem();
            labelStatus = new Label();
            labelLastTransformed = new Label();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();

            // contextMenuStrip1
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { openMenuItem, exitMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";

            // openMenuItem
            openMenuItem.Name = "openMenuItem";
            openMenuItem.Text = "Open";
            openMenuItem.Click += OpenMenuItem_Click;

            // exitMenuItem
            exitMenuItem.Name = "exitMenuItem";
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += ExitMenuItem_Click;

            // notifyIcon1
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = LoadEmbeddedIcon();
            notifyIcon1.Text = "Git Clipboard Transformer";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += NotifyIcon1_DoubleClick;

            // labelStatus
            labelStatus.AutoSize = false;
            labelStatus.Dock = DockStyle.Top;
            labelStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            labelStatus.Height = 30;
            labelStatus.Padding = new Padding(8, 8, 8, 0);
            labelStatus.Text = "Monitoring clipboard...";

            // labelLastTransformed
            labelLastTransformed.AutoSize = false;
            labelLastTransformed.Dock = DockStyle.Fill;
            labelLastTransformed.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            labelLastTransformed.ForeColor = Color.DarkGreen;
            labelLastTransformed.Padding = new Padding(8, 8, 8, 8);
            labelLastTransformed.Text = "";

            // Form1
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 120);
            Controls.Add(labelLastTransformed);
            Controls.Add(labelStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = LoadEmbeddedIcon();
            MaximizeBox = false;
            Text = "Git Clipboard Transformer";
            Resize += Form1_Resize;

            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem openMenuItem;
        private ToolStripMenuItem exitMenuItem;
        private Label labelStatus;
        private Label labelLastTransformed;
    }
}
