using System;
using System.Collections.Concurrent;
namespace FastAPI.Net
{
    public class Logger
    {
        static ConcurrentQueue<(string,string)> logs = new ConcurrentQueue<(string,string)>();
        static System.IO.StreamWriter filewriter = new System.IO.StreamWriter(GetLogName());
        static System.Threading.Thread runThread = new System.Threading.Thread(LogLoop);
        static bool started = false;
        static void TryStart()
        {
            if (!started)
            {
                runThread.Start();
                started = true;
            }
        }
        public static void Log(string message)
        {
            TryStart();
            logs.Enqueue((message, "LOG"));
        }
        public static void Error(Exception e)
        {
            TryStart();
            logs.Enqueue((e.ToString(), "ERR"));
        }
        public static void Warning(string message)
        {
            TryStart();
            logs.Enqueue((message, "WAR"));
        }
        static void LogLoop()
        {
            while (true)
            {
                try
                {
                    if(logs.TryDequeue(out var s))
                    {
                        filewriter.Write(Format(s.Item1, s.Item2));
                        filewriter.Write("\n");
                        filewriter.Flush();
                    }
                }
                catch(Exception e)
                {
                    Error(e);
                }
            }
        }
        static string Format(string message,string tag)
        {
            return $"[{tag}][{DateTime.Now.ToLongTimeString()}]: {message}";
        }
        static string GetLogName()
        {
            var now = DateTime.Now;
            return $"Log_{now.Day}_{now.Month}_{now.Hour}_{now.Second}";
        }
    }
}
