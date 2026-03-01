using Libra.Agent.Models.Module;
using Libra.Virgo.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Xml.XPath;

namespace Libra.Agent.Models
{
    public static class InfoHelper
    {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RTL_OSVERSIONINFOEXW
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlGetVersion(ref RTL_OSVERSIONINFOEXW versionInfo);




        public static async Task<City> GetCity()
        {
            HttpClient client = new HttpClient();
            var result = JsonSerializer.Deserialize<City>(await client.GetStringAsync("https://ipcity.api.etek.top/"), AgentJsonContext.Default.City);
            if (result == null)
                return new City();
            else
                return result;
        }

        public static string[] GetQQList()
        {
            try
            {
                var qqList = new List<string>();
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tencent Files");
                //D //D Console.WriteLine($"{path}");

                var qqDirs = Directory.GetDirectories(path);

                foreach (var dir in qqDirs)
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName.Length >= 5 && long.TryParse(dirName, out _))
                    {
                        qqList.Add(dirName);
                    }
                }

                return qqList.ToArray();
            }
            catch (Exception ex)
            {
                return new string[0];
            }
        }

        public static List<Disk> Drives()
        {
            List<Disk> drives = [];
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    drives.Add(new Disk()
                    {
                        Name = drive.Name.Substring(0,2),
                        DriveFormat = drive.DriveFormat,
                        TotalSize = drive.TotalSize / (1024 * 1024 * 1024),
                        AvailableSizes = drive.AvailableFreeSpace / (1024 * 1024 * 1024),
                        Label = drive.VolumeLabel == "" ? "NONE" : drive.VolumeLabel
                    });
                }
            }
            return drives;
        }

        public static FileModel[] Files(string path)
        {
            List<FileModel> files = new List<FileModel>();

            if (Path.GetPathRoot(path) == path)
            {
                foreach (var dirName in Directory.GetDirectories(path))
                {
                    var dir = new DirectoryInfo(dirName);
                    files.Add(new FileModel()
                    {
                        FileName = dir.Name,
                        ChangeDate = dir.CreationTime.ToString("yy-MM-dd HH:mm"),
                        Size = "",
                        Type = "文件夹",
                    });
                }
                foreach (var fileName in Directory.GetFiles(path))
                {
                    var file = new FileInfo(fileName);
                    files.Add(new FileModel()
                    {
                        FileName = file.Name,
                        ChangeDate = file.LastWriteTime.ToString("yy-MM-dd HH:mm"),
                        Size = ToReadableSize(file.Length),
                        Type = $"{file.Extension}文件",
                    });
                }
            }
            else if (Directory.Exists(path))
            {
                foreach (var dirName in Directory.GetDirectories(path))
                {
                    var dir = new DirectoryInfo(dirName);
                    files.Add(new FileModel()
                    {
                        FileName = dir.Name,
                        ChangeDate = dir.CreationTime.ToString("yy-MM-dd HH:mm"),
                        Size = "",
                        Type = "文件夹",
                    });
                }
                foreach (var fileName in Directory.GetFiles(path))
                {
                    var file = new FileInfo(fileName);
                    files.Add(new FileModel()
                    {
                        FileName = file.Name,
                        ChangeDate = file.LastWriteTime.ToString("yy-MM-dd HH:mm"),
                        Size = ToReadableSize(file.Length),
                        Type = $"{file.Extension}文件",
                    });
                }
            }
            return files.ToArray();
        }

        public static string GetSystemVersion()
        {
            var versionInfo = new RTL_OSVERSIONINFOEXW();
            versionInfo.dwOSVersionInfoSize = (uint)Marshal.SizeOf(versionInfo);

            if (RtlGetVersion(ref versionInfo) != 0)
                return "Unknown Windows Version";

            string displayVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion", "")?.ToString() ?? "";
            string productName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "")?.ToString() ?? "";
            var splitSpa = productName.Split(' ');
            if (splitSpa.Length == 3)
            {
                productName = $"{GetWindowsVersionName()} {splitSpa[2]}";
            }
            return $"{productName} {displayVersion}";
        }

        public static string GetWindowsVersionName()
        {
            var versionInfo = new RTL_OSVERSIONINFOEXW();
            versionInfo.dwOSVersionInfoSize = (uint)Marshal.SizeOf(versionInfo);
            RtlGetVersion(ref versionInfo);

            if (versionInfo.dwMajorVersion == 10)
            {
                if (versionInfo.dwBuildNumber >= 22000)
                    return "Windows 11";
                else
                    return "Windows 10";
            }
            else if (versionInfo.dwMajorVersion == 6)
            {
                if (versionInfo.dwMinorVersion == 3)
                    return "Windows 8.1";
                if (versionInfo.dwMinorVersion == 2)
                    return "Windows 8";
                if (versionInfo.dwMinorVersion == 1)
                    return "Windows 7";
            }

            return "Unknown Windows";
        }

        public static CpuInfo GetCpuInfo()
        {
            try
            {
                var cpuInfo = new CpuInfo
                {
                    Name = GetCpuName(),
                    Cores = Environment.ProcessorCount
                };

                return cpuInfo;
            }
            catch
            {
                return new CpuInfo
                {
                    Name = "Unknown",
                    Cores = Environment.ProcessorCount
                };
            }
        }

        private static string GetCpuName()
        {
            try
            {
                // 使用PowerShell获取CPU名称
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "(Get-WmiObject -Class Win32_Processor).Name",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd().Trim();
                        return string.IsNullOrEmpty(result) ? "Unknown" : result;
                    }
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        private static double? GetCpuFrequency()
        {
            try
            {
                // 使用PowerShell获取CPU频率
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "(Get-WmiObject -Class Win32_Processor).MaxClockSpeed",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd().Trim();
                        if (double.TryParse(result, out double frequency))
                        {
                            return frequency;
                        }
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static string GetArchitectureName()
        {
            try
            {
                // 使用Environment.Is64BitOperatingSystem判断架构
                if (Environment.Is64BitOperatingSystem)
                {
                    return "x64";
                }
                else
                {
                    return "x86";
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        public static float GetPhysicalMemory()
        {
            try
            {
                // 使用PowerShell获取物理内存
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "(Get-WmiObject -Class Win32_ComputerSystem).TotalPhysicalMemory",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd().Trim();
                        if (long.TryParse(result, out long totalMemory))
                        {
                            return totalMemory / 1024f / 1024f / 1024f; // 转换为GB
                        }
                        return 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        public static string[] GetGpuNames()
        {
            var gpuList = new List<string>();

            try
            {
                // 使用PowerShell获取GPU信息
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "(Get-WmiObject -Class Win32_VideoController).Name",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd().Trim();
                        if (!string.IsNullOrEmpty(result))
                        {
                            // PowerShell可能返回多行，每行一个GPU
                            string[] gpus = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var gpu in gpus)
                            {
                                if (!string.IsNullOrEmpty(gpu))
                                {
                                    gpuList.Add(gpu);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return Array.Empty<string>();
            }

            return gpuList.ToArray();
        }

        public static DateTime? GetBootTime()
        {
            try{
                long uptimeMs = Environment.TickCount64;
                return DateTime.Now.AddMilliseconds(-uptimeMs);
            }
            catch
            {
                return null;
            }
        }

        public static bool? GetUacStatus()
        {
            try
            {
                var uacValue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 1);
                return Convert.ToInt32(uacValue) == 1;
            }
            catch
            {
                return null;
            }
        }

        public static (bool IsVirtualMachine, string VmType) GetVirtualMachineInfo()
        {
            try
            {
                var vmTypes = new Dictionary<string, string>
                {
                    { "VMware", "VMware" },
                    { "VirtualBox", "VirtualBox" },
                    { "Hyper-V", "Hyper-V" },
                    { "QEMU", "QEMU" },
                    { "Xen", "Xen" }
                };

                // 使用PowerShell获取BIOS信息
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "Get-WmiObject -Class Win32_BIOS | Select-Object Manufacturer, Version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd().Trim();
                        if (!string.IsNullOrEmpty(result))
                        {
                            foreach (var kvp in vmTypes)
                            {
                                if (result.Contains(kvp.Key))
                                {
                                    return (true, kvp.Value);
                                }
                            }
                        }
                    }
                }

                // 使用PowerShell获取系统信息
                psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "Get-WmiObject -Class Win32_ComputerSystem | Select-Object Model",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd().Trim();
                        if (!string.IsNullOrEmpty(result))
                        {
                            foreach (var kvp in vmTypes)
                            {
                                if (result.Contains(kvp.Key))
                                {
                                    return (true, kvp.Value);
                                }
                            }
                        }
                    }
                }

                return (false, "");
            }
            catch
            {
                return (false, "");
            }
        }

        public static string ToReadableSize(long sizeInBytes)
        {
            if (sizeInBytes <= 0) return "0 KB";

            var units = new[] { "KB", "MB", "GB" };
            double size = sizeInBytes / 1024.0;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:F2} {units[unitIndex]}";
        }

        public static int ToInt(this ProcessPriorityClass priority)
        {
            switch (priority)
            {
                case ProcessPriorityClass.Idle:
                case ProcessPriorityClass.BelowNormal:
                    return 0; // 低

                case ProcessPriorityClass.Normal:
                    return 1; // 中

                case ProcessPriorityClass.AboveNormal:
                case ProcessPriorityClass.High:
                case ProcessPriorityClass.RealTime:
                    return 2; // 高

                default:
                    return 1; // 中
            }
        }

        public static class MouseTracker
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;
                public POINT(int x, int y) { X = x; Y = y; }

                // ✅ 增加容差比较：允许1-2像素的微小抖动
                public bool IsSame(POINT other, int tolerance = 2) =>
                    Math.Abs(X - other.X) <= tolerance && Math.Abs(Y - other.Y) <= tolerance;
            }

            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out POINT lpPoint);

            private static POINT _lastMousePosition;
            private static DateTime _lastActiveTime;
            private static DateTime? _idleStartTime;
            private static TimeSpan _totalIdleTime;
            private static bool _isIdle;
            private static Timer _checkTimer;

            // ✅ 核心改进：用计数器代替时间判断
            private static int _unchangedCount;          // 连续未变化次数
            private static readonly int _idleThreshold = 4;  // 连续4次不变 = 挂机（2s×4=8秒）
            private static readonly int _checkIntervalMs = 2000; // 检测间隔2秒

            private static readonly object _lock = new object();

            static MouseTracker()
            {
                GetCursorPos(out _lastMousePosition);
                _lastActiveTime = DateTime.Now;
                _isIdle = false;
                _unchangedCount = 0;

                _checkTimer = new Timer(CheckMousePosition, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_checkIntervalMs));
            }

            private static void CheckMousePosition(object state)
            {
                lock (_lock)
                {
                    POINT currentPosition;
                    GetCursorPos(out currentPosition);

                    if (currentPosition.IsSame(_lastMousePosition))
                    {
                        _unchangedCount++;
                        if (_unchangedCount >= _idleThreshold && !_isIdle)
                        {
                            _isIdle = true;
                            _idleStartTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        _lastMousePosition = currentPosition;
                        _lastActiveTime = DateTime.Now;
                        _unchangedCount = 0;

                        if (_isIdle && _idleStartTime.HasValue)
                        {
                            _totalIdleTime += DateTime.Now - _idleStartTime.Value;
                            _idleStartTime = null;
                            _isIdle = false;
                        }
                    }
                }
            }

            public static DateTime GetLastActiveTime()
            {
                lock (_lock)
                {
                    CheckMousePosition(null);
                    return _lastActiveTime;
                }
            }

            public static bool IsIdle()
            {
                lock (_lock)
                {
                    CheckMousePosition(null);
                    return _isIdle;
                }
            }

            public static TimeSpan GetTotalIdleTime()
            {
                lock (_lock)
                {
                    if (_isIdle && _idleStartTime.HasValue)
                    {
                        return _totalIdleTime + (DateTime.Now - _idleStartTime.Value);
                    }
                    return _totalIdleTime;
                }
            }
        }
    }
}
