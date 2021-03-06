﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Owin;
using Microsoft.Owin.Hosting;

using NLog;

using Owin;

using Scripl.Utils.Contracts;

namespace Scripl.Runtime
{
    internal class NetworkEndpoint
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        
        private readonly IServiceAddressProvider _addressProvider;
        private readonly CommandRunner _commandRunner;

        public NetworkEndpoint(IServiceAddressProvider addressProvider, CommandRunner commandRunner)
        {
            _addressProvider = addressProvider;
            _commandRunner = commandRunner;
        }

        public IDisposable Start()
        {
            _log.Trace("Starting scripl host with address " + _addressProvider.GetAddress());
            return WebApp.Start(new StartOptions(_addressProvider.GetAddress()) , AppBuilder);
        }

        private void AppBuilder(IAppBuilder app)
        {
            app.Use(Invoke);

#if DEBUG
            app.UseErrorPage();
#endif
        }

        private async Task Invoke(IOwinContext context, Func<Task> next)
        {
            _log.Trace("{0} {1}", context.Request.Method, context.Request.Path);
            if (context.Request.Method == "GET" && context.Request.Path.Equals(new PathString("/running")))
            {
                await HealthCheck(context);
            }
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments(new PathString("/cli")))
            {
                var result = await RunCommandOnServer(context);
                context.Response.Write(result);
            }
            else
            {
                await next();
            }
        }

        private static async Task HealthCheck(IOwinContext context)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        }

        private async Task<string> RunCommandOnServer(IOwinContext context)
        {
            PathString commandNameSegment;
            context.Request.Path.StartsWithSegments(new PathString("/cli"), out commandNameSegment);
            var commandName = commandNameSegment.Value.Substring(1);

            var returnObj = _commandRunner.Invoke(commandName, (await GetCommandArguments(context)));
            return (returnObj ?? string.Empty).ToString();
        }

        private static async Task<string[]> GetCommandArguments(IOwinContext context)
        {
            return (await context.Request.ReadFormAsync()).OrderBy(pair => pair.Key).Select(pair => pair.Value.First()).ToArray();
        }
    }
}