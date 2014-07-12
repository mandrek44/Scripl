namespace Scritpl.Utils.Contracts
{
    public interface IFileSystem
    {
        string ReadAllText(string sourceFileName);

        bool Exists(string fileName);

        void WriteAllText(string filePath, string content);

        string GetChecksum(string filePath);

        IFileSystemWatcher WatchFile(string fileName);

        string ReadAllTextRetrying(string sourceFileName, int retries = 20);
    }
}