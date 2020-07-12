﻿using System;
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
using System.Diagnostics;

namespace EDMCOverlay
{
    public class OverlayRenderer
    {
        public const string EDWindowName = "Elite - Dangerous(CLIENT)";
        public const string EDProgramName = "EliteDangerous64";
        public const int FPS = 20;

        public bool TestMode { get; set; }

        public const int VIRTUAL_ORIGIN_X = 20;
        public const int VIRTUAL_ORIGIN_Y = 40;
        public const int VIRTUAL_WIDTH = 1280;
        public const int VIRTUAL_HEIGHT = 1024;

        Logger Logger = Logger.GetInstance(typeof(OverlayRenderer));
        
        public bool ForceRender { get; set; }
        public bool HalfSize { get; set; }
        public EDGlassForm Glass { get; set; }
        public Dictionary<String, InternalGraphic> Graphics { get; set; }

        public Nullable<Point> ForceLocation { get; set; }
        public Nullable<Size> ForceSize { get; set; }

        private bool run = true;

        public System.Diagnostics.Process GetGame()
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(EDProgramName).FirstOrDefault();
            if (process == null)
            {
                Logger.LogMessage(String.Format("Can't find running {0}", EDProgramName));
                return null;
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


        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = WindowUtils.GetForegroundWindow();

            if (WindowUtils.GetWindowText(handle, Buff, nChars) > 0)
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
                { GraphicType.FONT_LARGE, new Font(fonts.Families[0], (float)19.0, FontStyle.Bold) },
                { GraphicType.FONT_NORMAL, normalFont }
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
                    int a = 0xff;
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    Color newcolour = default(Color);
                    bool defined = true;
                    switch(colour.Length)
                    {
                        case 7:
                            // #rrggbb
                            r = Convert.ToInt32(colour.Substring(1, 2), 16);
                            g = Convert.ToInt32(colour.Substring(3, 2), 16);
                            b = Convert.ToInt32(colour.Substring(5, 2), 16);                
                            break;
                        case 9:
                            // #aarrggbb
                            a = Convert.ToInt32(colour.Substring(1, 2), 16);
                            r = Convert.ToInt32(colour.Substring(3, 2), 16);
                            g = Convert.ToInt32(colour.Substring(5, 2), 16);
                            b = Convert.ToInt32(colour.Substring(7, 2), 16);                            
                            break;
                        default:
                            defined = false;
                            break;
                    }

                    if (defined)
                    {
                        newcolour = Color.FromArgb(a, r, g, b);
                        colours.Add(colour, new SolidBrush(newcolour));
                    }
                }

