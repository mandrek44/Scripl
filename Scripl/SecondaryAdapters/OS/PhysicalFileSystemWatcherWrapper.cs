using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting;

using Scripl.PortsOut;

namespace Scripl.SecondaryAdapters.OS
{
    class PhysicalFileSystemWatcherWrapper : IFileSystemWatcher
    {
        private FileSystemWatcher _fileSystemWatcher;

        public PhysicalFileSystemWatcherWrapper(FileSystemWatcher fileSystemWatcher)
        {
            _fileSystemWatcher = fileSystemWatcher;
        }

        public object GetLifetimeService()
        {
            return _fileSystemWatcher.GetLifetimeService();
        }

        public object InitializeLifetimeService()
        {
            return _fileSystemWatcher.InitializeLifetimeService();
        }

        public ObjRef CreateObjRef(Type requestedType)
        {
            return _fileSystemWatcher.CreateObjRef(requestedType);
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }

        public IContainer Container
        {
            get
            {
                return _fileSystemWatcher.Container;
            }
        }

        public event EventHandler Disposed
        {
            add
            {
                _fileSystemWatcher.Disposed += value;
            }
            remove
            {
                _fileSystemWatcher.Disposed -= value;
            }
        }

        public void BeginInit()
        {
            _fileSystemWatcher.BeginInit();
        }

        public void EndInit()
        {
            _fileSystemWatcher.EndInit();
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
        {
            return _fileSystemWatcher.WaitForChanged(changeType);
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
        {
            return _fileSystemWatcher.WaitForChanged(changeType, timeout);
        }

        public NotifyFilters NotifyFilter
        {
            get
            {
                return _fileSystemWatcher.NotifyFilter;
            }
            set
            {
                _fileSystemWatcher.NotifyFilter = value;
            }
        }

        public bool EnableRaisingEvents
        {
            get
            {
                return _fileSystemWatcher.EnableRaisingEvents;
            }
            set
            {
                _fileSystemWatcher.EnableRaisingEvents = value;
            }
        }

        public string Filter
        {
            get
            {
                return _fileSystemWatcher.Filter;
            }
            set
            {
                _fileSystemWatcher.Filter = value;
            }
        }

        public bool IncludeSubdirectories
        {
            get
            {
                return _fileSystemWatcher.IncludeSubdirectories;
            }
            set
            {
                _fileSystemWatcher.IncludeSubdirectories = value;
            }
        }

        public int InternalBufferSize
        {
            get
            {
                return _fileSystemWatcher.InternalBufferSize;
            }
            set
            {
                _fileSystemWatcher.InternalBufferSize = value;
            }
        }

        public string Path
        {
            get
            {
                return _fileSystemWatcher.Path;
            }
            set
            {
                _fileSystemWatcher.Path = value;
            }
        }

        public ISite Site
        {
            get
            {
                return _fileSystemWatcher.Site;
            }
            set
            {
                _fileSystemWatcher.Site = value;
            }
        }

        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                return _fileSystemWatcher.SynchronizingObject;
            }
            set
            {
                _fileSystemWatcher.SynchronizingObject = value;
            }
        }

        public event FileSystemEventHandler Changed
        {
            add
            {
                _fileSystemWatcher.Changed += value;
            }
            remove
            {
                _fileSystemWatcher.Changed -= value;
            }
        }

        public event FileSystemEventHandler Created
        {
            add
            {
                _fileSystemWatcher.Created += value;
            }
            remove
            {
                _fileSystemWatcher.Created -= value;
            }
        }

        public event FileSystemEventHandler Deleted
        {
            add
            {
                _fileSystemWatcher.Deleted += value;
            }
            remove
            {
                _fileSystemWatcher.Deleted -= value;
            }
        }

        public event ErrorEventHandler Error
        {
            add
            {
                _fileSystemWatcher.Error += value;
            }
            remove
            {
                _fileSystemWatcher.Error -= value;
            }
        }

        public event RenamedEventHandler Renamed
        {
            add
            {
                _fileSystemWatcher.Renamed += value;
            }
            remove
            {
                _fileSystemWatcher.Renamed -= value;
            }
        }
    }
}