﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScanWatch
{
    class Scheduler
    {
        private readonly ConcurrentDictionary<Action, ScheduledTask> _scheduledTasks = new ConcurrentDictionary<Action, ScheduledTask>();

        public void Execute(Action action, int timeoutMs)
        {
            var task = new ScheduledTask(action, timeoutMs);
            task.TaskComplete += RemoveTask;
            _scheduledTasks.TryAdd(action, task);
            task.Timer.Start();
        }

        private void RemoveTask(object sender, EventArgs e)
        {
            var task = (ScheduledTask)sender;
            task.TaskComplete -= RemoveTask;
            _scheduledTasks.TryRemove(task.Action, out var deleted);
        }
    }
}
