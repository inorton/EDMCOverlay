using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Process = System.Diagnostics.Process;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace EDMCOverlay
{
    public class OverlayRenderer
    {
        public const string EDWindowName = "Elite - Dangerous(CLIENT)";
        public const string EDProgramName = "EliteDangerous64";
        public const int FPS = 20;

        private System.Diagnostics.Process _game;

        public EDGlassForm Glass { get; set; }
        public Dictionary<String, InternalGraphic> Graphics { get; set; }

        private bool run = true;

        private Thread renderThread;
        
        public bool Attached
        {
            get { return _game != null && !_game.HasExited; }
        }


        public System.Diagnostics.Process GetGame()
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(EDProgramName).FirstOrDefault();
            if (process == null)
            {
                Console.WriteLine("ED not running");
                System.Environment.Exit(0);
            }
            return process;
        }

        public void Start(OverlayJsonServer service)
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                this.StartUpdate();
            });
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }


        private void StartUpdate()
        {
            Graphics draw = null;

            Font normal = new Font(FontFamily.GenericMonospace, (float)14.0, FontStyle.Regular);
            Font large = new Font(FontFamily.GenericMonospace, (float)19.0, FontStyle.Bold);

            Dictionary<String, Brush> colours = new Dictionary<string, Brush>
            {
                { "red", new SolidBrush(Color.Red) },
                { "yellow", new SolidBrush(Color.Yellow) },
                { "green", new SolidBrush(Color.Green) },
                { "blue", new SolidBrush(Color.Blue) },
                { "black", new SolidBrush(Color.Black) },
            };

            Dictionary<String, Font> fontSizes = new Dictionary<string, Font>
            {
                { "large", large },                
            };

            
            while (this.run)
            {
                if (Glass != null)
                {
                    if (draw == null)
                    {
                        draw = Glass.CreateGraphics();
                        draw.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    }

                    if (Graphics == null) continue;


                    String title = GetActiveWindowTitle();
                    IntPtr activeWindow = GetForegroundWindow();

                    if (activeWindow != Glass.Follow.MainWindowHandle)
                    {
                        Glass.BeginInvoke(new Action(() =>
                        {
                            draw.Clear(Color.Black);
                        }));
                    } else {
                        lock (Graphics)
                        {
                            Glass.BeginInvoke(new Action(() =>
                            {
                                Glass.TopMost = true;
                                draw.Clear(Color.Black);
                                foreach (var id in Graphics.Keys.ToArray())
                                {
                                    var gfx = Graphics[id];
                                    if (gfx.Expired)
                                    {
                                        Graphics.Remove(id);
                                        continue;
                                    }
                                    
                                    Graphic g = gfx.RealGraphic;
                                    Font size = normal;
                                    if (g.Size != null)
                                        fontSizes.TryGetValue(g.Size, out size);                                    
                                    Brush paint = null;
                                    if (colours.TryGetValue(g.Color, out paint))
                                    {
                                        draw.DrawString(gfx.RealGraphic.Text, size, paint, (float)g.X, (float)g.Y);
                                    }
                                }
                            }));
                        }

                        Glass.FollowWindow();
                    }
                }
                
                System.Threading.Thread.Sleep(1000 / FPS);
            }
        }
    }
}