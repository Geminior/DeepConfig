namespace DeepConfigEditor.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Principal;

    public sealed class ProcessService : IProcessService
    {
        public bool IsRunningAsAdministrator
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();

                if (identity != null)
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                return false;
            }
        }

        public bool RunAsAdministrator(string command)
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                var startInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, command)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                try
                {
                    var process = Process.Start(startInfo);
                    process.WaitForExit();

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("Failed to execute administrator command: ", ex);
                }
            }

            return false;
        }
    }
}
