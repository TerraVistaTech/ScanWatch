using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ScanWatch.Properties;

namespace ScanWatch
{
    /// <summary>
    /// Framework for running application as a tray app.
    /// </summary>
    /// <remarks>
    /// Tray app code adapted from "Creating Applications with NotifyIcon in Windows Forms", Jessica Fosler,
    /// http://windowsclient.net/articles/notifyiconapplications.aspx
    /// </remarks>
    public class NotificationIconContext : ApplicationContext
    {
        private IContainer _components;
        private NotifyIcon _notifyIcon;
        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher(Settings.Default.ScanDirectory);
        private readonly LogViewForm _logViewForm = new LogViewForm();
        private readonly string _iconTooltip = "Check for new files from scanner.";
        private Scheduler scheduler = new Scheduler();

#if DEBUG
        private FileStream fs;
#endif

        public NotificationIconContext() 
		{
            AddLog("Starting up...");
		    InitializeContext();
		    AddLog("Ready.");
#if DEBUG
		    AddLog("Debug version");
#endif
        }

        private void AddLog(string line)
        {
            _logViewForm.AddLog(line);
        }

        private void ContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            _notifyIcon.ContextMenuStrip.Items.Clear();

            var openFolderItem = new ToolStripMenuItem("&Open scan folder");
            openFolderItem.Click += OpenFolderItemOnClick;
            _notifyIcon.ContextMenuStrip.Items.Add(openFolderItem);

#if DEBUG
            var unlockItem = new ToolStripMenuItem("&Unlock file");
            unlockItem.Click += (o, args) => fs?.Close();
            _notifyIcon.ContextMenuStrip.Items.Add(unlockItem);
#endif

            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            var logItem = new ToolStripMenuItem("&Show Log");
            logItem.Click += (o, args) => _logViewForm.Show();
            _notifyIcon.ContextMenuStrip.Items.Add(logItem);

            var restartItem = new ToolStripMenuItem("&Restart App");
            restartItem.Click += RestartItemOnClick;
            _notifyIcon.ContextMenuStrip.Items.Add(restartItem);

            var exitItem = new ToolStripMenuItem("&Exit");
            exitItem.Click += (o, args) => ExitThread();
            _notifyIcon.ContextMenuStrip.Items.Add(exitItem);
        }

        private void RestartItemOnClick(object sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath);
            ExitThread();
        }
        
        private void OpenFolderItemOnClick(object sender, EventArgs e)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C start \"OpenDirectory\" \"{Settings.Default.ScanDirectory}\""
            };

            process.StartInfo = startInfo;
            process.Start();
        }
        
        private void NotifyIconMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                if (mi != null) mi.Invoke(_notifyIcon, null);
            }
        }

        private void InitializeContext()
        {
            _components = new Container();

            _notifyIcon = new NotifyIcon(_components) {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = Resources.Scanner,
                Text = _iconTooltip,
                Visible = true
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStripOpening;
            _notifyIcon.MouseUp += NotifyIconMouseUp;
            
            AddLog($"Filtering for: {Settings.Default.FilenameFilter}");
            AddLog($"Scan directory: {Settings.Default.ScanDirectory}");
            AddLog($"Maximum wait time (secs): {Settings.Default.MaxWaitSeconds}");

            if (Settings.Default.AttemptOpenOnCreate)
            {
                _fileSystemWatcher.Created += FileSystemWatcherOnCreated;
            }
            _fileSystemWatcher.Changed += FileSystemWatcherOnCreated;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Filter = Settings.Default.FilenameFilter;
        }

        private string _lastProcessed;
        private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(500); // Let the FS settle.

#if DEBUG
            //fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.None);
#endif
            if (new FileInfo(e.FullPath).Length == 0 || _lastProcessed == e.FullPath)
            {
                return;
            }

            _lastProcessed = e.FullPath;

            OpenFile(e.FullPath, Settings.Default.MaxWaitSeconds);
        }

        private void OpenFile(string filePath, int triesLeft)
        {
            try
            {
                if (IsFileLocked(filePath))
                {
                    if (triesLeft == 0)
                    {
                        AddLog($"Unable to open file; the file was still locked after {Settings.Default.MaxWaitSeconds} seconds.");
                        return;
                    }

                    scheduler.Execute(() => OpenFile(filePath, triesLeft - 1), 1000);
                    return;
                }

                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = $"/C start \"Scan\" \"{filePath}\""
                };

                process.StartInfo = startInfo;

                AddLog($"Opening {filePath}...");

                process.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }
        }

        public bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                return true;
            }

            return false;
        }

        // ------- Overrides ---------

        protected override void Dispose( bool disposing )
		{
		    if (disposing)
		    {
		        _components?.Dispose();
		    }
		}

        protected override void ExitThreadCore()
        {
            _notifyIcon.Visible = false;
            base.ExitThreadCore();
        }
    }
}
