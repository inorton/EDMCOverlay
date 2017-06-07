using System;
using System.IO;

namespace EDMCOverlay
{
    public class Logger
    {
        static int MAX_LOG_BYTES = 1024 * 1024 * 5;

        public string LogFile { get; private set; }

        public Type Subsystem { get; set; }

        public void Setup(String logPath)
        {
            LogFile = logPath;
            if (System.IO.File.Exists(LogFile))
            {
                var size = new System.IO.FileInfo(LogFile).Length;
                if (size > MAX_LOG_BYTES)
                {
                    System.IO.File.Delete(LogFile);
                }
            }

            using (var ffs = new FileStream(LogFile, FileMode.OpenOrCreate))
            {

            }
        }

        public void LogMessage(String msg)
        {
            if (LogFile != null)
            {
                try
                {
                    using (var ffs = new FileStream(LogFile, FileMode.Append))
                    using (var fos = new StreamWriter(ffs))
                    {
                        lock (instance)
                        {
                            fos.Write(DateTime.Now.ToString("s"));
                            if (Subsystem != null)
                            {
                                fos.Write(" {0}:", Subsystem.Name);
                            }
                            fos.WriteLine(" {0}", msg);
                        }
                    }
                } catch (Exception fail)
                {
                    // cant log? read-only logfile?
                    Console.Error.WriteLine(fail);
                }
            }
        }

        static Logger instance = null;
        public static Logger GetInstance()
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }

        public static Logger GetInstance(Type subsys)
        {
            var sub = new Logger();
            sub.Setup(GetInstance().LogFile);
            sub.Subsystem = subsys;
            return sub;
        }
    }
}