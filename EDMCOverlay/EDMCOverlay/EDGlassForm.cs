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

namespace EDMCOverlay
{
    public class EDGlassForm : Form
    {
        public System.Diagnostics.Process Follow;

        private const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int attrLen);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }       

        public bool HalfSize { get; set; }

        public int XOffset { get; set; }
        public int YOffset { get; set; }

        Nullable<Point> forceLocation;
        Nullable<Size> forceSize;

        public void ForceGeometry(Point p, Size s)
        {
            forceLocation = p;
            forceSize = s;
        }

        public EDGlassForm(System.Diagnostics.Process follow)
        {            
            this.Opacity = 1.0; // Tweak as desired
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(100, 100);            
            this.DoubleBuffered = true;            

            this.Name = "EDMC Overlay Window";
            this.Text = this.Name;
            this.TopMost = true;
            this.TransparencyKey = Color.Black;

            int initialStyle = GetWindowLong(this.Handle, -20);
            //SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            this.Follow = follow;

            // Disable Aero transitions, the plexiglass gets too visible
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int value = 1;
                if (follow != null)
                    DwmSetWindowAttribute(follow.MainWindowHandle, DWMWA_TRANSITIONS_FORCEDISABLED, ref value, 4);
            }
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

                if (Process.GetCurrentProcess().Id != Follow.Id
                    && GetWindowRect(Follow.MainWindowHandle, ref window))
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
    }
}