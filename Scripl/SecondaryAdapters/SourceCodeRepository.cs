using System;

using Raven.Client.Embedded;

using Scripl.Data;
using Scripl.PortsOut;

namespace Scripl.Adapters
{
    public class SourceCodeRepository : ISourceCodeRepository
    {
        public static ISourceCodeRepository Instance = new SourceCodeRepository();
        
        private readonly Lazy<EmbeddableDocumentStore> _lazdyDocumentStore;

        public SourceCodeRepository()
        {
            _lazdyDocumentStore = new Lazy<EmbeddableDocumentStore>(
                () =>
                {
                    var documentStore = new EmbeddableDocumentStore { DataDirectory = Properties.Settings.Default.DataDirectory };
                    documentStore.Initialize();

                    return documentStore;
                });
        }

        public SourceCode LoadSourceCode(string checksum)
        {
            using (var session = _lazdyDocumentStore.Value.OpenSession())
            {
                return session.Load<SourceCode>(checksum);
            }
        }

        public void SaveSourceCode(SourceCode sourceCode)
        {
            using (var session = _lazdyDocumentStore.Value.OpenSession())
            {
                session.Store(sourceCode);
                session.SaveChanges();
            }
        }
    }
}