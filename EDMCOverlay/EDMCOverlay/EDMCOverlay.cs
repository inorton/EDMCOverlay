using System;
using System.Diagnostics;
using System.Linq;
using Process = System.Diagnostics.Process;

namespace EDMCOverlay
{
    public class EDMCOverlay
    {
        public static Logger Logger => loggerInstance;

        static Logger loggerInstance = new Logger();

        public static void Main(string[] argv)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Environment.Exit(1);
            };
            loggerInstance.Setup("edmcoverlay.log");
            try
            {
                OverlayRenderer renderer = new OverlayRenderer();
                new OverlayJsonServer(5010, renderer).Start();
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