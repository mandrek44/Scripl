using Raven.Client.Embedded;

namespace Scripl.Data
{
    class SourceCodeRepository
    {
        public static SourceCodeRepository Instance = new SourceCodeRepository();

        private SourceCodeRepository()
        {
            
        }

        public EmbeddableDocumentStore _documentStore;

        private void InitializeDataStore()
        {
            lock (Instance)
            {
                if (_documentStore == null)
                {
                    _documentStore = new EmbeddableDocumentStore { DataDirectory = @"C:\Src\public\Scripl\Scripl\bin\Debug\Data" };
                    _documentStore.Initialize();
                }
            }
        }

        public SourceCode LoadSourceCode(string checksum)
        {
            InitializeDataStore();
            SourceCode sourceCode;
            using (var session = _documentStore.OpenSession())
            {
                sourceCode = session.Load<SourceCode>(checksum);
            }

            return sourceCode;
        }

        public void SaveSourceCode(SourceCode sourceCode)
        {
            InitializeDataStore();
            using (var session = _documentStore.OpenSession())
            {
                session.Store(sourceCode);
                session.SaveChanges();
            }
        }
    }
}