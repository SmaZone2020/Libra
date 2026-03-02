using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace Libra.Agent.Helper
{
    internal class VirtualCheck
    {
        public static bool IsVirtualMachine()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"HARDWARE\DESCRIPTION\System\BIOS");

                if (key == null)
                    return false;

                string? biosVendor = key.GetValue("BIOSVendor")?.ToString()?.ToLower();
                string? systemManufacturer = key.GetValue("SystemManufacturer")?.ToString()?.ToLower();
                string? systemProductName = key.GetValue("SystemProductName")?.ToString()?.ToLower();

                string combined = $"{biosVendor} {systemManufacturer} {systemProductName}";

                string[] vmIndicators =
                {
                "vmware",
                "virtualbox",
                "vbox",
                "xen",
                "kvm",
                "qemu",
                "hyper-v",
                "virtual"
            };

                foreach (var indicator in vmIndicators)
                {
                    if (combined.Contains(indicator))
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CMONITORS = 80;

        public static bool HasPhysicalDisplay()
        {
            int monitorCount = GetSystemMetrics(SM_CMONITORS);
            return monitorCount > 0;
        }

        public static bool IsTestSigningEnabled()
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\CI\Config");

            if (key == null) return false;

            var value = key.GetValue("TestSigning");
            return value != null && (int)value == 1;
        }
    }
}
