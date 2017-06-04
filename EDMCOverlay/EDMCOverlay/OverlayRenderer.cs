using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Process = System.Diagnostics.Process;
using System.Drawing;

namespace EDMCOverlay
{
    public class OverlayRenderer
    {
        public const string EDProgramName = "EliteDangerous64";
        public const int FPS = 30;

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
                this.Update();
            });
        }


        private void Update()
        {
            Graphics draw = null;

            Font normal = new Font(FontFamily.GenericSansSerif, (float)14.0, FontStyle.Regular);
            Font large = new Font(FontFamily.GenericSansSerif, (float)19.0, FontStyle.Bold);

            Brush red = new SolidBrush(Color.Red);
            Brush yellow = new SolidBrush(Color.Yellow);
            Brush green = new SolidBrush(Color.Green);
            Brush blue = new SolidBrush(Color.Blue);

            Dictionary<String, Brush> colours = new Dictionary<string, Brush>
            {
                { "red", red },
                { "yellow", yellow },
                { "green", green },
                { "blue", blue },
            };

            while (this.run)
            {
                // _controller.Update();
                if (Glass != null)
                {
                    if (draw == null)
                        draw = Glass.CreateGraphics();

                    if (Graphics == null) continue;

                    draw.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                    lock (Graphics)
                    {
                        Glass.BeginInvoke(new Action(() =>
                        {
                            draw.Clear(Color.Black);
                            foreach (var gfx in Graphics.Values)
                            {
                                Graphic g = gfx.RealGraphic;
                                Font size = normal;
                                if (g.Size != null && g.Size.Equals("large"))
                                {
                                    size = large;
                                }
                                Brush paint = null;
                                if (colours.TryGetValue(g.Color, out paint))
                                {
                                    draw.DrawString(gfx.RealGraphic.Text, size, paint, (float)g.X, (float)g.Y);
                                }
                            }


                            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(
                               50, 10, 115, 115);
                            draw.DrawEllipse(System.Drawing.Pens.Red, rectangle);
                            draw.DrawRectangle(System.Drawing.Pens.Green, rectangle);
                        }));                    
                    }

                    Glass.FollowWindow();
                }
                
                System.Threading.Thread.Sleep(1000 / FPS);
            }
        }
    }
}