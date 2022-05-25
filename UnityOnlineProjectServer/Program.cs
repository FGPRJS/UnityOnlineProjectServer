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
            ThreadPool.SetMinThreads(3, 1);

            Logger.Instance.InfoLog("Server turned on");
            Logger.Instance.InfoLog("---Server Spec Info---");
            Logger.Instance.InfoLog($"Processor Count : {Environment.ProcessorCount}");
            ThreadPool.GetAvailableThreads(out var worker, out var asyncIO);
            Logger.Instance.InfoLog($"Available WorkerThread Count : {worker}");
            Logger.Instance.InfoLog($"Available AsyncI/O Count : {asyncIO}");
            Logger.Instance.InfoLog($"ThreadPool Thread Count : {ThreadPool.ThreadCount}");

            var server = new GameServer();

            Logger.Instance.InfoLog("Server process ON.");

            bool isRun = true;
            bool isDebug = false;

            if((args.Length > 0) && (args[0].ToLower() == "debug"))
            {
                isDebug = true;
            }

            if(isDebug)
                server.StartLocal();
            else
                server.Start();

            ManualResetEvent _quitEvent = new ManualResetEvent(false);

            server.ServerShutdownEvent += (sender, arg) =>
            {
                _quitEvent.Set();
            };

            _quitEvent.WaitOne();


            Logger.Instance.InfoLog("Server process OFF");
        }
    }
}
