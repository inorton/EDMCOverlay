using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace EDMCOverlay
{
    public class EDMCOverlay
    {
        public static Logger Logger = Logger.GetInstance();
        static OverlayJsonServer server;
        
        private static void TestThread(Object obj)
        {
            OverlayRenderer xr = (OverlayRenderer)obj;
            int i = 0;
            Graphic test = new Graphic();

            Graphic rect = new Graphic();
            rect.Shape = GraphicType.SHAPE_RECT;
            rect.X = 200;
            rect.Y = 100;
            rect.W = 150;
            rect.H = 70;
            rect.TTL = 10;
            rect.Color = "#780000ff";
            rect.Fill = "#660000ff";
            rect.Id = "rectangle";

            Graphic bounds = new Graphic();
            bounds.Shape = "vect";
            bounds.Color = "#aaff00";
            bounds.Id = "bounds";
            bounds.TTL = 10;            
            bounds.Vector = new VectorPoint[] {
                new VectorPoint() {
                    Color = "#00ff00",
                    Text = "NE",
                    Marker = "cross",
                    X = 1279,
                    Y = 0,
                },
                new VectorPoint() {
                    Color = "#00ff00",
                    Text = "SE",
                    Marker = "cross",
                    X = 1279,
                    Y = 1023,
                },
                new VectorPoint() {
                    Color = "#00ff00",
                    Text = "SW",
                    Marker = "cross",
                    X = 0,
                    Y = 1023,
                },
                new VectorPoint() {
                    Color = "#00ff00",
                    Text = "NW",
                    Marker = "cross",
                    X = 0,
                    Y = 1,
                },
            };

            List<Graphic> markers = new List<Graphic>();

            // some markers
            for (int m = 0; m < 32; m++) {
                Graphic g = new Graphic();
                g.Shape = "vect";
                g.Color = "#ff2200";
                g.TTL = 2;
                g.Id = Guid.NewGuid().ToString();
                g.Vector = new VectorPoint[]
                {
                    new VectorPoint()
                    {
                        Color = g.Color,
                        Marker = (m % 2 == 0) ? "cross" : "circle",
                        X= 130 + m * 8,
                        Y = 345 - m * 8,
                    },
                };
                markers.Add(g);
            }


            Graphic vectorline = new Graphic();
            vectorline.Shape = "vect";
            vectorline.Color = "#cdcd00";
            vectorline.Id = "graph";
            vectorline.TTL = 10;
            vectorline.Vector = new VectorPoint[]
            {
                new VectorPoint() {
                    Color = "#00ff00",
                    Text = "Point 1",
                    Marker = "cross",
                    X = 100,
                    Y = 400,
                },
                new VectorPoint() {
                    Color = "#ff0000",
                    Text = "Point 2",
                    Marker = "cross",
                    X = 200,
                    Y = 410,
                },
                new VectorPoint() {
                    Color = "#ffff00",
                    Text = "Point 3",
                    Marker = "circle",
                    X = 300,
                    Y = 490,
                },
                new VectorPoint() {
                    Color = "#ff00ff",
                    Text = "Point 4",
                    Marker = "cross",
                    X = 400,
                    Y = 410,
                }
            };

            var box = new Graphic()
            {
                Shape = "rect",
                Id = "box",
                X = -1,
                Y = -1,
                W = 1281,
                H = 1025,
                Color = "#00ffff",
                TTL = 2,                
            };

            var overflow = new Graphic()
            {
                Shape = "rect",
                Id = "overflow",
                TTL = 2,
                X = 900,
                Y = 600,
                W = 500,
                H = 600,
                Color = "#ff00ff"
            };

            var underflow = new Graphic()
            {
                Shape = "rect",
                Id = "underflow",
                TTL = 2,
                X = -200,
                Y = -200,
                W = 400,
                H = 400,
                Color = "#00ff22"
            };

            int colorcycle = 0;

            while (true)
            {
                System.Threading.Thread.Sleep(100);
                test.Text = String.Format("Hello {0}", i++);
                test.Id = "test1";
                test.TTL = 3;
                test.X = 2 * i % 100;
                test.Y = i % 200;
                test.Color = String.Format("#{0:X6}", colorcycle);

                colorcycle += 3 + (255 * test.X);
                
                colorcycle = colorcycle % 0xffffff;

                server.SendGraphic(bounds, 1);
                server.SendGraphic(test, 1);
                server.SendGraphic(rect, 1);
                server.SendGraphic(vectorline, 1);
                foreach (var m in markers)
                {
                    server.SendGraphic(box, 2);
                    server.SendGraphic(m, 1);
                    server.SendGraphic(overflow, 2);
                    server.SendGraphic(underflow, 2);
                }
            }
        }

        public static void Main(string[] argv)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Logger.LogMessage(String.Format("unhandled exception!!: {0} {1}", sender, args));
                Logger.LogMessage(((Exception)args.ExceptionObject).StackTrace);
                Environment.Exit(1);
            };

            String appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            String edmc = System.IO.Path.Combine(appdata, "EDMarketConnector");

            Logger.Setup(System.IO.Path.Combine(edmc, "edmcoverlay.log"));
            Logger.LogMessage("starting..");
            Logger.Subsystem = typeof(EDMCOverlay);
            try
            {
                OverlayRenderer renderer = new OverlayRenderer();

                foreach (var arg in argv)
                {
                    if (arg.Equals("--test"))
                    {
                        renderer.TestMode = true;
                        System.Threading.ThreadPool.QueueUserWorkItem(TestThread);
                    }

                    if (arg.Equals("--foreground"))
                    {
                        renderer.ForceRender = true;
                    }

                    if (arg.Equals("--half"))
                    {
                        renderer.HalfSize = true;
                    }
                }            
                server = new OverlayJsonServer(5010, renderer);
                System.Threading.ThreadPool.QueueUserWorkItem((x) => server.Start());

                EDGlassForm glass = new EDGlassForm(renderer.GetGame());
                renderer.Glass = glass;
                glass.HalfSize = renderer.HalfSize;
                renderer.Graphics = server.Graphics;
                System.Windows.Forms.Application.Run(renderer.Glass);
            }
            catch (Exception err)
            {
                Logger.LogMessage(String.Format("exiting!: {0}", err.ToString()));
                Environment.Exit(0);
            }
        }
    }
}
