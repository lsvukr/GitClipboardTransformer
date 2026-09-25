using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace GitClipboardTransformer
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private const int MaxRetries = 5;

        private string _lastClipboardText = string.Empty;
        private bool _handleCreated = false;
        private bool _paused = false;
        private bool _processing = false;
        private uint _lastSequence;
        private int _retryCount;
        private readonly System.Windows.Forms.Timer _pollTimer;
        private static readonly Regex WorkItemPattern =
            new(@"^(?:Bug|Task)\s+(\d+):\s+(.+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;

            // Safety net: WM_CLIPBOARDUPDATE can be missed (listener dropped after sleep/lock/RDP,
            // or clipboard locked by another app). Poll the sequence number to catch up and retry.
            _pollTimer = new System.Windows.Forms.Timer(components!) { Interval = 1000 };
            _pollTimer.Tick += PollTimer_Tick;
            _pollTimer.Start();

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
        {
            if (e.Reason is SessionSwitchReason.SessionUnlock or SessionSwitchReason.ConsoleConnect
                or SessionSwitchReason.RemoteConnect)
                BeginInvoke(ReRegisterListener);
        }

        private void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                BeginInvoke(ReRegisterListener);
        }

        private void ReRegisterListener()
        {
            if (!IsHandleCreated)
                return;

            RemoveClipboardFormatListener(Handle);
            AddClipboardFormatListener(Handle);
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
            _lastSequence = GetClipboardSequenceNumber();
            AddClipboardFormatListener(Handle);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // SystemEvents are static; unsubscribe to avoid leaking the form.
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            base.OnFormClosed(e);
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

        private void PollTimer_Tick(object? sender, EventArgs e)
        {
            if (!IsHandleCreated || GetClipboardSequenceNumber() == _lastSequence)
                return;

            // A change went unhandled: re-register the listener in case Windows dropped it.
            ReRegisterListener();
            ProcessClipboard();
        }

        private void ProcessClipboard()
        {
            uint sequence = GetClipboardSequenceNumber();
            if (_processing || sequence == _lastSequence)
                return;

            uint previousSequence = _lastSequence;
            _lastSequence = sequence;

            if (_paused)
                return;

            _processing = true;
            try
            {
                if (!Clipboard.ContainsText())
                    return;

                string text = Clipboard.GetText().Trim();
                _retryCount = 0;
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
                _lastSequence = GetClipboardSequenceNumber();

                labelLastTransformed.Text = transformed;
                labelStatus.Text = $"Transformed at {DateTime.Now:HH:mm:ss}";

                notifyIcon1.ShowBalloonTip(2000, "Clipboard transformed", transformed, ToolTipIcon.Info);
            }
            catch
            {
                // Clipboard access can fail when another app holds it. Mark the change as
                // unprocessed so the poll timer retries it, up to MaxRetries attempts.
                if (++_retryCount < MaxRetries)
                    _lastSequence = previousSequence;
                else
                    _retryCount = 0;
            }
            finally
            {
                _processing = false;
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
