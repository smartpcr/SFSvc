

namespace SimpleApi
{
    using System;
    using System.IO;

    /// <summary>
    /// 
    /// </summary>
    public class FileLogger
    {
        private static readonly string logFile;

        static FileLogger()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SimpleApi");
            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }
            var logFolder = Path.Combine(appData, "Log");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            logFile = Path.Combine(logFolder, string.Format("{0:yyyyMMdd-hhmmss}.err", DateTime.Now));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            try
            {
                if (!File.Exists(logFile))
                {
                    File.WriteAllText(logFile, string.Format("Started at {0}\n", DateTime.Now));
                }

                using (var writer = File.AppendText(logFile))
                {
                    writer.Write(msg);
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Error(ex.ToString());
            }
        }
    }
}
