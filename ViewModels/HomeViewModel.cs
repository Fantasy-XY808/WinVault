using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using WinVault.Constants;
using WinVault.Infrastructure;
using WinVault.Services;

namespace WinVault.ViewModels
{
    /// <summary>
    /// 首页视图模型
    /// </summary>
    public class HomeViewModel : ViewModelBase
    {
        private readonly AppInfoService _appInfoService;
        private readonly NavigationService _navigationService;
        private readonly SystemStatusService _systemStatusService;
        
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName => _appInfoService.GetAppName();
        
        /// <summary>
        /// 应用版本
        /// </summary>
        public string Version => _appInfoService.GetVersionString();

        /// <summary>
        /// 功能卡片集合
        /// </summary>
        public ObservableCollection<FunctionCard> FunctionCards { get; }

        /// <summary>
        /// 工具卡片集合
        /// </summary>
        public ObservableCollection<FunctionCard> ToolCards { get; }

        /// <summary>
        /// 系统状态卡片集合
        /// </summary>
        public ObservableCollection<SystemStatusCard> SystemStatusCards { get; }

        /// <summary>
        /// 快速操作卡片集合
        /// </summary>
        public ObservableCollection<QuickActionCard> QuickActionCards { get; }

        /// <summary>
        /// 功能卡片导航命令
        /// </summary>
        public ICommand NavigateCommand { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public HomeViewModel()
        {
            _appInfoService = AppInfoService.Instance;
            _navigationService = NavigationService.Instance;
            _systemStatusService = new SystemStatusService();
            
            NavigateCommand = new WinVault.Infrastructure.RelayCommand<string>(NavigateTo);
            
            // 初始化功能卡片
            FunctionCards = new ObservableCollection<FunctionCard>
            {
                new FunctionCard
                {
                    Title = "硬件信息",
                    Description = "查看CPU、内存、硬盘等硬件信息",
                    Icon = "\uE7F4",
                    NavigationKey = AppConstants.NavigationRoutes.Hardware
                },
                new FunctionCard
                {
                    Title = "服务管理",
                    Description = "查看和管理Windows系统服务",
                    Icon = "\uE90F",
                    NavigationKey = AppConstants.NavigationRoutes.Services
                },
                new FunctionCard
                {
                    Title = "指令查询",
                    Description = "查询常用命令和指令",
                    Icon = "\uE756",
                    NavigationKey = AppConstants.NavigationRoutes.CommandQuery
                },
                new FunctionCard
                {
                    Title = "快速设置",
                    Description = "常用设置选项快速调整",
                    Icon = "\uE713",
                    NavigationKey = AppConstants.NavigationRoutes.QuickSettings
                }
            };
            
            // 初始化工具卡片
            ToolCards = new ObservableCollection<FunctionCard>
            {

                new FunctionCard
                {
                    Title = "工具箱",
                    Description = "各种实用小工具集合",
                    Icon = "\uE2AC",
                    NavigationKey = AppConstants.NavigationRoutes.ExeTools
                }
            };

            // 初始化系统状态卡片
            SystemStatusCards = new ObservableCollection<SystemStatusCard>();

            // 异步加载系统状态
            _ = LoadSystemStatusAsync();

            // 初始化快速操作卡片
            QuickActionCards = new ObservableCollection<QuickActionCard>
            {
                new QuickActionCard
                {
                    Title = "系统清理",
                    Description = "清理临时文件和垃圾",
                    Icon = "\uE74D",
                    ActionCommand = new WinVault.Infrastructure.RelayCommand<object>(_ => { /* TODO: 实现系统清理 */ })
                },
                new QuickActionCard
                {
                    Title = "性能优化",
                    Description = "优化系统性能设置",
                    Icon = "\uE7C3",
                    ActionCommand = new WinVault.Infrastructure.RelayCommand<object>(_ => { /* TODO: 实现性能优化 */ })
                },
                new QuickActionCard
                {
                    Title = "安全扫描",
                    Description = "扫描系统安全状态",
                    Icon = "\uE72E",
                    ActionCommand = new WinVault.Infrastructure.RelayCommand<object>(_ => { /* TODO: 实现安全扫描 */ })
                },
                new QuickActionCard
                {
                    Title = "系统信息",
                    Description = "查看详细系统信息",
                    Icon = "\uE946",
                    ActionCommand = new WinVault.Infrastructure.RelayCommand<object>(_ => NavigateTo(AppConstants.NavigationRoutes.Hardware))
                }
            };
        }

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        private void NavigateTo(string? navigationKey)
        {
            if (!string.IsNullOrEmpty(navigationKey))
            {
                _navigationService.NavigateTo(navigationKey);
            }
        }

        /// <summary>
        /// 异步加载系统状态数据
        /// </summary>
        private async Task LoadSystemStatusAsync()
        {
            try
            {
                // 获取CPU使用率
                var cpuUsage = await _systemStatusService.GetCpuUsageAsync();
                SystemStatusCards.Add(new SystemStatusCard
                {
                    Title = "CPU使用率",
                    Value = $"{cpuUsage:F1}%",
                    Icon = "\uE7F4",
                    Status = cpuUsage < 80 ? "正常" : "高负载"
                });

                // 获取内存使用情况
                var (totalMemory, availableMemory, memoryUsage) = await _systemStatusService.GetMemoryUsageAsync();
                var totalGB = totalMemory / (1024.0 * 1024.0 * 1024.0);
                var usedGB = (totalMemory - availableMemory) / (1024.0 * 1024.0 * 1024.0);
                SystemStatusCards.Add(new SystemStatusCard
                {
                    Title = "内存使用",
                    Value = $"{usedGB:F1}GB / {totalGB:F1}GB",
                    Icon = "\uE950",
                    Status = memoryUsage < 80 ? "正常" : "高使用率"
                });

                // 获取磁盘使用情况
                var (totalSpace, freeSpace, diskUsage) = await _systemStatusService.GetDiskUsageAsync();
                var totalSpaceGB = totalSpace / (1024.0 * 1024.0 * 1024.0);
                var usedSpaceGB = (totalSpace - freeSpace) / (1024.0 * 1024.0 * 1024.0);
                SystemStatusCards.Add(new SystemStatusCard
                {
                    Title = "磁盘空间",
                    Value = $"{usedSpaceGB:F0}GB / {totalSpaceGB:F0}GB",
                    Icon = "\uE7B8",
                    Status = diskUsage < 90 ? "正常" : "空间不足"
                });

                // 获取网络状态
                var (isConnected, connectionType, _, _) = await _systemStatusService.GetNetworkStatusAsync();
                SystemStatusCards.Add(new SystemStatusCard
                {
                    Title = "网络状态",
                    Value = isConnected ? connectionType : "未连接",
                    Icon = "\uE968",
                    Status = isConnected ? "已连接" : "断开连接"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载系统状态失败: {ex.Message}");

                // 添加错误状态卡片
                SystemStatusCards.Add(new SystemStatusCard
                {
                    Title = "系统状态",
                    Value = "加载失败",
                    Icon = "\uE783",
                    Status = "错误"
                });
            }
        }
    }

    /// <summary>
    /// 功能卡片模型
    /// </summary>
    public class FunctionCard
    {
        /// <summary>
        /// 标题
        /// </summary>
        public required string Title { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public required string Description { get; set; }
        
        /// <summary>
        /// 图标
        /// </summary>
        public required string Icon { get; set; }
        
        /// <summary>
        /// 导航键
        /// </summary>
        public required string NavigationKey { get; set; }
    }

    /// <summary>
    /// 系统状态卡片模型
    /// </summary>
    public class SystemStatusCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 快速操作卡片模型
    /// </summary>
    public class QuickActionCard
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public ICommand? ActionCommand { get; set; }
    }
}