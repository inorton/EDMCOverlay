using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using EDMCOverlay.Properties;
using System.Reflection;

namespace EDMCOverlay
{
    public class EDGlassForm : Form
    {
        public System.Diagnostics.Process Follow;
        
     

        public bool HalfSize { get; set; }

        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public Boolean standalone { get; set; }

        Nullable<Point> forceLocation;
        Nullable<Size> forceSize;
        
        bool windowInitialized;

        public void ForceGeometry(Point p, Size s)
        {
            forceLocation = p;
            forceSize = s;
        }

        public EDGlassForm(System.Diagnostics.Process follow, Boolean standalone)
        {
            this.standalone = standalone;
            this.Opacity = 1.0; // Tweak as desired
            this.BackColor = Color.Black;
            this.StartPosition = FormStartPosition.Manual;
            this.AutoScaleMode = AutoScaleMode.None;
            this.DoubleBuffered = true;          
            this.ClientSize = new Size(100, 100);
            var version = Assembly.GetEntryAssembly().GetName().Version;
            this.Name = $"EDMC Overlay {version}";
            this.TransparencyKey = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            if (this.standalone) {
                this.ControlBox = false;
                //this.Text = String.Empty;
                this.Text = this.Name;
                this.ShowInTaskbar = true;
                this.TopMost = false;
            } else {
                this.ControlBox = false;
                this.Text = this.Name;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                
                int initialStyle = WindowUtils.GetWindowLong(this.Handle, WindowUtils.GWL_EXSTYLE);
                // makes window click-trough
                WindowUtils.SetWindowLong(this.Handle, WindowUtils.GWL_EXSTYLE, 
                    initialStyle | WindowUtils.WS_EX_LAYERED | WindowUtils.WS_EX_TRANSPARENT | WindowUtils.WS_EX_NOACTIVATE );
            }
            this.Follow = follow;

            // Disable Aero transitions, the plexiglass gets too visible
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int value = 1;
                if (follow != null)
                    WindowUtils.DwmSetWindowAttribute(follow.MainWindowHandle, WindowUtils.DWMWA_TRANSITIONS_FORCEDISABLED, ref value, 4);
            }
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        private const int HTBOTTOMRIGHT = 0x11;
        public const int cGrip = 16;

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == WM_NCHITTEST)
            {  
                Point pos = new Point(message.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    message.Result = (IntPtr)HTBOTTOMRIGHT;
                    return;
                }
                else if (pos.X <= cGrip && pos.Y <= cGrip)
                {
                    message.Result = (IntPtr)HTCAPTION;
                    return;
                }
                else
                {
                    message.Result = (IntPtr)HTCAPTION;
                    return;
                }
            }
            base.WndProc(ref message);
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FollowWindow();

            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |                
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            if (this.standalone)
            {
                if (Settings.Default.WindowPosition != Rectangle.Empty && IsVisibleOnAnyScreen(Settings.Default.WindowPosition))
                {
                    // first set the bounds
                    this.StartPosition = FormStartPosition.Manual;
                    this.DesktopBounds = Settings.Default.WindowPosition;

                    this.WindowState = Settings.Default.WindowState;
                } else
                {
                    this.StartPosition = FormStartPosition.WindowsDefaultLocation;

                    if (Settings.Default.WindowPosition != Rectangle.Empty)
                    {
                        this.Size = Settings.Default.WindowPosition.Size;
                    }
                }
            }
            windowInitialized = true;
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (! this.standalone) 
            {
                return;
            }
            // only save the WindowState if Normal or Maximized
            switch (this.WindowState)
            {
                case FormWindowState.Normal:
                case FormWindowState.Maximized:
                    Settings.Default.WindowState = this.WindowState;
                    break;

                default:
                    Settings.Default.WindowState = FormWindowState.Normal;
                    break;
            }

            Settings.Default.Save();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            TrackWindowState();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            TrackWindowState();
        }

        // On a move or resize in Normal state, record the new values as they occur.
        // This solves the problem of closing the app when minimized or maximized.
        private void TrackWindowState()
        {
            // Don't record the window setup, otherwise we lose the persistent values!
            if (!windowInitialized) { return; }

            if (WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowPosition = this.DesktopBounds;
            }
        }


        private bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return true;
                }
            }
            return false;
        }



        public void FollowWindow()
        {
            if (Follow == null)
            {                
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { FollowWindow(); }));
            }
            else
            {
                RECT window = new RECT();
                Point pos = new Point(300, 300);
                Size siz = new Size(640, 400);

                this.TopMost = true;

                if (Process.GetCurrentProcess().Id != Follow.Id
                    && WindowUtils.GetWindowRect(Follow.MainWindowHandle, ref window))
                {
                    pos = new Point(window.Left + this.XOffset, window.Top + this.YOffset);
                    siz = new Size(
                        window.Right - window.Left - (2 * this.XOffset),
                        window.Bottom - window.Top - (2 * this.YOffset));

                    if (HalfSize)
                    {
                        pos.X = siz.Width / 3;
                        pos.Y = siz.Height / 3;

                        siz.Height = siz.Height / 2;
                        siz.Width = siz.Width / 2;
                    }
                }

                if (forceLocation.HasValue && forceSize.HasValue)
                {
                    pos = forceLocation.Value;
                    siz = forceSize.Value;
                }

                this.Location = pos;
                this.ClientSize = siz;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {

        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            
        }

        /* 
         * protected override void OnClosing(CancelEventArgs e)
        {
            if (this.standalone)
            {
                Settings.Default.WindowState = (int)this.WindowState;
                Settings.Default.WnidowLocation = this.Location;

                // Copy window size to app settings
                if (this.WindowState == FormWindowState.Normal)
                {
                    Settings.Default.WindowSize = this.Size;
                }
                else
                {
                    Settings.Default.WindowSize = this.RestoreBounds.Size;
                }

                // Save settings
                Settings.Default.Save();
            }

            base.OnClosing(e);
        } */
    }
}