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
                new OverlayJsonServer(5010, renderer).Start();
            }
            catch (Exception err)
            {
                loggerInstance.LogMessage(err.ToString());
            }
        }
    }
}