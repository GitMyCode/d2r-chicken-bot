using System;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace D.WpfApp {
    public static class MachineStamp {
        public static string Version {
            get {
                var assembly = Assembly.GetAssembly(typeof(MainWindow));

                var gitVersionInformationType = assembly.GetType("GitVersionInformation");
                // GitVersionInformation
                //"MajorMinorPatch";
                var versionField = gitVersionInformationType.GetField("MajorMinorPatch");
                if (versionField != null) {
                    return versionField.GetValue(null).ToString();
                }

                var versionProperty = gitVersionInformationType.GetProperty("MajorMinorPatch");
                if (versionProperty != null) return versionProperty.GetGetMethod(true).Invoke(null, null).ToString();

                return "";
            }
        }

        public static string CreateMachineId() {
            var sha1 = SHA256.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes($"{GetProcessorId()}{FecthMACAddressInternal()}"));
            var base64Hash = Convert.ToBase64String(hash);
            return base64Hash[^9..];
        }

        public static string GetProcessorId() {
            var cpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var cpuObj in cpu.Get())
                return cpuObj["ProcessorId"]?.ToString() ?? "";
            return string.Empty;
        }

        public static string FecthMACAddressInternal() {
            try {
                var macAddress = string.Empty;
                var networkAdapterObjs = new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances();
                foreach (var networkAdapterObj in networkAdapterObjs)
                    if (networkAdapterObj["MacAddress"] != null &&
                        bool.TryParse(networkAdapterObj["IPEnabled"]?.ToString(), out var isIpEnabled) && isIpEnabled)
                        return networkAdapterObj["MacAddress"]?.ToString() ?? string.Empty;
                return "";
            } catch (Exception) {
                return "";
            }
        }
    }
}