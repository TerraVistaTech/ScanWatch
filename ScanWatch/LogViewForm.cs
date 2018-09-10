using System;
using System.Windows.Forms;

namespace ScanWatch
{
    public partial class LogViewForm : Form
    {
        public LogViewForm()
        {
            InitializeComponent();
        }

        delegate void AddLogCallback(string text);

        public void AddLog(string line)
        {
            if (lstLog.InvokeRequired)
            {
                AddLogCallback d = AddLog;
                Invoke(d, line);
            }
            else
            {
                lstLog.Items.Add($"{DateTime.Now}\t{line}");
            }
        }

        private void LogViewFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
