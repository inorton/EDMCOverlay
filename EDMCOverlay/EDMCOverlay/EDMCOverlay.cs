using System;
using System.Diagnostics;
using System.Linq;

namespace EDMCOverlay
{
    public class EDMCOverlay
    {
        public static Logger Logger => loggerInstance;

        static Logger loggerInstance = new Logger();
        static OverlayJsonServer server;


        private static void TestThread(Object obj)
        {
            OverlayRenderer xr = (OverlayRenderer)obj;
            int i = 0;
            Graphic test = new Graphic();
    
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
            }
        }

        public static void Main(string[] argv)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Environment.Exit(1);
            };
            loggerInstance.Setup("edmcoverlay.log");
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
                try
                {
                    loggerInstance.LogMessage(err.ToString());
                }
                catch (Exception unhandled)
                {
                    // logger problem?
                }
                Environment.Exit(0);
            }
        }
    }
}