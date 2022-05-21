using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using log4net.Config;
using UnityOnlineProjectServer.Connection;

namespace UnityOnlineProjectServer.Utility
{
    public class Logger
    {
        private const string loggerConfigDirectory = "./config.log4net";

        private static Logger instance;
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }

        private readonly ILog logger;

        public Logger()
        {
            logger = LogManager.GetLogger(typeof(GameServer));
            XmlConfigurator.Configure(new System.IO.FileInfo(loggerConfigDirectory));
        }


        public void DebugLog(string message)
        {
            logger.Debug(message);
        }

        public void InfoLog(string message)
        {
            logger.Info(message);
        }

        public void WarningLog(string message)
        {
            logger.Warn(message);
        }

        public void ErrorLog(string message)
        {
            logger.Error(message);
        }
    }
}
