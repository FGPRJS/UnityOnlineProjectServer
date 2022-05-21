using System;
using System.Numerics;
using System.Threading;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Content.Map;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var isRun = true;

            ThreadPool.SetMinThreads(2, 1);

            GameServer server = new GameServer();

            Logger.Instance.InfoLog("Server turned on");
            Logger.Instance.InfoLog("---Server Spec Info---");
            Logger.Instance.InfoLog($"Processor Count : {Environment.ProcessorCount}");
            ThreadPool.GetAvailableThreads(out var worker, out var asyncIO);
            Logger.Instance.InfoLog($"Available WorkerThread Count : {worker}");
            Logger.Instance.InfoLog($"Available AsyncI/O Count : {asyncIO}");
            Logger.Instance.InfoLog($"ThreadPool Thread Count : {ThreadPool.ThreadCount}");

            while (isRun)
            {
                var Readed = Console.ReadLine();

                switch (Readed.ToLower())
                {
                    case "x":
                    case "exit":
                    case "shutdown":
                    case "quit":

                        isRun = false;
                        server.ShutDownServer();

                        break;

                    case "start":
                    case "on":
                    case "o":

#if debug
                        server.StartLocal();
#else
                        server.Start();
#endif
                        break;
                }
            }

        }
    }
}
