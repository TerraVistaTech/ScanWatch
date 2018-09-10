using System;
using System.Reflection;

namespace MutexManager
{

    /// <summary>
    /// Gets information about the application.
    /// </summary>
    /// <remarks>
    /// This class is just for getting information about the application.
    /// Each assembly has a GUID, and that GUID is useful to us in this application,
    /// so the most important thing in this class is the AssemblyGuid property.
    /// GetEntryAssembly() is used instead of GetExecutingAssembly(), so that you
    /// can put this code into a class library and still get the results you expect.
    /// (Otherwise it would return info on the DLL assembly instead of your application.)
    /// 
    /// From http://www.codeproject.com/KB/cs/SingleInstanceAppMutex.aspx
    /// </remarks>
    internal static class ProgramInfo
    {
        internal static string AssemblyGuid
        {
            get
            {
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;
            }
        }

        internal static string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
        }
    }
}