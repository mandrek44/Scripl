using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting;

namespace Scripl.PortsOut
{
    public interface IFileSystemWatcher
    {
        object GetLifetimeService();

        object InitializeLifetimeService();

        ObjRef CreateObjRef(Type requestedType);

        void Dispose();

        IContainer Container { get; }

        NotifyFilters NotifyFilter { get; set; }

        bool EnableRaisingEvents { get; set; }

        string Filter { get; set; }

        bool IncludeSubdirectories { get; set; }

        int InternalBufferSize { get; set; }

        string Path { get; set; }

        ISite Site { get; set; }

        ISynchronizeInvoke SynchronizingObject { get; set; }

        event EventHandler Disposed;

        void BeginInit();

        void EndInit();

        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType);

        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout);

        event FileSystemEventHandler Changed;

        event FileSystemEventHandler Created;

        event FileSystemEventHandler Deleted;

        event ErrorEventHandler Error;

        event RenamedEventHandler Renamed;
    }
}