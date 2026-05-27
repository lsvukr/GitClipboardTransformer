using System.Text.RegularExpressions;

namespace GitClipboardTransformer
{
    public partial class Form1 : Form
    {
        private string _lastClipboardText = string.Empty;
        private bool _handleCreated = false;
        private static readonly Regex WorkItemPattern =
            new(@"^(?:Bug|Task)\s+(\d+):\s+(.+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!_handleCreated)
            {
                if (!IsHandleCreated)
                    CreateHandle();
                _handleCreated = true;
                base.SetVisibleCore(false);
                return;
            }
            base.SetVisibleCore(value);
        }

        private void ClipboardTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (!Clipboard.ContainsText())
                    return;

                string text = Clipboard.GetText().Trim();
                if (text == _lastClipboardText)
                    return;

                _lastClipboardText = text;

                var match = WorkItemPattern.Match(text);
                if (!match.Success)
                    return;

                string id = match.Groups[1].Value;
                string description = match.Groups[2].Value.Trim();
                string transformed = $"AB#{id} {description} ai:90%";

                _lastClipboardText = transformed;
                Clipboard.SetText(transformed);

                labelLastTransformed.Text = transformed;
                labelStatus.Text = $"Transformed at {DateTime.Now:HH:mm:ss}";

                notifyIcon1.ShowBalloonTip(2000, "Clipboard transformed", transformed, ToolTipIcon.Info);
            }
            catch
            {
                // Clipboard access can fail when another app holds it; ignore and retry next tick.
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void NotifyIcon1_DoubleClick(object? sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void OpenMenuItem_Click(object? sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void RestoreWindow()
        {
            ShowInTaskbar = true;
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }
    }
}
