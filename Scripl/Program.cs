using System.Linq;

namespace Scripl
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandName = "service"; ;
            var commandArgs = args;

            if (args.Length != 0 && !args.First().StartsWith("-"))
            {
                commandName = args.First();
                commandArgs = args.Skip(1).ToArray();
            }

            new CommandRunner().Invoke(commandName, commandArgs);
        }
    }
}
