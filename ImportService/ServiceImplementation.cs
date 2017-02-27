using ImportService.Framework;
using System.ServiceProcess;
using System.IO;
using System;
using System.Threading;

namespace ImportService
{
    /// <summary>
    /// The actual implementation of the windows service goes here...
    /// </summary>
    [WindowsService("ImportService",
        DisplayName = "ImportService",
        Description = "The description of the ImportService service.",
        EventLogSource = "ImportService",
        StartMode = ServiceStartMode.Automatic)]
    public class ServiceImplementation : IWindowsService
    {
        FileSystemWatcher watcher = null;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        /// <summary>
        /// This method is called when the service gets a request to start.
        /// </summary>
        /// <param name="args">Any command line arguments</param>
        public void OnStart(string[] args)
        {
            try
            {
                watcher = new FileSystemWatcher();
                //watcher.Path = ConfigHelper.FetchStringValue("ImportFoler", true);
                watcher.Path = "C:\\FtpData";
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = false;

                watcher.Created += new FileSystemEventHandler(Watcher_Created);
                watcher.Changed += new FileSystemEventHandler(Watcher_Changed);
                watcher.Deleted += new FileSystemEventHandler(Watcher_Deleted);
                //watcher.Renamed += new FileSystemEventHandler(Watcher_Renamed);

                watcher.EnableRaisingEvents = true;

            }
            catch (System.Exception ex)
            {
                string msg = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// This method is called when the service gets a request to stop.
        /// </summary>
        public void OnStop()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;

        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (GetIdleFile(e.FullPath))
                {
                    // LOG: file found and data import started

                    BusinessLogic.ImportData(e.Name, e.FullPath);

                    // LOG: data import complete
                }
            }
            catch (Exception ex)
            {
                String msg = ex.Message;
                throw;
            }
        }

        private static bool GetIdleFile(string path)
        {
            var fileIdle = false;
            const int MaximumAttemptsAllowed = 30;
            var attemptsMade = 0;

            while (!fileIdle && attemptsMade <= MaximumAttemptsAllowed)
            {
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        fileIdle = true;
                    }
                }
                catch
                {
                    attemptsMade++;
                    Thread.Sleep(100);
                }
            }

            return fileIdle;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is called when a service gets a request to pause,
        /// but not stop completely.
        /// </summary>
        public void OnPause()
        {
        }

        /// <summary>
        /// This method is called when a service gets a request to resume 
        /// after a pause is issued.
        /// </summary>
        public void OnContinue()
        {
        }

        /// <summary>
        /// This method is called when the machine the service is running on
        /// is being shutdown.
        /// </summary>
        public void OnShutdown()
        {
        }

        /// <summary>
        /// This method is called when a custom command is issued to the service.
        /// </summary>
        /// <param name="command">The command identifier to execute.</param >
        public void OnCustomCommand(int command)
        {
        }
    }
}
