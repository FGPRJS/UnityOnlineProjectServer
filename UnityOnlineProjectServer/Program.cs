#define debug

using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Content.Map;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(2, 1);

            Logger.Instance.InfoLog("Server turned on");
            Logger.Instance.InfoLog("---Server Spec Info---");
            Logger.Instance.InfoLog($"Processor Count : {Environment.ProcessorCount}");
            ThreadPool.GetAvailableThreads(out var worker, out var asyncIO);
            Logger.Instance.InfoLog($"Available WorkerThread Count : {worker}");
            Logger.Instance.InfoLog($"Available AsyncI/O Count : {asyncIO}");
            Logger.Instance.InfoLog($"ThreadPool Thread Count : {ThreadPool.ThreadCount}");

            Thread trd = new Thread(() =>
            {
                var isRun = true;

                GameServer server = new GameServer();

                while (isRun)
                {
                    var readed = Console.ReadLine();

                    switch (readed?.ToLower())
                    {
                        case "s":
                        case "start":

#if debug
                            server.StartLocal();
#else
                            server.Start();
#endif

                            break;

                        case "x":
                        case "stop":

                            isRun = false;

                            break;

                        default:
                            continue;
                    }
                }
            });

            trd.Start();
        }
    }
}
