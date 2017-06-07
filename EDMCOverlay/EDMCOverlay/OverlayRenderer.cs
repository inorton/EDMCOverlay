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
using System.Drawing.Text;

namespace EDMCOverlay
{
    public class OverlayRenderer
    {
        public const string EDWindowName = "Elite - Dangerous(CLIENT)";
        public const string EDProgramName = "EliteDangerous64";
        public const int FPS = 20;

        public const int VIRTUAL_WIDTH = 1280;
        public const int VIRTUAL_HEIGHT = 960;

        Logger Logger = Logger.GetInstance(typeof(OverlayRenderer));
        
        public EDGlassForm Glass { get; set; }
        public Dictionary<String, InternalGraphic> Graphics { get; set; }

        private bool run = true;

        public System.Diagnostics.Process GetGame()
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(EDProgramName).FirstOrDefault();
            if (process == null)
            {
                Logger.LogMessage(String.Format("Can't find running {0}", EDProgramName));
                System.Environment.Exit(0);
            }
            return process;
        }

        public void Start(OverlayJsonServer service)
        {
            LoadFonts();
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

        PrivateFontCollection fonts = new PrivateFontCollection();
        Font normalFont;
        Dictionary<String, Font> fontSizes;

        public void LoadFonts()
        {
            Logger.LogMessage("Loading fonts..");
            byte[] data = Properties.Resources.EUROCAPS;
            IntPtr ptr = Marshal.AllocCoTaskMem(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            fonts.AddMemoryFont(ptr, data.Length);

            normalFont = new Font(fonts.Families[0], (float)12.0, FontStyle.Regular);
            fontSizes = new Dictionary<string, Font>
            {
                { "large", new Font(fonts.Families[0], (float)19.0, FontStyle.Bold) },
                { "normal", normalFont }
            };
        }


        Dictionary<String, Brush> colours = new Dictionary<string, Brush>
            {
                { "red", new SolidBrush(Color.Red) },
                { "yellow", new SolidBrush(Color.Yellow) },
                { "green", new SolidBrush(Color.Green) },
                { "blue", new SolidBrush(Color.Blue) },
                { "black", new SolidBrush(Color.Black) },
            };


        private Brush GetBrush(String colour)
        {
            Brush brush = null;

            if (String.IsNullOrWhiteSpace(colour)) return null;
            
            if (colours.TryGetValue(colour, out brush))
            {
                return brush;
            }

            try
            {
                if (colour.StartsWith("#"))
                {                         
                    if (colour.Length == 7) // #rrggbb
                    {
                        int r = Convert.ToInt32(colour.Substring(1, 2), 16);
                        int g = Convert.ToInt32(colour.Substring(3, 2), 16);
                        int b = Convert.ToInt32(colour.Substring(5, 2), 16);

                        Color newcolour = Color.FromArgb(r, g, b);
                        colours.Add(colour, new SolidBrush(newcolour));

                    }
                    if (colour.Length == 9) // #aarrggbb
                    {
                        int a = Convert.ToInt32(colour.Substring(1, 2), 16);
                        int r = Convert.ToInt32(colour.Substring(3, 2), 16);
                        int g = Convert.ToInt32(colour.Substring(5, 2), 16);
                        int b = Convert.ToInt32(colour.Substring(7, 2), 16);

                        Color newcolour = Color.FromArgb(a, r, g, b);
                        colours.Add(colour, new SolidBrush(newcolour));
                    }
                }
            } catch (Exception ignore)
            {
                Logger.LogMessage(String.Format("Exception: {0}", ignore));
            }
            return null;
        }

        private void StartUpdate()
        {
            Graphics draw = null;
            Logger.LogMessage("Starting update loop");
            while (this.run)
            {
                if (Glass != null)
                {
                    if (draw == null)
                    {
                        draw = Glass.CreateGraphics();
                        draw.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
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

                                    if (!String.IsNullOrEmpty(g.Shape))
                                    {
                                        DrawShape(draw, g);
                                    }
                                    else
                                    {
                                        if (!String.IsNullOrEmpty(g.Text))
                                        {
                                            DrawText(draw, g);
                                        }
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

        

        Point Scale(int x, int y)
        {
            Point p = new Point();

            double x_factor = (double)(VIRTUAL_WIDTH) / this.Glass.ClientSize.Width;
            double y_factor = (double)(VIRTUAL_HEIGHT) / this.Glass.ClientSize.Height;

            p.X = (int)Math.Round(x * x_factor);
            p.Y = (int)Math.Round(y * y_factor);

            return p;
        }

        private void DrawShape(Graphics draw, Graphic g)
        {
            Point position = Scale(g.X, g.Y);

            if (g.Shape.Equals("rect"))
            {
                Brush fill = GetBrush(g.Fill);
                if (fill != null)
                {                    
                    draw.FillRectangle(fill, g.X, g.Y, g.W, g.H);
                }

                Brush paint = GetBrush(g.Color);
                if (paint != null) { 
                    Pen p = new Pen(paint);
                    Point size = Scale(g.W, g.H);
                    draw.DrawRectangle(p, g.X, g.Y, g.W, g.H);
                }
            }
        }

        private void DrawText(Graphics draw, Graphic g)
        {
            Font size = normalFont;
            if (g.Size != null)
                fontSizes.TryGetValue(g.Size, out size);
            Brush paint = GetBrush(g.Color);
            if (paint != null)
                draw.DrawString(g.Text, size, paint, (float)g.X, (float)g.Y);
        }
    }
}