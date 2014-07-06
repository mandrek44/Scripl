using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Scripl.Tests
{
    /*
     * Command parser
     * contruction -> loaded types
     *  input -> command name
     *  output -> implementation
     * 
     *  Dictionary<string, Type>
     */

    public class StaticCommandFinder
    {
        [Test]
        public void ReturnsAssemblyCommandTypes()
        {
            var commands = CommandsScanner.FindCommands<CommandAttribute>(Assembly.GetExecutingAssembly());

            
            Assert.That(commands.Values, Contains.Item(typeof(DummyCommand)));
        }

        [Test]
        public void AssignsNameToCommandTypes()
        {
            var commands = CommandsScanner.FindCommands<CommandAttribute>(Assembly.GetExecutingAssembly());

            var commandType = commands.Values.First(command => command == typeof(DummyCommand));
            var commandName = commandType.GetCustomAttributes().OfType<CommandAttribute>().First().Name;

            Assert.That(commands[commandName], Is.EqualTo(typeof(DummyCommand)));
        }
    }


    public interface ICommand
    {
        
    }

    [Command("dummy")]
    public class DummyCommand : ICommand
    {
        
    }
}
