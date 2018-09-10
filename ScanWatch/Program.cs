using System;
using System.Windows.Forms;
using MutexManager;
using ScanWatch;

namespace HostSwitcher
{
    /// <summary>
    /// Framework for restricting app to a single instance and for running as a tray app.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!SingleInstance.Start())
            {
                SingleInstance.ShowFirstInstance();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var applicationContext = new NotificationIconContext();
                Application.Run(applicationContext);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program Terminated Unexpectedly",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SingleInstance.Stop();
        }
    }
}
