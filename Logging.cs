using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$
{
    internal static class Logging
    {
        public static void InitializeLogger(log4net.Repository.ILoggerRepository logRepository, System.Reflection.Assembly assembly)
        {
            var appName = assembly.GetName().Name;
            var appdata = GetAppDataFolder(appName);

            Directory.CreateDirectory(appdata);

            var path = System.IO.Path.Combine(appdata, "log");
            GlobalContext.Properties["LogFileName"] = path;

            var processName = Process.GetCurrentProcess().ProcessName;
            GlobalContext.Properties["pname"] = processName;
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;

            var resourceName = $"$safeprojectname$.log4net.config"; // sensitive to renames
            var thisMethod = System.Reflection.MethodInfo.GetCurrentMethod();
            var thisAssembly = thisMethod.Module.Assembly;

            using (Stream stream = thisAssembly.GetManifestResourceStream(resourceName))
            {
                XmlConfigurator.Configure(logRepository, stream);
            }
        }

        public static string GetAppDataFolder(string appName)
        {
            var appdata = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return System.IO.Path.Combine(appdata, appName);
        }

    }
}
