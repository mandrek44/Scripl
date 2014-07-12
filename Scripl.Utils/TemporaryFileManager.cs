using System.CodeDom.Compiler;
using System.IO;

using Scritpl.Utils.Contracts;

namespace Scritpl.Utils
{
    class TemporaryFileManager : ITemporaryFileManager
    {
        readonly TempFileCollection _tempFileCollection = new TempFileCollection();

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        public string AddFileWithExtension(string extension, bool keepFile = false)
        {
            return _tempFileCollection.AddExtension(extension, keepFile);
        }

        public void AddFile(string filePath, bool keepFile = false)
        {
            _tempFileCollection.AddFile(filePath, keepFile);
        }
    }
}