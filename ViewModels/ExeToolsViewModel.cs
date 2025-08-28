using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WinVault.ViewModels
{
    public class ToolItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string IconSource { get; set; } = "\uE7F0"; // 默认图标
        public string ToolType { get; set; }
        public bool IsWebLink { get; set; }
        public bool IsPowerShell { get; set; }
        public bool IsExecutable { get; set; }
        public string CategoryKey { get; set; }
        public BitmapImage? IconBitmap { get; set; }
        public bool UseCustomIcon { get; set; } = false;

        public ToolItem(string name, string description, string filePath, string toolType = "应用程序", string categoryKey = "")
        {
            Name = name;
            Description = description;
            FilePath = filePath;
            ToolType = toolType;
            CategoryKey = categoryKey;
            IsWebLink = filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase);
            IsPowerShell = Path.GetExtension(filePath)?.Equals(".ps1", StringComparison.OrdinalIgnoreCase) ?? false;
            IsExecutable = Path.GetExtension(filePath)?.Equals(".exe", StringComparison.OrdinalIgnoreCase) ?? false;
            
            SetIconSource();
        }

        private void SetIconSource()
        {
            if (IsWebLink)
            {
                // 网页链接使用favicon
                try
                {
                    var uri = new Uri(FilePath);
                    IconSource = $"https://www.google.com/s2/favicons?domain={uri.Host}&sz=64";
                    UseCustomIcon = true;
                }
                catch
                {
                    IconSource = "\uE774"; // 地球图标作为备用
                }
            }
            else if (IsExecutable)
            {
                // 可执行文件尝试提取图标
                ExtractExecutableIcon();
            }
            else if (IsPowerShell)
            {
                // PowerShell脚本根据功能匹配图标
                IconSource = GetPowerShellIcon();
            }
            else
            {
                // 其他文件类型
                IconSource = GetFileTypeIcon();
            }
        }

        private void ExtractExecutableIcon()
        {
            // 优先尝试根据文件名获取在线图标资源
            var fileName = Path.GetFileNameWithoutExtension(FilePath).ToLower();
            
            // 常见软件的在线图标
            var onlineIcon = GetOnlineExecutableIcon(fileName);
            if (!string.IsNullOrEmpty(onlineIcon))
            {
                IconSource = onlineIcon;
                UseCustomIcon = true;
                return;
            }
            
            // 如果没有在线图标，使用FontIcon作为备用
            IconSource = GetExecutableIcon();
        }
        
        private string? GetOnlineExecutableIcon(string fileName)
        {
            return fileName switch
            {
                // 系统工具
                "aida64" => "https://www.aida64.com/favicon.ico",
                "hwinfo" or "hwinfo64" => "https://www.hwinfo.com/favicon.ico",
                "cpu-z" or "cpuz" => "https://www.cpuid.com/favicon.ico",
                "gpu-z" or "gpuz" => "https://www.techpowerup.com/favicon.ico",
                "crystaldiskinfo" => "https://crystalmark.info/favicon.ico",
                "crystaldiskmark" => "https://crystalmark.info/favicon.ico",
                "coretemp" => "https://www.alcpu.com/favicon.ico",
                "prime95" => "https://www.mersenne.org/favicon.ico",
                "memreduct" => "https://www.henrypp.org/favicon.ico",
                "diskgenius" => "https://www.diskgenius.com/favicon.ico",
                "everything" => "https://www.voidtools.com/favicon.ico",
                "rufus" => "https://rufus.ie/favicon.ico",
                "ventoy" => "https://www.ventoy.net/favicon.ico",
                
                _ => null
            };
        }

        private string GetPowerShellIcon()
        {
            // 根据PowerShell脚本功能匹配对应软件图标
            var fileName = Path.GetFileNameWithoutExtension(FilePath).ToLower();
            
            // Steam相关
            if (fileName.Contains("steam"))
            {
                IconSource = "https://store.steampowered.com/favicon.ico";
                UseCustomIcon = true;
                return IconSource;
            }
            
            // Epic Games相关
            if (fileName.Contains("epic"))
            {
                IconSource = "https://www.epicgames.com/favicon.ico";
                UseCustomIcon = true;
                return IconSource;
            }
            
            // EA相关
            if (fileName.Contains("ea"))
            {
                IconSource = "https://www.ea.com/favicon.ico";
                UseCustomIcon = true;
                return IconSource;
            }
            
            // Uplay/Ubisoft相关
            if (fileName.Contains("uplay") || fileName.Contains("ubisoft"))
            {
                IconSource = "https://www.ubisoft.com/favicon.ico";
                UseCustomIcon = true;
                return IconSource;
            }
            
            // 游戏加加相关
            if (fileName.Contains("gamepp") || fileName.Contains("游戏加加"))
            {
                // 使用游戏加加官网图标
                IconSource = "https://gamepp.com/favicon.ico";
                UseCustomIcon = true;
                return IconSource;
            }
            
            // Watt Toolkit相关
            if (fileName.Contains("watt") || fileName.Contains("steampp"))
            {
                IconSource = "https://steampp.net/favicon.ico";
                UseCustomIcon = true;
                return IconSource;
            }
            
            // 默认PowerShell图标
            return "\uE756";
        }

        private string GetExecutableIcon()
        {
            // 根据文件名或目录名判断工具类型的备用图标
            var fileName = Path.GetFileNameWithoutExtension(FilePath).ToLower();
            var directoryName = Path.GetDirectoryName(FilePath)?.Split('\\').LastOrDefault()?.ToLower() ?? "";
            
            return (fileName, directoryName) switch
            {
                // CPU工具
                (var f, _) when f.Contains("cpu") => "\uE950",
                ("cpuz", _) => "\uE950",
                ("cpu-z", _) => "\uE950",
                ("coretemp", _) => "\uE950",
                ("prime95", _) => "\uE950",
                ("linx", _) => "\uE950",
                (var f, _) when f.Contains("burntest") => "\uE950",
                
                // GPU工具
                (var f, _) when f.Contains("gpu") => "\uE7F4",
                ("gpuz", _) => "\uE7F4",
                ("gpu-z", _) => "\uE7F4",
                ("gputest", _) => "\uE7F4",
                (var f, _) when f.Contains("nvidia") => "\uE7F4",
                (var f, _) when f.Contains("dxva") => "\uE7F4",
                
                // 内存工具
                ("memreduct", _) => "\uE8C6",
                (var f, _) when f.Contains("mem") => "\uE8C6",
                ("thaiphoon", _) => "\uE8C6",
                ("tm5", _) => "\uE8C6",
                
                // 磁盘工具
                (var f, _) when f.Contains("crystal") => "\uE8A7",
                (var f, _) when f.Contains("disk") => "\uE8A7",
                ("diskgenius", _) => "\uE8A7",
                ("defraggler", _) => "\uE8A7",
                (var f, _) when f.Contains("ssd") => "\uE8A7",
                
                // 外设工具
                (var f, _) when f.Contains("mouse") => "\uE1E3",
                (var f, _) when f.Contains("keyboard") => "\uE92E",
                ("keytweak", _) => "\uE92E",
                
                // 屏幕工具
                (var f, _) when f.Contains("monitor") => "\uE7F3",
                (var f, _) when f.Contains("screen") => "\uE7F3",
                
                // 系统工具
                ("aida64", _) => "\uE9A2",
                ("hwinfo", _) => "\uE9A2",
                (var f, _) when f.Contains("dism") => "\uE8F1",
                ("everything", _) => "\uE721",
                ("rufus", _) => "\uE88E",
                ("ventoy", _) => "\uE88E",
                ("trafficmonitor", _) => "\uE968",
                
                _ => "\uE756" // 默认应用图标
            };
        }
        
        private string GetFileTypeIcon()
        {
            var extension = Path.GetExtension(FilePath)?.ToLower();
            return extension switch
            {
                ".bat" => "\uE756",
                ".cmd" => "\uE756",
                ".ps1" => "\uE756",
                ".msi" => "\uE7B8",
                _ => "\uE7C3"
            };
        }
    }

    public class ToolCategory
    {
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string CategoryKey { get; set; }
        public ObservableCollection<ToolItem> Tools { get; set; } = new ObservableCollection<ToolItem>();
        public int ToolCount => Tools.Count;
        public bool HasTools => Tools.Count > 0;

        public ToolCategory(string name, string icon = "\uE7F0", string key = "")
        {
            CategoryName = name;
            CategoryIcon = icon;
            CategoryKey = key;
        }
    }

    public class ExeToolsViewModel
    {
        public ObservableCollection<ToolCategory> ToolCategories { get; } = new ObservableCollection<ToolCategory>();

        // 分类配置
        private readonly Dictionary<string, (string DisplayName, string Icon)> CategoryConfigs = new Dictionary<string, (string, string)>
        {
            { "CPU_Tools", ("处理器工具", "\uE950") },
            { "GPU_Tools", ("显卡工具", "\uE7F4") },
            { "Memory_Tools", ("内存工具", "\uE8C6") },
            { "Disk_Tools", ("磁盘工具", "\uE8A7") },
            { "Screen_Tools", ("屏幕工具", "\uE7F3") },
            { "Peripheral_Tools", ("外设工具", "\uE1E3") },
            { "Game_Tools", ("游戏工具", "\uE7FC") },
            { "Other_Tools", ("其他工具", "\uE8F1") },
            { "Motherboard_Tools", ("主板工具", "\uE9A2") }
        };

        // 工具描述映射
        private readonly Dictionary<string, string> ToolDescriptions = new Dictionary<string, string>
        {
            // CPU工具
            { "cpuz", "CPU信息检测工具，显示处理器详细规格和实时参数" },
            { "cpu-z", "CPU信息检测工具，显示处理器详细规格和实时参数" },
            { "coretemp", "轻量级CPU温度监控工具，实时显示核心温度" },
            { "intelBurnTest", "Intel处理器压力测试工具，用于稳定性验证" },
            { "linx", "基于Linpack的CPU压力测试和性能基准工具" },
            { "prime95", "著名的CPU压力测试工具，数学运算稳定性测试" },
            
            // GPU工具
            { "gpuz", "显卡信息检测工具，显示GPU详细规格和传感器数据" },
            { "gpu-z", "显卡信息检测工具，显示GPU详细规格和传感器数据" },
            { "gputest", "显卡性能测试和稳定性压力测试工具" },
            { "nvidiainspector", "NVIDIA显卡超频和监控工具" },
            { "nvidiaprofileinspector", "NVIDIA显卡驱动配置文件编辑器" },
            { "dxvachecker", "DirectX视频加速支持检测工具" },
            
            // 内存工具
            { "memreduct", "内存清理和优化工具，释放未使用的RAM" },
            { "thaiphoon", "内存SPD信息读取工具，显示内存条详细参数" },
            { "memtest", "内存稳定性测试工具，检测RAM错误" },
            { "memtest64", "64位内存测试工具，支持大容量内存检测" },
            { "memtestpro", "专业版内存测试工具，全面内存诊断" },
            { "tm5", "现代化内存稳定性测试工具" },
            { "魔方内存盘", "创建高速虚拟内存盘，加速文件读写" },
            
            // 磁盘工具
            { "crystaldiskinfo", "硬盘健康监控工具，S.M.A.R.T.信息检测" },
            { "crystaldiskmark", "磁盘性能基准测试，测量读写速度" },
            { "diskgenius", "专业磁盘管理工具，分区和数据恢复" },
            { "defraggler", "磁盘碎片整理工具，优化文件系统" },
            { "asssdbenchmark", "SSD性能基准测试工具" },
            { "flashmaster", "U盘真伪检测和性能测试工具" },
            { "h2testw", "存储设备完整性测试，检测虚假容量" },
            { "hdtune", "硬盘诊断和性能测试套件" },
            { "mydisktest", "U盘和存储卡检测工具" },
            { "ssdz", "SSD优化和管理工具" },
            { "spacesniffer", "磁盘空间可视化分析工具" },
            { "urwtest", "USB存储设备读写速度测试" },
            { "wiztree", "超快速磁盘空间分析工具" },
            { "windirstat", "磁盘使用情况统计和可视化" },
            { "finaldata", "专业数据恢复软件" },
            { "魔方数据恢复", "数据恢复工具，找回丢失文件" },
            
            // 屏幕工具
            { "monitorinfo", "显示器信息检测工具" },
            { "ufo帧率测试", "TestUFO在线帧率和运动模糊测试" },
            { "屏幕色块检测", "在线显示器坏点和色彩检测工具" },
            
            // 外设工具
            { "aresonmousetest", "鼠标性能测试工具" },
            { "keytweak", "键盘按键重映射工具" },
            { "keyboard test utility", "键盘按键检测工具" },
            { "mouserate", "鼠标回报率测试工具" },
            { "mousetester", "鼠标点击测试工具" },
            
            // 游戏工具
            { "installsteam", "Steam游戏平台安装脚本" },
            { "installepic", "Epic Games商店安装脚本" },
            { "installea", "EA游戏平台安装脚本" },
            { "installuplay", "Ubisoft Connect安装脚本" },
            { "installgamepp", "游戏加加性能监控工具安装脚本" },
            { "installwatt_toolkit", "Watt Toolkit网络加速工具安装脚本" },
            
            // 其他工具
            { "aida64", "系统信息和硬件检测专业工具" },
            { "hwinfo", "硬件信息监控和传感器数据工具" },
            { "everything", "极速文件搜索工具" },
            { "dism++", "Windows系统精简和优化工具" },
            { "geek_uninstaller", "彻底软件卸载工具" },
            { "processexplorer", "高级进程管理和监控工具" },
            { "rufus", "USB启动盘制作工具" },
            { "ultraiso", "光盘映像文件处理工具" },
            { "ventoy", "多系统启动U盘制作工具" },
            { "wintogo", "Windows便携系统制作工具" },
            { "bluescreenview", "蓝屏错误分析工具" },
            { "nomeiryoui", "Windows字体设置工具" },
            { "trafficmonitor", "网络流量实时监控工具" },
            { "windbg", "Windows内核调试工具" }
        };

        public void LoadTools(string baseDirectory)
        {
            ToolCategories.Clear();

            try
            {
                // 创建各个分类
                var categories = new Dictionary<string, ToolCategory>();
                foreach (var config in CategoryConfigs)
                {
                    categories[config.Key] = new ToolCategory(config.Value.DisplayName, config.Value.Icon, config.Key);
                }

                // 扫描各个目录
                foreach (var category in categories)
                {
                    var categoryPath = Path.Combine(baseDirectory, category.Key);
                    if (Directory.Exists(categoryPath))
                    {
                        ScanToolsDirectory(categoryPath, category.Value);
                    }
                }

                // 添加特殊工具
                AddSpecialTools(categories, baseDirectory);

                // 只添加有工具的分类
                foreach (var category in categories.Values.Where(c => c.HasTools))
                {
                    // 按名称排序工具
                    var sortedTools = category.Tools.OrderBy(t => t.Name).ToList();
                    category.Tools.Clear();
                    foreach (var tool in sortedTools)
                    {
                        category.Tools.Add(tool);
                    }
                    ToolCategories.Add(category);
                }

                // 按分类名排序
                var sortedCategories = ToolCategories.OrderBy(c => c.CategoryName).ToList();
                ToolCategories.Clear();
                foreach (var category in sortedCategories)
                {
                    ToolCategories.Add(category);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载工具时出错: {ex.Message}");
            }
        }

        private void ScanToolsDirectory(string directory, ToolCategory category)
        {
            try
            {
                // 扫描EXE文件
                var exeFiles = Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories)
                    .Where(f => !Path.GetFileName(f).ToLower().Contains("uninstall") && 
                               !Path.GetFileName(f).ToLower().Contains("setup") &&
                               !Path.GetFileName(f).ToLower().Contains("installer"));

                foreach (var file in exeFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                    var displayName = GetDisplayName(fileName);
                    var description = GetToolDescription(fileName);

                    category.Tools.Add(new ToolItem(displayName, description, file, "可执行程序", category.CategoryKey));
                }

                // 扫描其他可执行文件类型
                var scriptFiles = Directory.GetFiles(directory, "*.bat", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(directory, "*.cmd", SearchOption.AllDirectories))
                    .Concat(Directory.GetFiles(directory, "*.ps1", SearchOption.AllDirectories));

                foreach (var file in scriptFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                    var displayName = GetDisplayName(fileName);
                    var extension = Path.GetExtension(file).ToLower();
                    var toolType = extension switch
                    {
                        ".bat" => "批处理脚本",
                        ".cmd" => "命令脚本", 
                        ".ps1" => "PowerShell脚本",
                        _ => "脚本文件"
                    };

                    var description = GetToolDescription(fileName);
                    category.Tools.Add(new ToolItem(displayName, description, file, toolType, category.CategoryKey));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描目录 {directory} 时出错: {ex.Message}");
            }
        }

        private string GetDisplayName(string fileName)
        {
            // 将文件名转换为友好的显示名称
            return fileName switch
            {
                "cpuz" or "cpu-z" => "CPU-Z",
                "gpuz" or "gpu-z" => "GPU-Z", 
                "coretemp" => "Core Temp",
                "prime95" => "Prime95",
                "linx" => "LinX",
                "intelBurnTest" => "Intel Burn Test",
                "memreduct" => "Mem Reduct",
                "thaiphoon" => "Thaiphoon Burner",
                "crystaldiskinfo" => "CrystalDiskInfo",
                "crystaldiskmark" => "CrystalDiskMark",
                "diskgenius" => "DiskGenius",
                "dism++" => "Dism++",
                "aida64" => "AIDA64",
                "hwinfo" => "HWiNFO",
                "installsteam" => "Steam安装器",
                "installepic" => "Epic Games安装器",
                "installea" => "EA Desktop安装器",
                "installuplay" => "Ubisoft Connect安装器",
                "installgamepp" => "游戏加加安装器",
                "installwatt_toolkit" => "Watt Toolkit安装器",
                "monitorinfo" => "Monitor Info",
                "aresonmousetest" => "Areson Mouse Test",
                "keytweak" => "KeyTweak",
                "mouserate" => "Mouse Rate",
                "mousetester" => "Mouse Tester",
                "keyboard test utility" => "Keyboard Test Utility",
                var name => name.ToUpper().Contains("UFO") ? "UFO Test" : 
                           char.ToUpper(name[0]) + name[1..]
            };
        }

        private string GetToolDescription(string fileName)
        {
            return ToolDescriptions.TryGetValue(fileName, out var description) 
                ? description 
                : $"{GetDisplayName(fileName)} - 系统工具";
        }

        private void AddSpecialTools(Dictionary<string, ToolCategory> categories, string baseDirectory)
        {
            // 添加主板工具（引用其他工具）
            var motherboardCategory = categories["Motherboard_Tools"];

            // 从 Other_Tools 中添加 AIDA64
            var aida64Paths = new[] {
                Path.Combine(baseDirectory, "Other_Tools", "AIDA64", "aida64.exe"),
                Path.Combine(baseDirectory, "Other_Tools", "AIDA64", "aida64_x64.exe")
            };
            foreach (var path in aida64Paths.Where(File.Exists).Take(1))
            {
                motherboardCategory.Tools.Add(new ToolItem(
                    "AIDA64", 
                    ToolDescriptions["aida64"], 
                    path,
                    "系统信息工具",
                    "Motherboard_Tools"
                ));
            }

            // 从 Other_Tools 中添加 HWiNFO
            var hwinfoPath = Path.Combine(baseDirectory, "Other_Tools", "HWiNFO64.exe");
            if (File.Exists(hwinfoPath))
            {
                motherboardCategory.Tools.Add(new ToolItem(
                    "HWiNFO", 
                    ToolDescriptions["hwinfo"], 
                    hwinfoPath,
                    "硬件监控工具",
                    "Motherboard_Tools"
                ));
            }

            // 添加屏幕工具中的网页工具
            if (categories.TryGetValue("Screen_Tools", out var screenCategory))
            {
                // UFO测试工具
                screenCategory.Tools.Add(new ToolItem(
                    "UFO帧率测试",
                    ToolDescriptions["ufo帧率测试"],
                    "https://www.testufo.com",
                    "在线工具",
                    "Screen_Tools"
                ));

                // 屏幕色块检测工具
                screenCategory.Tools.Add(new ToolItem(
                    "屏幕色块检测",
                    ToolDescriptions["屏幕色块检测"],
                    "https://screen.bmcx.com/#welcome",
                    "在线工具",
                    "Screen_Tools"
                ));
            }
        }
    }
}