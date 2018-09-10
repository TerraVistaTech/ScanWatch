using System;
using System.Timers;

namespace ScanWatch
{
    class ScheduledTask
    {
        internal readonly Action Action;
        internal Timer Timer;
        internal EventHandler TaskComplete;

        public ScheduledTask(Action action, int timeoutMs)
        {
            Action = action;
            Timer = new Timer { Interval = timeoutMs };
            Timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Timer.Stop();
            Timer.Elapsed -= TimerElapsed;
            Timer.Dispose();
            Timer = null;

            Action();
            TaskComplete(this, EventArgs.Empty);
        }
    }
}
