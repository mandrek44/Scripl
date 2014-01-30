using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripl.Setup
{
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security.Principal;

    class Program
    {
        static void Main(string[] args)
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool administrativeMode = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!administrativeMode)
            {
                var startInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    FileName = Assembly.GetEntryAssembly().Location
                };

                Process.Start(startInfo);
                return;
            }


            using (var tempFiles = new TempFileCollection())
            using (Stream stream = typeof(Program).Assembly.GetManifestResourceStream("Scripl.Setup.scriplfiles.zip"))
            {
                Console.WriteLine("Writing temporary files...");
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                var zipPath = Path.Combine(Path.GetTempPath(), tempFiles.AddExtension("zip"));
                File.WriteAllBytes(zipPath, buffer);

                Console.WriteLine("Extracting files...");
                string extractPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                try
                {
                    Directory.CreateDirectory(extractPath);

                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

                    var fileName = Path.Combine(extractPath, "Scripl.ConsoleHost.exe");
                    Console.WriteLine("Installing....");
                    Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = false })
                           .WaitForExit();
                }
                finally
                {
                    if (Directory.Exists(extractPath))
                        new DirectoryInfo(extractPath).Delete(true);
                }

                Console.WriteLine("Finished.");
            }
        }
    }
}
