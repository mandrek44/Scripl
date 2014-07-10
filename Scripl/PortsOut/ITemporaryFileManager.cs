namespace Scripl.PortsOut
{
    public interface ITemporaryFileManager
    {
        string GetTempFileName();

        string AddFileWithExtension(string extension, bool keepFile = false);

        void AddFile(string filePath, bool keepFile = false);
    }
}