using System;
using System.Threading;

namespace MutexManager
{

    /// <summary>
    /// Enforces a single instance.
    /// </summary>
    /// <remarks>
    /// This is where the magic happens.
    /// Start() tries to create a mutex.
    /// If it detects that another instance is already using the mutex, then it returns FALSE.
    /// Otherwise it returns TRUE.
    /// (Notice that a GUID is used for the mutex name, which is a little better than using the application name.)
    /// If another instance is detected, then you can use ShowFirstInstance() to show it
    /// (which will work as long as you override WndProc as shown above).
    /// ShowFirstInstance() broadcasts a message to all windows.
    /// The message is WM_SHOWFIRSTINSTANCE.
    /// (Notice that a GUID is used for WM_SHOWFIRSTINSTANCE.
    /// That allows you to reuse this code in multiple applications without getting
    /// strange results when you run them all at the same time.)
    /// 
    /// From http://www.codeproject.com/KB/cs/SingleInstanceAppMutex.aspx
    /// </remarks>
    public static class SingleInstance
    {
        private static readonly int WM_SHOWFIRSTINSTANCE =
            WinApi.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|{0}", ProgramInfo.AssemblyGuid);
        private static Mutex mutex;

        public static bool Start()
        {
            var mutexName = $"Local\\{ProgramInfo.AssemblyGuid}";

            // if you want your app to be limited to a single instance
            // across ALL SESSIONS (multiple users & terminal services), then use the following line instead:
            // var mutexName = $"Global\\{ProgramInfo.AssemblyGuid}";

            mutex = new Mutex(true, mutexName, out var onlyInstance);
            return onlyInstance;
        }

        public static void ShowFirstInstance()
        {
            WinApi.PostMessage(
                (IntPtr)WinApi.HWND_BROADCAST,
                WM_SHOWFIRSTINSTANCE,
                IntPtr.Zero,
                IntPtr.Zero);
        }

        public static void Stop()
        {
            mutex.ReleaseMutex();
        }
    }
}