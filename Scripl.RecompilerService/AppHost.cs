namespace Scripl.RecompilerService
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;

    using Funq;

    using Scripl.SelfInstall;

    using ServiceStack.OrmLite;
    using ServiceStack.WebHost.Endpoints;

    public class AppHost : AppHostHttpListenerBase
    {
        private readonly InstallerPaths _paths;

        public AppHost(InstallerPaths paths)
            : base("Scripl.RecompilerService", typeof(AppHost).Assembly)
        {
            _paths = paths;
        }

        public override void Configure(Container container)
        {
            var ormLiteConnectionFactory = new OrmLiteConnectionFactory(
                string.Format("Data Source={0};Version=3", Path.Combine((string)AppDomain.CurrentDomain.GetData("DataDirectory"), "scripl.db")),
                autoDisposeConnection: false,
                dialectProvider: SqliteDialect.Provider);

            using (var db = ormLiteConnectionFactory.OpenDbConnection())
            {
                db.CreateTableIfNotExists<ScriplHash>();
            }

            container.Register(ormLiteConnectionFactory);
            container.Register(new TempFileCollection());
            container.Register(_paths);
        }
    }
}