using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Scripl
{
    public class CommandsScanner
    {
        public static IDictionary<string, Type> FindCommands<TCommand>(Assembly assembly) where TCommand : CommandAttribute
        {
            return assembly.GetTypes().Where(type => CustomAttributeExtensions.GetCustomAttributes(type).Any(attr => attr.GetType() == typeof(TCommand))).ToDictionary(command => command.GetCustomAttributes().OfType<TCommand>().First().Name);
        }
    }
}