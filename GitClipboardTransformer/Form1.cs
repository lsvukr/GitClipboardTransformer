using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace GitClipboardTransformer
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private string _lastClipboardText = string.Empty;
        private bool _handleCreated = false;
        private bool _paused = false;
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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AddClipboardFormatListener(Handle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            RemoveClipboardFormatListener(Handle);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
                ProcessClipboard();
            base.WndProc(ref m);
        }

        private void ProcessClipboard()
        {
            if (_paused)
                return;

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
                description = description.Replace(" - DEVELOPMENT", string.Empty);
                description = description.Replace(":", " ");
                string transformed = $"AB#{id} {description} ai:90%";

                _lastClipboardText = transformed;
                Clipboard.SetText(transformed);

                labelLastTransformed.Text = transformed;
                labelStatus.Text = $"Transformed at {DateTime.Now:HH:mm:ss}";

                notifyIcon1.ShowBalloonTip(2000, "Clipboard transformed", transformed, ToolTipIcon.Info);
            }
            catch
            {
                // Clipboard access can fail when another app holds it; ignore and retry.
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

        private void PauseMenuItem_Click(object? sender, EventArgs e)
        {
            _paused = !_paused;
            pauseMenuItem.Text = _paused ? "Resume" : "Pause";
            labelStatus.Text = _paused ? "Paused" : "Monitoring clipboard...";
            notifyIcon1.Text = _paused ? "Git Clipboard Transformer (Paused)" : "Git Clipboard Transformer";
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

        private static Icon LoadEmbeddedIcon()
        {
            var assembly = typeof(Form1).Assembly;
            using var stream = assembly.GetManifestResourceStream("GitClipboardTransformer.git.ico")!;
            return new Icon(stream);
        }
    }
}
