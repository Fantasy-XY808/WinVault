using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Net.NetworkInformation;
using System.Management;
using Windows.System.Power;
using Microsoft.UI.Xaml.Media;

namespace WinVault.Pages
{
    public sealed partial class HardwarePage : Page
    {
        // 硬件信息集合
        private ObservableCollection<HardwareInfo> HardwareItems { get; } = new();
        
        public HardwarePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            
            // 在UI初始化后加载硬件信息
            Loaded += HardwarePage_Loaded;
            
            // 监听分类选择变化
            HardwareCategoriesListView.SelectionChanged += HardwareCategoriesListView_SelectionChanged;
        }

        private void HardwarePage_Loaded(object sender, RoutedEventArgs e)
        {
            // 默认加载处理器信息
            LoadHardwareInfo("CPU");
        }

        private void HardwareCategoriesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HardwareCategoriesListView.SelectedItem is ListViewItem selectedItem)
            {
                string category = ((TextBlock)((StackPanel)selectedItem.Content).Children[1]).Text;
                HardwareCategoryTitle.Text = $"{category}信息";
                
                string categoryType = category switch
                {
                    "处理器" => "CPU",
                    "内存" => "Memory",
                    "磁盘" => "Disk",
                    "显卡" => "GPU",
                    "网络适配器" => "Network",
                    "电源信息" => "Power",
                    _ => "CPU"
                };
                
                LoadHardwareInfo(categoryType);
            }
        }
        
        private async void LoadHardwareInfo(string category)
        {
            // 清空当前列表并显示加载指示器
            HardwareItems.Clear();
            HardwareInfoListView.ItemsSource = null;
            
            try
            {
                                switch (category)
                {
                    case "CPU":
                        await LoadCpuInfoAsync();
                        break;
                    case "Memory":
                        await LoadMemoryInfoAsync();
                        break;
                    case "Disk":
                        await LoadDiskInfoAsync();
                        break;
                    case "GPU":
                        await LoadGpuInfoAsync();
                        break;
                    case "Network":
                        await LoadNetworkInfoAsync();
                        break;
                    case "Power":
                        await LoadPowerInfoAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                var error = new HardwareInfo { Name = "加载硬件信息失败" };
                error.Properties.Add(new KeyValuePair<string, string>("错误", ex.Message));
                HardwareItems.Add(error);
            }
            
            // 绑定数据
            HardwareInfoListView.ItemsSource = HardwareItems;
        }
        
        // === 真实数据实现 ===
        private async Task LoadCpuInfoAsync()
        {
            var items = new List<HardwareInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, L3CacheSize FROM Win32_Processor");
                    using var results = searcher.Get();
                    foreach (ManagementObject mo in results)
                    {
                        var cpu = new HardwareInfo { Name = mo["Name"]?.ToString() ?? "处理器" };
                        var cores = mo["NumberOfCores"]?.ToString();
                        var logical = mo["NumberOfLogicalProcessors"]?.ToString();
                        var maxClock = mo["MaxClockSpeed"] != null ? $"{mo["MaxClockSpeed"]} MHz" : null;
                        var l3 = mo["L3CacheSize"] != null ? $"{mo["L3CacheSize"]} KB" : null;

                        if (!string.IsNullOrWhiteSpace(cores)) cpu.Properties.Add(new KeyValuePair<string, string>("核心数", cores!));
                        if (!string.IsNullOrWhiteSpace(logical)) cpu.Properties.Add(new KeyValuePair<string, string>("线程数", logical!));
                        if (!string.IsNullOrWhiteSpace(maxClock)) cpu.Properties.Add(new KeyValuePair<string, string>("最大频率", maxClock!));
                        if (!string.IsNullOrWhiteSpace(l3)) cpu.Properties.Add(new KeyValuePair<string, string>("三级缓存", l3!));

                        items.Add(cpu);
                    }
                }
                catch (Exception ex)
                {
                    var cpu = new HardwareInfo { Name = "处理器" };
                    cpu.Properties.Add(new KeyValuePair<string, string>("错误", ex.Message));
                    items.Add(cpu);
                }
            });

            foreach (var it in items) HardwareItems.Add(it);
        }
        
        private async Task LoadMemoryInfoAsync()
        {
            var items = new List<HardwareInfo>();

            await Task.Run(() =>
            {
                try
                {
                    // 总物理内存
                    ulong totalPhysicalMemory = 0;
                    try
                    {
                        using var cs = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                        using var csResults = cs.Get();
                        foreach (ManagementObject mo in csResults)
                        {
                            if (mo["TotalPhysicalMemory"] != null)
                            {
                                totalPhysicalMemory = (ulong)(Convert.ToUInt64(mo["TotalPhysicalMemory"]));
                                break;
                            }
                        }
                    }
                    catch { }

                    // 插槽数
                    int slotCount = 0;
                    try
                    {
                        using var arrSearcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
                        using var arrResults = arrSearcher.Get();
                        foreach (ManagementObject mo in arrResults)
                        {
                            if (mo["MemoryDevices"] != null)
                            {
                                slotCount = Convert.ToInt32(mo["MemoryDevices"]);
                                break;
                            }
                        }
                    }
                    catch { }

                    // 物理内存条详情
                    var modules = new List<(string Bank, string Capacity, string Type, string Speed)>();
                    try
                    {
                        using var memSearcher = new ManagementObjectSearcher("SELECT BankLabel, Capacity, MemoryType, Speed FROM Win32_PhysicalMemory");
                        using var memResults = memSearcher.Get();
                        foreach (ManagementObject mo in memResults)
                        {
                            var capacityStr = mo["Capacity"] != null ? FormatBytes(Convert.ToUInt64(mo["Capacity"])) : "未知";
                            var type = MemoryTypeToString(mo["MemoryType"] as ushort?);
                            var speed = mo["Speed"] != null ? $"{mo["Speed"]} MHz" : "未知";
                            var bank = mo["BankLabel"]?.ToString() ?? "槽位";
                            modules.Add((bank, capacityStr, type, speed));
                        }
                    }
                    catch { }

                    var memory = new HardwareInfo { Name = "系统内存" };
                    if (totalPhysicalMemory > 0) memory.Properties.Add(new KeyValuePair<string, string>("总容量", FormatBytes(totalPhysicalMemory)));
                    if (slotCount > 0) memory.Properties.Add(new KeyValuePair<string, string>("插槽数", slotCount.ToString()));
                    memory.Properties.Add(new KeyValuePair<string, string>("已检测内存条数量", modules.Count.ToString()));
                    
                    items.Add(memory);

                    int idx = 1;
                    foreach (var m in modules)
                    {
                        var mod = new HardwareInfo { Name = $"内存条 #{idx++} ({m.Bank})" };
                        mod.Properties.Add(new KeyValuePair<string, string>("容量", m.Capacity));
                        mod.Properties.Add(new KeyValuePair<string, string>("类型", m.Type));
                        mod.Properties.Add(new KeyValuePair<string, string>("频率", m.Speed));
                        items.Add(mod);
                    }
                }
                                catch (Exception ex)
                {
                    var memory = new HardwareInfo { Name = "系统内存" };
                    memory.Properties.Add(new KeyValuePair<string, string>("错误", ex.Message));
                    items.Add(memory);
                }
            });

            foreach (var it in items) HardwareItems.Add(it);
        }
        
        private async Task LoadDiskInfoAsync()
        {
            var items = new List<HardwareInfo>();

            await Task.Run(() =>
            {
                try
                {
                    // 物理磁盘
                    var physicalDisks = new List<(string Model, string Size, string InterfaceType, string MediaType, string DeviceId)>();
                    try
                    {
                        using var diskSearcher = new ManagementObjectSearcher("SELECT Model, Size, InterfaceType, MediaType, DeviceID FROM Win32_DiskDrive");
                        using var diskResults = diskSearcher.Get();
                        foreach (ManagementObject mo in diskResults)
                        {
                            var model = mo["Model"]?.ToString() ?? "物理磁盘";
                            var size = mo["Size"] != null ? FormatBytes(Convert.ToUInt64(mo["Size"])) : "未知";
                            var interfaceType = mo["InterfaceType"]?.ToString() ?? "未知";
                            var mediaType = mo["MediaType"]?.ToString() ?? "未知";
                            var deviceId = mo["DeviceID"]?.ToString() ?? "";
                            physicalDisks.Add((model, size, interfaceType, mediaType, deviceId));
                        }
                    }
                    catch { }

                    // 逻辑卷
                    var logicalVolumes = new List<(string Drive, string VolumeLabel, string FileSystem, string TotalSize, string FreeSpace, string DeviceId)>();
                    try
                    {
                        var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
                        foreach (var d in drives)
                        {
                            var volLabel = !string.IsNullOrWhiteSpace(d.VolumeLabel) ? d.VolumeLabel : "本地磁盘";
                            var fileSystem = d.DriveFormat;
                            var totalSize = FormatBytes((ulong)d.TotalSize);
                            var freeSpace = FormatBytes((ulong)d.AvailableFreeSpace);
                            var deviceId = d.Name.Replace("\\", "");
                            logicalVolumes.Add((d.Name, volLabel, fileSystem, totalSize, freeSpace, deviceId));
                        }
                    }
                    catch { }

                    // 显示物理磁盘信息
                    foreach (var disk in physicalDisks)
                    {
                        var diskInfo = new HardwareInfo { Name = $"物理磁盘: {disk.Model}", IsDisk = true, DeviceId = disk.DeviceId };
                        diskInfo.Properties.Add(new KeyValuePair<string, string>("型号", disk.Model));
                        diskInfo.Properties.Add(new KeyValuePair<string, string>("类型", disk.MediaType));
                        diskInfo.Properties.Add(new KeyValuePair<string, string>("接口", disk.InterfaceType));
                        diskInfo.Properties.Add(new KeyValuePair<string, string>("容量", disk.Size));
                        diskInfo.Properties.Add(new KeyValuePair<string, string>("设备ID", disk.DeviceId));
                        
                        items.Add(diskInfo);
                        
                        // 查找对应的分区
                        var relatedPartitions = logicalVolumes.Where(v => v.DeviceId.StartsWith(disk.DeviceId.Replace("\\", ""))).ToList();
                        if (relatedPartitions.Any())
                        {
                            diskInfo.Properties.Add(new KeyValuePair<string, string>("分区数量", relatedPartitions.Count.ToString()));
                            
                            // 添加分区信息作为子项
                            foreach (var partition in relatedPartitions)
                            {
                                var partitionInfo = new HardwareInfo { Name = $"  └─ {partition.Drive} ({partition.VolumeLabel})", IsPartition = true };
                                partitionInfo.Properties.Add(new KeyValuePair<string, string>("卷标", partition.VolumeLabel));
                                partitionInfo.Properties.Add(new KeyValuePair<string, string>("文件系统", partition.FileSystem));
                                partitionInfo.Properties.Add(new KeyValuePair<string, string>("总容量", partition.TotalSize));
                                partitionInfo.Properties.Add(new KeyValuePair<string, string>("可用空间", partition.FreeSpace));
                                
                                // 计算使用率
                                try
                                {
                                    var total = Convert.ToUInt64(partition.TotalSize.Split(' ')[0]) * 1024 * 1024 * 1024; // 假设是GB
                                    var free = Convert.ToUInt64(partition.FreeSpace.Split(' ')[0]) * 1024 * 1024 * 1024;
                                    var used = total - free;
                                    var usagePercent = (float)used / total * 100;
                                    partitionInfo.Properties.Add(new KeyValuePair<string, string>("使用率", $"{usagePercent:F1}%"));
                                }
                                catch { }
                                
                                diskInfo.Partitions.Add(partitionInfo);
                            }
                        }
                        
                        items.Add(diskInfo);
                    }
                }
                catch (Exception ex)
                {
                    var disk = new HardwareInfo { Name = "磁盘" };
                    disk.Properties.Add(new KeyValuePair<string, string>("错误", ex.Message));
                    items.Add(disk);
                }
            });

            foreach (var it in items) HardwareItems.Add(it);
        }
        
        private async Task LoadGpuInfoAsync()
        {
            var items = new List<HardwareInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using var gpuSearcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, DriverVersion, VideoProcessor FROM Win32_VideoController");
                    using var gpuResults = gpuSearcher.Get();
                    foreach (ManagementObject mo in gpuResults)
                    {
                        var gpu = new HardwareInfo { Name = mo["Name"]?.ToString() ?? "显卡" };
                        if (mo["AdapterRAM"] != null)
                        {
                            try
                            {
                                var vram = Convert.ToUInt64(mo["AdapterRAM"]);
                                gpu.Properties.Add(new KeyValuePair<string, string>("显存", FormatBytes(vram)));
                            }
                            catch { }
                        }
                        if (mo["VideoProcessor"] != null) gpu.Properties.Add(new KeyValuePair<string, string>("图形处理器", mo["VideoProcessor"].ToString()!));
                        if (mo["DriverVersion"] != null) gpu.Properties.Add(new KeyValuePair<string, string>("驱动程序版本", mo["DriverVersion"].ToString()!));
                        items.Add(gpu);
                    }
                }
                catch (Exception ex)
                {
                    var gpu = new HardwareInfo { Name = "显卡" };
                    gpu.Properties.Add(new KeyValuePair<string, string>("错误", ex.Message));
                    items.Add(gpu);
                }
            });

            foreach (var it in items) HardwareItems.Add(it);
        }
        
        private async Task LoadNetworkInfoAsync()
        {
            var items = new List<HardwareInfo>();

            await Task.Run(() =>
            {
                try
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                    foreach (var ni in interfaces)
                    {
                        var hw = new HardwareInfo { Name = ni.Name };
                        hw.Properties.Add(new KeyValuePair<string, string>("类型", NetworkTypeToString(ni.NetworkInterfaceType)));
                        hw.Properties.Add(new KeyValuePair<string, string>("状态", ni.OperationalStatus == OperationalStatus.Up ? "已连接" : "未连接"));
                        
                        var mac = string.Join(":", ni.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));
                        if (!string.IsNullOrWhiteSpace(mac)) hw.Properties.Add(new KeyValuePair<string, string>("MAC地址", mac));

                        try
                        {
                            var ipProps = ni.GetIPProperties();
                            var ipv4 = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address?.ToString();
                            if (!string.IsNullOrWhiteSpace(ipv4)) hw.Properties.Add(new KeyValuePair<string, string>("IP地址", ipv4!));
                            var gw = ipProps.GatewayAddresses.FirstOrDefault()?.Address?.ToString();
                            if (!string.IsNullOrWhiteSpace(gw)) hw.Properties.Add(new KeyValuePair<string, string>("默认网关", gw!));
                        }
                        catch { }

                        items.Add(hw);
                    }
                }
                catch (Exception ex)
                {
                    var net = new HardwareInfo { Name = "网络适配器" };
                    net.Properties.Add(new KeyValuePair<string, string>("错误", ex.Message));
                    items.Add(net);
                }
            });

            foreach (var it in items) HardwareItems.Add(it);
        }
        
        private async Task LoadPowerInfoAsync()
        {
            var items = new List<HardwareInfo>();

            await Task.Run(() =>
            {
                try
                {
                    var power = new HardwareInfo { Name = "电源状态" };

                    try
                    {
                        var supply = PowerManager.PowerSupplyStatus;
                        var battery = PowerManager.BatteryStatus;
                        var saver = PowerManager.EnergySaverStatus;
                        var percent = PowerManager.RemainingChargePercent;

                        power.Properties.Add(new KeyValuePair<string, string>("电源状态", SupplyStatusToString(supply)));
                        power.Properties.Add(new KeyValuePair<string, string>("电池状态", BatteryStatusToString(battery)));
                        if (percent >= 0) power.Properties.Add(new KeyValuePair<string, string>("剩余电量", $"{percent}%"));
                        power.Properties.Add(new KeyValuePair<string, string>("省电模式", saver == EnergySaverStatus.On ? "开启" : saver == EnergySaverStatus.Off ? "关闭" : "未知"));
                    }
                    catch
                    {
                        // Fallback: WMI 查询电池（部分台式机没有电池信息）
                        try
                        {
                            using var batterySearcher = new ManagementObjectSearcher("SELECT BatteryStatus, EstimatedChargeRemaining FROM Win32_Battery");
                            using var batteryResults = batterySearcher.Get();
                            foreach (ManagementObject mo in batteryResults)
                            {
                                if (mo["BatteryStatus"] != null)
                                {
                                    power.Properties.Add(new KeyValuePair<string, string>("电池状态", mo["BatteryStatus"].ToString()!));
                                }
                                if (mo["EstimatedChargeRemaining"] != null)
                                {
                                    power.Properties.Add(new KeyValuePair<string, string>("剩余电量", mo["EstimatedChargeRemaining"] + "%"));
                                }
                                break;
                            }
                        }
                        catch { }
                    }

                    items.Add(power);
                }
                catch (Exception ex)
                {
                    var power = new HardwareInfo { Name = "电源状态" };
                    power.Properties.Add(new KeyValuePair<string, string>("错误", ex.Message));
                    items.Add(power);
                }
            });

            foreach (var it in items) HardwareItems.Add(it);
        }

        private static string FormatBytes(ulong bytes)
        {
            const double KB = 1024.0;
            const double MB = KB * 1024.0;
            const double GB = MB * 1024.0;
            const double TB = GB * 1024.0;
            double b = bytes;
            if (b >= TB) return $"{b / TB:0.##} TB";
            if (b >= GB) return $"{b / GB:0.##} GB";
            if (b >= MB) return $"{b / MB:0.##} MB";
            if (b >= KB) return $"{b / KB:0.##} KB";
            return $"{bytes} B";
        }

        private static string MemoryTypeToString(ushort? type)
        {
            return type switch
            {
                20 => "DDR",
                21 => "DDR2",
                22 => "DDR2 FB-DIMM",
                24 => "DDR3",
                26 => "DDR4",
                27 => "LPDDR",
                28 => "LPDDR2",
                29 => "LPDDR3",
                30 => "LPDDR4",
                34 => "DDR5",
                _ => "未知"
            };
        }

        private static string NetworkTypeToString(NetworkInterfaceType type)
        {
            return type switch
            {
                NetworkInterfaceType.Ethernet => "以太网",
                NetworkInterfaceType.Wireless80211 => "Wi-Fi",
                NetworkInterfaceType.Ppp => "PPP",
                NetworkInterfaceType.Loopback => "环回",
                NetworkInterfaceType.Tunnel => "隧道",
                _ => type.ToString()
            };
        }

        private static string SupplyStatusToString(PowerSupplyStatus status)
        {
            return status switch
            {
                PowerSupplyStatus.Adequate => "接通电源",
                PowerSupplyStatus.NotPresent => "未接通",
                PowerSupplyStatus.Inadequate => "电源不足",
                _ => "未知"
            };
        }

        private static string BatteryStatusToString(BatteryStatus status)
        {
            return status switch
            {
                BatteryStatus.Charging => "正在充电",
                BatteryStatus.Discharging => "放电中",
                BatteryStatus.Idle => "空闲",
                BatteryStatus.NotPresent => "无电池",
                _ => "未知"
            };
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// 展开按钮点击事件
        /// </summary>
        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is HardwareInfo diskInfo)
            {
                // 查找对应的ListViewItem
                var listViewItem = FindParent<ListViewItem>(button);
                if (listViewItem != null)
                {
                    // 查找分区列表
                    var partitionsListView = FindChild<ListView>(listViewItem, "PartitionsListView");
                    if (partitionsListView != null)
                    {
                        // 切换显示状态
                        if (partitionsListView.Visibility == Visibility.Collapsed)
                        {
                            partitionsListView.Visibility = Visibility.Visible;
                            button.Content = "收起";
                        }
                        else
                        {
                            partitionsListView.Visibility = Visibility.Collapsed;
                            button.Content = "展开";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查找父级控件
        /// </summary>
        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            
            if (parent is T result) return result;
            
            return FindParent<T>(parent);
        }

        /// <summary>
        /// 查找子级控件
        /// </summary>
        private T? FindChild<T>(DependencyObject parent, string name = "") where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    if (string.IsNullOrEmpty(name) || (child is FrameworkElement fe && fe.Name == name))
                    {
                        return result;
                    }
                }
                
                var found = FindChild<T>(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
    
    // 硬件信息数据模型
    public class HardwareInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<KeyValuePair<string, string>> Properties { get; } = new();
        public List<HardwareInfo> Partitions { get; } = new();
        public bool IsDisk { get; set; } = false;
        public bool IsPartition { get; set; } = false;
        public string DeviceId { get; set; } = string.Empty;
    }
} 
