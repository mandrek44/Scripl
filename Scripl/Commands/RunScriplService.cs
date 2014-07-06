using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Owin;
using Microsoft.Owin.Hosting;

using NLog;

using Owin;

using Scripl.Data;

namespace Scripl.Commands
{
    [Command("service")]
    internal class RunScriplService
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private readonly IServiceAddressProvider _addressProvider;

        public RunScriplService(IServiceAddressProvider addressProvider)
        {
            _addressProvider = addressProvider;
        }

        public void Run()
        {
            

            using (Start())
            {
                _log.Trace("Scripl host started");
                Console.ReadLine();

                _log.Trace("Scripl host exiting");
            }
        }

        public IDisposable Start()
        {
            _log.Trace("Starting scripl host with address " + _addressProvider.GetAddress());
            return WebApp.Start(new StartOptions(_addressProvider.GetAddress()) , AppBuilder);
        }

        private void AppBuilder(IAppBuilder app)
        {
            app.Use(
                async (context, next) =>
                {
                    try
                    {
                        await next();
                    }
                    catch (Exception ex)
                    {
                        _log.Trace(ex);
                        throw;
                    }
                });

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
                HealthCheck(context);
            }
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments(new PathString("/cli")))
            {
                await RunCommandOnServer(context);
            }
            else
            {
                await next();
            }
        }

        private static void HealthCheck(IOwinContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.Write("OK");
        }

        private async Task RunCommandOnServer(IOwinContext context)
        {
            PathString commandNameSegment;
            context.Request.Path.StartsWithSegments(new PathString("/cli"), out commandNameSegment);
            var commandName = commandNameSegment.Value.Substring(1);

            var commandRunner = new CommandRunner(isService: true);
            commandRunner.Invoke(commandName, (await context.Request.ReadFormAsync()).OrderBy(pair => pair.Key).Select(pair => pair.Value.First()).ToArray());
        }
    }
}