﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

using NLog;

namespace Scripl
{
    class Program
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => _log.Fatal(eventArgs.ExceptionObject);

            if (Environment.UserInteractive)
            {
                var commandName = "service";
                var commandArgs = args;

                if (args.Length != 0 && !args.First().StartsWith("-"))
                {
                    commandName = args.First();
                    commandArgs = args.Skip(1).ToArray();
                }

                new CommandRunner().Invoke(commandName, commandArgs);
            }
            else
            {
                ServiceBase.Run(new WindowsService());
            }
        }
    }
}
