using System;
using System.IO;

namespace EDMCOverlay
{
    public class Logger
    {
        static object instanceLock = new object();

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

        public FileStream logFileStream;
        public StreamWriter logStream;

        public void Setup(Logger parent)
        {
            this.LogFile = parent.LogFile;
            this.logFileStream = parent.logFileStream;
            this.logStream = parent.logStream;
        }

        public void LogMessage(String msg)
        {            
            Console.Error.WriteLine(msg);
            if (LogFile != null)
            {
                try
                {
                    lock (instance)
                    { 
                        //using (var ffs = )
                        //using (var fos = new StreamWriter(ffs))
                        if (logStream == null) {
                            logFileStream = new FileStream(LogFile, FileMode.Append);
                            logStream = new StreamWriter(logFileStream);
                        }

                        if (logStream != null)
                        {
                            logStream.Write(DateTime.Now.ToString("s"));
                            if (Subsystem != null)
                            {
                                logStream.Write(" {0}:", Subsystem.Name);
                            }
                            logStream.WriteLine(" {0}", msg);
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
            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
            }
            return instance;
        }

        public static Logger GetInstance(Type subsys)
        {
            var sub = new Logger();
            sub.Setup(GetInstance());
            sub.Subsystem = subsys;
            return sub;
        }
    }
}