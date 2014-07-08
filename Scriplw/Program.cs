using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scriplw
{
    class Program
    {
        static void Main(string[] args)
        {
            var scriplPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "scripl.exe");
            var arguments = string.Join(" ", args.Select(arg => arg.Contains(" ") ? "\"" + arg + "\"" : arg));
            var processStartInfo = new ProcessStartInfo(scriplPath)
                                   {
                                       Arguments = arguments, 
                                       CreateNoWindow = true,
                                       UseShellExecute = true,
                                       WindowStyle = ProcessWindowStyle.Hidden
                                   };

            Process.Start(processStartInfo);
        }
    }
}
