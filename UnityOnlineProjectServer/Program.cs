#define debug

using System;
using System.Numerics;
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

            GameServer server = new GameServer();

            Console.WriteLine("Server turned on");

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
                        server.ShutDownAllClients();

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
