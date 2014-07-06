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
                var checksum = new SHA256Managed().ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        public static string ReadAllTextRetrying(string sourceFileName, int retries = 20)
        {
            for (int i = 0; i < retries; ++i)
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