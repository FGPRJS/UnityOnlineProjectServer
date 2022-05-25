using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Content.Map;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<GameServer>();
                });


        static void Main(string[] args)
        {
            bool isDebug = false;

            if ((args.Length > 0) && (args[0].ToLower() == "debug"))
            {
                isDebug = true;
            }

            ThreadPool.SetMinThreads(4, 1);

            Logger.Instance.InfoLog("Server start with main");
            Logger.Instance.InfoLog("---Server Spec Info---");
            Logger.Instance.InfoLog($"Processor Count : {Environment.ProcessorCount}");
            ThreadPool.GetAvailableThreads(out var worker, out var asyncIO);
            Logger.Instance.InfoLog($"Available WorkerThread Count : {worker}");
            Logger.Instance.InfoLog($"Available AsyncI/O Count : {asyncIO}");
            Logger.Instance.InfoLog($"ThreadPool Thread Count : {ThreadPool.ThreadCount}");

            var server = new GameServer();

            Logger.Instance.InfoLog("Server process ON.");

            bool isRun = true;

            if (isDebug)
            {
                server.StartLocal();
            }
            else
            {
                server.Start();
            }

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
