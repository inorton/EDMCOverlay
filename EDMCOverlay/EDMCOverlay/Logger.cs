using System;
using System.IO;

namespace EDMCOverlay
{
    public class Logger
    {
        public string LogFile { get; private set; }

        public void Setup(String logPath)
        {
            LogFile = logPath;
            using (var ffs = new FileStream(LogFile, FileMode.OpenOrCreate))
            {

            }
        }

        public void LogMessage(String msg)
        {
            using (var ffs = new FileStream(LogFile, FileMode.Append))
            using (var fos = new StreamWriter(ffs))
            {
                fos.WriteLine(msg);
            }
        }
    }
}