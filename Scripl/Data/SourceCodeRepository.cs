using Raven.Client.Embedded;

namespace Scripl.Data
{
    public class SourceCodeRepository
    {
        public static SourceCodeRepository Instance = new SourceCodeRepository();
        private EmbeddableDocumentStore _documentStore;

        private SourceCodeRepository()
        {
            
        }

        private void InitializeDataStore()
        {
            lock (Instance)
            {
                if (_documentStore == null)
                {
                    //C:\Src\public\Scripl\Scripl\bin\Debug\Data
                    _documentStore = new EmbeddableDocumentStore { DataDirectory = Properties.Settings.Default.DataDirectory };
                    _documentStore.Initialize();
                }
            }
        }

        public SourceCode LoadSourceCode(string checksum)
        {
            InitializeDataStore();
            using (var session = _documentStore.OpenSession())
            {
                return session.Load<SourceCode>(checksum);
            }
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