                if (colours.TryGetValue(colour, out brush))
                    return brush;

            } catch (Exception ignore)
            {
                Logger.LogMessage(String.Format("Exception: {0}", ignore));
            }
            return null;
        }

        private void Clear(Graphics draw)
        {
            if (Glass.InvokeRequired)
            {
                Glass.Invoke(new Action(() => { Clear(draw); }));
                return;
            }

            draw.Clear(Color.Black);
        }

        private void Draw(Graphics draw)
        {
            if (Glass.InvokeRequired)
            {
                Glass.Invoke(new Action(() => { Draw(draw); }));
                return;
            }

            // this causes flickering
            Clear(draw);
            foreach (var id in Graphics.Keys.ToArray())
            {
                var gfx = Graphics[id];                
                Graphic g = gfx.RealGraphic;

                if (gfx.Expired)
                {
                    Graphics.Remove(id);
                    continue;
                }

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
            
        }
        
        BufferedGraphics back = null;
        Graphics canvas = null;

        void allocateBuffers(BufferedGraphicsContext bufctx)
        {
            if (this.Glass == null) return;

            lock (this) {
                if (back != null)
                {                    
                    back.Dispose();
                }
                if (canvas == null)
                {
                    canvas = this.Glass.CreateGraphics();
                }
                canvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                back = bufctx.Allocate(canvas, this.Glass.DisplayRectangle);
            }
        }

        void swapBuffers(BufferedGraphicsContext bufctx)
        {
            if (Glass.InvokeRequired)
            {
                Glass.Invoke(new Action(() => { swapBuffers(bufctx); }));
                return;
            }

            if ( back != null)
            {
                back.Render();
            }
        }

        Graphics getDraw()
        {
            if (back != null)
            {
                return back.Graphics;
            }
            return null;
        }

        private void StartUpdate()
        {
            var bufg = BufferedGraphicsManager.Current;            
            DateTime lastframe = DateTime.Now;
            Graphics draw = null;
            Logger.LogMessage("Starting update loop");

            double fixedwait = (1000 / FPS); // number of msec to wait assuming zero time to draw frame

            while (this.run)
            {
                if (back == null) allocateBuffers(bufg);
                TimeSpan elapsed = DateTime.Now.Subtract(lastframe);

                double wait = fixedwait - elapsed.TotalMilliseconds;                
                if (wait > 0)
                    System.Threading.Thread.Sleep((int)fixedwait);
                
                if (Glass == null)
                    System.Threading.Thread.Sleep(1000);

                if (Glass != null)
                {
                    Glass.XOffset = VIRTUAL_ORIGIN_X;
                    Glass.YOffset = VIRTUAL_ORIGIN_Y;

                    if (this.ForceLocation.HasValue && this.ForceSize.HasValue)
                    {
                        Glass.ForceGeometry(this.ForceLocation.Value, this.ForceSize.Value);
                    }

                    if (Glass.Follow != null && Glass.Follow.HasExited)
                    {
                        Logger.LogMessage(String.Format("{0} has exited. quitting.", Glass.Follow.ProcessName));
                        System.Environment.Exit(0);
                    }
                    
                    if (draw == null)
                    {
                        draw = getDraw();
                    }

                    if (Graphics == null || draw == null)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    IntPtr activeWindow = WindowUtils.GetForegroundWindow();

                    bool foreground = (activeWindow == Glass.Follow.MainWindowHandle);

                    if (foreground)
                    {
                        Debug.WriteLine(DateTime.Now + " window foreground");
                    } else { 
                        Debug.WriteLine(DateTime.Now + " window obscured");
                    }

                    bool render = (foreground && (Graphics.Values.Count > 0)) || this.ForceRender;

                    if (render)
                    {
                        lock (Graphics)
                        {
                            Draw(draw);
                            swapBuffers(bufg);
                            Glass.FollowWindow();
                        }
                    } else {
                        // nothing to draw, clear and sleep a long sleep                        
                        Clear(draw);
                        swapBuffers(bufg);
                        Thread.Sleep(1000);                    
                    }
                    lastframe = DateTime.Now;
                }
            }
        }

        Size GetClientSize()
        {
            if (this.Glass != null)
            {
                return this.Glass.ClientSize;
            }

            var sz = new Size(1, 1);
            return sz;
        }

        Point Scale(int x, int y)
        {
            Point p = new Point();
            Size csize = GetClientSize();
            double x_factor = csize.Width / (double)(VIRTUAL_WIDTH + 32);
            double y_factor = csize.Height / (double)(VIRTUAL_HEIGHT + 18);

            p.X = (int)Math.Round(x * x_factor);
            p.Y = (int)Math.Round(y * y_factor);

            return p;
        }

        Point ScalePosition(int x, int y)
        {
            Point scaled = Scale(x, y);            
            return scaled;
        }

        private void DrawMarker(Graphics draw, VectorPoint marker)
        {
            if (String.IsNullOrWhiteSpace(marker.Color)) return;
            Brush brush = GetBrush(marker.Color);
            if (brush == null) return;

            Pen p = new Pen(brush);
            if ( marker.Marker.Equals("cross"))
            {
                // draw 2 lines
                draw.DrawLine(p, ScalePosition(marker.X - 3, marker.Y - 3), ScalePosition(marker.X + 3, marker.Y + 3));
                draw.DrawLine(p, ScalePosition(marker.X + 3, marker.Y - 3), ScalePosition(marker.X - 3, marker.Y + 3));
            }
            if ( marker.Marker.Equals("circle"))
            {
                var circlebounds = new Rectangle(ScalePosition(marker.X - 4, marker.Y - 4),
                    new Size(Scale(8, 8)));
                draw.DrawEllipse(p, circlebounds);
            }
        }

        private void DrawVectorLine(Graphics draw, Brush brush, VectorPoint start, VectorPoint end)
        {
            if (brush == null) return;
            Pen p = new Pen(brush);
            draw.DrawLine(p, ScalePosition(start.X, start.Y), Scale(end.X, end.Y));
        }

        private void DrawVector(Graphics draw, Graphic start, bool erase)
        {
            // draw first point
            if (start.Vector == null) return;
            if (start.Vector.Length < 1) return;

            var last = start.Vector.First();

            for (int i = 1; i < start.Vector.Length; i++)
            {
                var current = start.Vector[i];
                DrawVectorLine(draw, GetBrush(start.Color), last, current);
                DrawMarker(draw, last);
                DrawTextEx(draw, GraphicType.FONT_NORMAL, last.Color, last.Text, last.X + 2, last.Y + 7);
                last = current;
            }

            // draw last marker
            DrawMarker(draw, last);
            DrawTextEx(draw, GraphicType.FONT_NORMAL, last.Color, last.Text, last.X + 2, last.Y + 7);
        }        

        private void DrawVector(Graphics draw, Graphic start)
        {
            DrawVector(draw, start, false);
        }
        
        private void DrawShape(Graphics draw, Graphic g)
        {
            if (g.Shape.Equals(GraphicType.SHAPE_RECT))
            {
                Rectangle shapeRect = new Rectangle(
                    ScalePosition(g.X, g.Y),
                    new Size(Scale(g.W, g.H)));

                Brush fill = GetBrush(g.Fill);
                if (fill != null)
                {                    
                    draw.FillRectangle(fill, shapeRect);
                }

                Brush paint = GetBrush(g.Color);
                if (paint != null) {                     
                    draw.DrawRectangle(new Pen(paint), shapeRect);
                }
            } else
            {
                if (g.Shape.Equals(GraphicType.SHAPE_VECT))
                {
                    // a vector line
                    DrawVector(draw, g);
                }
            }
        }

        private void DrawText(Graphics draw, Graphic g)
        {
            
            DrawTextEx(draw, g.Size, g.Color, g.Text, g.X, g.Y);
        }
        
        private void DrawTextEx(Graphics draw, String fontsize, String fontcolor, String text, int x, int y)
        {
            if (String.IsNullOrWhiteSpace(text)) return;
            int textwidth = 8 * text.Length;
            Point loc = ScalePosition(x, y);
            Size csize = GetClientSize();
            if ( loc.Y > csize.Height - 20)
            {
                loc.Y -= 25;
            }
            if ( loc.X > csize.Width - 10 - textwidth)
            {
                loc.X -= 10 + textwidth;
            }

            Font size = normalFont;
            if (fontsize != null)
                fontSizes.TryGetValue(fontsize, out size);
            Brush paint = GetBrush(fontcolor);
            
            if (paint != null)
            {
                draw.DrawString(text, size, paint, (float)loc.X, (float)loc.Y);
            }
        }
    }
}