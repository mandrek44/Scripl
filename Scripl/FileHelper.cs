using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Scripl.Commands
{
    internal class FileHelper
    {
        public static string GetChecksum(string file)
        {
            using (var stream = new BufferedStream(File.OpenRead(file)))
            {
                byte[] checksum = new SHA256Managed().ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public static string SafeReadAllText(string sourceFileName)
        {
            for (int i = 0; i < 20; ++i)
            {
                try
                {
                    return File.ReadAllText(sourceFileName);
                }
                catch (IOException)
                {
                    if (i == 19) throw;

                    Console.WriteLine("Unable to read file, trying again...");
                    Thread.Sleep(500);
                }
            }

            return null;
        }
    }
}