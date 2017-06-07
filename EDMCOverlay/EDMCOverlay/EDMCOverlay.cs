using System;
using System.Diagnostics;
using System.Linq;

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
            rect.Shape = "rect";
            rect.X = 200;
            rect.Y = 100;
            rect.W = 150;
            rect.H = 70;
            rect.TTL = 10;
            rect.Color = "#780000ff";
            rect.Fill = "#660000ff";
            rect.Id = "rectangle";

            while(true)
            {
                System.Threading.Thread.Sleep(100);
                test.Text = String.Format("Hello {0}", i++);
                test.Id = "test1";
                test.TTL = 3;
                test.X = 2*i % 100;
                test.Y = i % 100;
                test.Color = "red";

                server.SendGraphic(test, 1);
                server.SendGraphic(rect, 1);
                
            }
        }

        public static void Main(string[] argv)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Logger.LogMessage(String.Format("unhandled exception!!: {0} {1}", sender, args));
                Environment.Exit(1);
            };
            String appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            String edmc = System.IO.Path.Combine(appdata, "EDMarketConnector");
            String plugins = System.IO.Path.Combine(edmc, "plugins");

            Logger.Setup(System.IO.Path.Combine(plugins, "edmcoverlay.log"));
            Logger.LogMessage("starting..");
            Logger.Subsystem = typeof(EDMCOverlay);
            try
            {
                if (argv.Length > 0)
                {
                    if (argv[0].Equals("--test"))
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(TestThread);
                    }
                }

                OverlayRenderer renderer = new OverlayRenderer();
                server = new OverlayJsonServer(5010, renderer);

                System.Threading.ThreadPool.QueueUserWorkItem((x) => server.Start());

                EDGlassForm glass = new EDGlassForm(renderer.GetGame());
                renderer.Glass = glass;
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