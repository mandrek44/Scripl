using System;

using Raven.Client.Embedded;

using Scripl.Contracts;

using Scritpl.Utils.Contracts;

namespace Scripl.RavenDb
{
    public class SourceCodeRepository : ISourceCodeRepository
    {
        private readonly Lazy<EmbeddableDocumentStore> _lazdyDocumentStore;

        public SourceCodeRepository(IUserSettings userSettings)
        {
            _lazdyDocumentStore = new Lazy<EmbeddableDocumentStore>(
                () =>
                {
                    var documentStore = new EmbeddableDocumentStore { DataDirectory = userSettings.DataDirectory };
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