using System;
using System.IO;

namespace EDMCOverlay
{
    public class EDMCOverlay
    {
        public static Logger Logger => loggerInstance;

        static Logger loggerInstance = new Logger();

        public static void Main(string[] argv)
        {
            loggerInstance.Setup("edmcoverlay.log");
            try
            {
                OverlayRenderer renderer = new OverlayRenderer();

            #if DEBUG
                // let exceptions bubble!
            #else
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    loggerInstance.LogMessage("unhandled exception: " + sender.ToString());
                    Environment.Exit(0);
                };
            #endif

                new OverlayJsonServer(5010, renderer).Start();
            }
            catch (Exception err)
            {
                loggerInstance.LogMessage(err.ToString());
            }
        }
    }
}