using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using WinVault.ViewModels;

namespace WinVault.Services
{
    /// <summary>
    /// 系统状态监控服务
    /// 提供CPU、内存、磁盘、网络等系统资源的实时监控功能
    /// </summary>
    public class SystemStatusService
    {
        private readonly PerformanceCounter? _cpuCounter;
        private readonly PerformanceCounter? _memoryCounter;
        private bool _isInitialized = false;

        /// <summary>
        /// 初始化系统状态服务
        /// </summary>
        public SystemStatusService()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"性能计数器初始化失败: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// 获取CPU使用率
        /// </summary>
        /// <returns>CPU使用率百分比</returns>
        public async Task<float> GetCpuUsageAsync()
        {
            if (!_isInitialized || _cpuCounter == null) return 0f;

            try
            {
                // 第一次调用返回0，需要等待一段时间后再次调用
                _cpuCounter.NextValue();
                await Task.Delay(100);
                return _cpuCounter.NextValue();
            }
            catch
            {
                return 0f;
            }
        }

        /// <summary>
        /// 获取内存使用信息
        /// </summary>
        /// <returns>内存使用信息</returns>
        public async Task<(long totalMemory, long availableMemory, float usagePercentage)> GetMemoryUsageAsync()
        {
            try
            {
                await Task.Run(() => { }); // 异步占位

                // 获取总内存
                var totalMemory = GC.GetTotalMemory(false);
                
                // 使用WMI获取更准确的内存信息
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                using var results = searcher.Get();
                
                long totalPhysicalMemory = 0;
                foreach (ManagementObject result in results)
                {
                    totalPhysicalMemory = Convert.ToInt64(result["TotalPhysicalMemory"]);
                    break;
                }

                // 获取可用内存
                long availableMemory = 0;
                if (_isInitialized && _memoryCounter != null)
                {
                    availableMemory = (long)_memoryCounter.NextValue() * 1024 * 1024; // 转换为字节
                }

                long usedMemory = totalPhysicalMemory - availableMemory;
                float usagePercentage = totalPhysicalMemory > 0 ? (float)usedMemory / totalPhysicalMemory * 100 : 0;

                return (totalPhysicalMemory, availableMemory, usagePercentage);
            }
            catch
            {
                return (0, 0, 0f);
            }
        }

        /// <summary>
        /// 获取磁盘使用信息
        /// </summary>
        /// <returns>磁盘使用信息</returns>
        public async Task<(long totalSpace, long freeSpace, float usagePercentage)> GetDiskUsageAsync()
        {
            try
            {
                await Task.Run(() => { }); // 异步占位

                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .ToArray();

                if (drives.Length == 0)
                    return (0, 0, 0f);

                // 获取系统盘信息
                var systemDrive = drives.FirstOrDefault(d => d.Name.StartsWith("C:")) ?? drives[0];
                
                long totalSpace = systemDrive.TotalSize;
                long freeSpace = systemDrive.TotalFreeSpace;
                long usedSpace = totalSpace - freeSpace;
                float usagePercentage = totalSpace > 0 ? (float)usedSpace / totalSpace * 100 : 0;

                return (totalSpace, freeSpace, usagePercentage);
            }
            catch
            {
                return (0, 0, 0f);
            }
        }

        /// <summary>
        /// 获取网络连接状态
        /// </summary>
        /// <returns>网络状态信息</returns>
        public async Task<(bool isConnected, string connectionType, long bytesReceived, long bytesSent)> GetNetworkStatusAsync()
        {
            try
            {
                await Task.Run(() => { }); // 异步占位

                bool isConnected = NetworkInterface.GetIsNetworkAvailable();
                string connectionType = "未连接";
                long bytesReceived = 0;
                long bytesSent = 0;

                if (isConnected)
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                   ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        .ToArray();

                    if (interfaces.Length > 0)
                    {
                        var activeInterface = interfaces.FirstOrDefault(ni => 
                            ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                            ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) ?? interfaces[0];

                        connectionType = activeInterface.NetworkInterfaceType switch
                        {
                            NetworkInterfaceType.Ethernet => "以太网",
                            NetworkInterfaceType.Wireless80211 => "Wi-Fi",
                            _ => "已连接"
                        };

                        var stats = activeInterface.GetIPv4Statistics();
                        bytesReceived = stats.BytesReceived;
                        bytesSent = stats.BytesSent;
                    }
                }

                return (isConnected, connectionType, bytesReceived, bytesSent);
            }
            catch
            {
                return (false, "未知", 0, 0);
            }
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息</returns>
        public async Task<(string osName, string osVersion, string computerName)> GetSystemInfoAsync()
        {
            try
            {
                await Task.Run(() => { }); // 异步占位

                string osName = Environment.OSVersion.Platform.ToString();
                string osVersion = Environment.OSVersion.VersionString;
                string computerName = Environment.MachineName;

                // 使用WMI获取更详细的系统信息
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
                    using var results = searcher.Get();
                    
                    foreach (ManagementObject result in results)
                    {
                        osName = result["Caption"]?.ToString() ?? osName;
                        osVersion = result["Version"]?.ToString() ?? osVersion;
                        break;
                    }
                }
                catch
                {
                    // 如果WMI失败，使用默认值
                }

                return (osName, osVersion, computerName);
            }
            catch
            {
                return ("Windows", "未知版本", Environment.MachineName);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
        }
    }
}
