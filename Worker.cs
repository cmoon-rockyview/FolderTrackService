using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileTrackService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WorkerSettings _settings;
        private FileSystemWatcher _watcher;

        //How to use parameters
        //https://chat.openai.com/c/976019e6-e4b6-4c5f-a9e0-4c7d7f9ee48b

        public Worker(ILogger<Worker> logger, WorkerSettings settings)
        {
            _logger = logger;
            _settings = settings;
            InitializeFileSystemWatcher();
        }

        private void InitializeFileSystemWatcher()
        {
            _watcher = new FileSystemWatcher
            {
                Path = _settings.FolderPath,
         
                NotifyFilter = NotifyFilters.Attributes
                                       | NotifyFilters.CreationTime
                                       | NotifyFilters.DirectoryName
                                       | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName,
                                     
                                      
            Filter = "*.*"
            };


            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Changed;
            _watcher.Deleted += Watcher_Changed;
            _watcher.Renamed += Watcher_Renamed;

            _watcher.EnableRaisingEvents = true;

            // Include subdirectories in the watch.
            _watcher.IncludeSubdirectories = true;
        }

        private void LogToFile(string message)
        {
            string path = _settings.ChangeLogFile;
            File.AppendAllText(path, message + Environment.NewLine);
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
      
            // Specify what is done when a file or a directory is renamed.
            if (Directory.Exists(e.FullPath))
            {
                string message = $"Directory: {e.OldFullPath} renamed to {e.FullPath} at: {DateTimeOffset.Now}";
                _logger.LogInformation(message);
                LogToFile(message);
            }

            //else
            //{
            //    string message = $"File: {e.OldFullPath} renamed to {e.FullPath} at {DateTime.Now}"; ;
            //    _logger.LogInformation(message);
            //    LogToFile(message);
            //}
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
      

            if (Directory.Exists(e.FullPath))
            {
                string directoryName = Path.GetFileName(e.FullPath);

              
                string message = $"Directory: {e.FullPath} {e.ChangeType} at {DateTime.Now}";
                _logger.LogInformation(message);
                LogToFile(message);
                Console.WriteLine($"Directory: {e.FullPath} {e.ChangeType}");
                

            }

            //else if (!e.FullPath.ToLower().Contains("changelog"))
            //{
            //    string message = $"File: {e.FullPath} {e.ChangeType} at {DateTime.Now}";
            //    _logger.LogInformation(message);
            //    LogToFile(message);
            //    Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            //}
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string message = $"File: {e.FullPath} deleted at: {DateTimeOffset.Now}";
            _logger.LogInformation(message);
            LogToFile(message);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                #region //Test
                //_logger.LogInformation("Worker running with {FolderPath} and {ChangeLogFile} at: {time}",
                //_settings.FolderPath,
                //_settings.ChangeLogFile,
                //DateTimeOffset.Now);
                #endregion

                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();

            return base.StopAsync(cancellationToken);
        }

    }
}
