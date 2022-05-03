﻿#define debug

using System;
using UnityOnlineProjectServer.Connection;


namespace UnityOnlineProjectServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var isRun = true;
            Server server = new Server();

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
                        server.ShutDown();

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
