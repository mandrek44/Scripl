using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

using Autofac;

namespace Scripl
{
    class Program
    {
        static void Main(string[] args)
        {
            string commandName;
            string[] commandArgs;
            FindCommand(args, out commandName, out commandArgs);

            new CommandRunner().Invoke(commandName, commandArgs);
        }

        private static void FindCommand(string[] args, out string commandName, out string[] commandArgs)
        {
            if (args.Length == 0 || args.First().StartsWith("-"))
            {
                // Run service
                commandName = "service";
                commandArgs = args;
            }
            else
            {
                commandName = args.First();
                commandArgs = args.Skip(1).ToArray();

                // run the command
            }
        }
    }
}
