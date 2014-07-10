using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

using Scripl.NotStructured;
using Scripl.PortsOut;

namespace Scripl.Adapters.OS
{
    class PhysicalFileSystem : IFileSystem
    {
        public string ReadAllText(string sourceFileName)
        {
            return File.ReadAllText(sourceFileName);
        }

        public string ReadAllTextRetrying(string sourceFileName, int retries = 20)
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
                    
                    Thread.Sleep(500);
                }
            }

            return null;
        }

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

        public void WriteAllText(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        public string GetChecksum(string filePath)
        {
            using (var stream = new BufferedStream(File.OpenRead(filePath)))
            {
                var checksum = new SHA256Managed().ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        public IFileSystemWatcher WatchFile(string fileName)
        {
            return new PhysicalFileSystemWatcherWrapper(
                new FileSystemWatcher(Path.GetDirectoryName(fileName)) { Filter = Path.GetFileName(fileName), NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName }
                );
        }
    }
}