using System;

namespace Scripl
{
    public class CommandAttribute : Attribute
    {

        public CommandAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}