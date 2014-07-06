using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Owin;
using Microsoft.Owin.Hosting;

using Owin;

using Scripl.Data;

namespace Scripl.Commands
{
    [Command("service")]
    internal class RunScriplService
    {
        public void Run()
        {
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.ReadLine();
            }
        }

        class Startup
        {
            public void Configuration(IAppBuilder app)
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
                            Console.WriteLine(ex);
                            throw;
                        }
                    });

                app.Use(new RunScriplService().Invoke);

#if DEBUG
                app.UseErrorPage();
#endif
            }
        }

        private async Task Invoke(IOwinContext context, Func<Task> next)
        {
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

        private static async Task RunCommandOnServer(IOwinContext context)
        {
            PathString commandNameSegment;
            context.Request.Path.StartsWithSegments(new PathString("/cli"), out commandNameSegment);
            var commandName = commandNameSegment.Value.Substring(1);

            var commandRunner = new CommandRunner(isService: true);
            commandRunner.Invoke(commandName, (await context.Request.ReadFormAsync()).OrderBy(pair => pair.Key).Select(pair => pair.Value.First()).ToArray());
        }
    }
}