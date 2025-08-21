using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.UI.Dispatching;

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
        
        // 实时刷新相关
        private DispatcherQueueTimer? _refreshTimer;
        private SystemStatusCard? _cpuCard;
        private SystemStatusCard? _memoryCard;
        private SystemStatusCard? _diskCard;
        private SystemStatusCard? _networkCard;
        
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

            // 异步加载一次
            _ = LoadSystemStatusAsync();
            
            // 启动定时刷新
            StartAutoRefresh();

            // 初始化快速操作卡片
            QuickActionCards = new ObservableCollection<QuickActionCard>
            {
                new QuickActionCard
                {
                    Title = "系统清理",
                    Description = "清理临时文件和垃圾",
                    Icon = "\uE74D",
                    ActionCommand = new WinVault.Infrastructure.RelayCommand<object>(_ =>
                    {
                        try { Process.Start(new ProcessStartInfo("cleanmgr.exe") { UseShellExecute = true }); } catch { }
                    })
                },
                new QuickActionCard
                {
                    Title = "性能优化",
                    Description = "优化系统性能设置",
                    Icon = "\uE7C3",
                    ActionCommand = new WinVault.Infrastructure.RelayCommand<object>(_ =>
                    {
                        try { Process.Start(new ProcessStartInfo("ms-settings:display") { UseShellExecute = true }); } catch { }
                    })
                },
                new QuickActionCard
                {
                    Title = "安全扫描",
                    Description = "扫描系统安全状态",
                    Icon = "\uE72E",
                    ActionCommand = new WinVault.Infrastructure.RelayCommand<object>(_ =>
                    {
                        try { Process.Start(new ProcessStartInfo("ms-settings:windowsdefender") { UseShellExecute = true }); } catch { }
                    })
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
        /// 异步加载系统状态数据（首次或占位）
        /// </summary>
        private async Task LoadSystemStatusAsync()
        {
            try
            {
                // 先创建占位卡片，避免后续重复 Add
                if (_cpuCard == null)
                {
                    _cpuCard = new SystemStatusCard { Title = "CPU使用率", Icon = "\uE7F4", Value = "--", Status = "-" };
                    SystemStatusCards.Add(_cpuCard);
                }
                if (_memoryCard == null)
                {
                    _memoryCard = new SystemStatusCard { Title = "内存使用", Icon = "\uE950", Value = "--", Status = "-" };
                    SystemStatusCards.Add(_memoryCard);
                }
                if (_diskCard == null)
                {
                    _diskCard = new SystemStatusCard { Title = "磁盘空间", Icon = "\uE7B8", Value = "--", Status = "-" };
                    SystemStatusCards.Add(_diskCard);
                }
                if (_networkCard == null)
                {
                    _networkCard = new SystemStatusCard { Title = "网络状态", Icon = "\uE968", Value = "--", Status = "-" };
                    SystemStatusCards.Add(_networkCard);
                }

                await RefreshSystemStatusAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载系统状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 周期刷新系统状态
        /// </summary>
        private void StartAutoRefresh()
        {
            var dq = App.CurrentWindow?.DispatcherQueue ?? App.MainWindow?.DispatcherQueue;
            if (dq == null) return;

            _refreshTimer = dq.CreateTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(2);
            _refreshTimer.IsRepeating = true;
            _refreshTimer.Tick += async (s, e) => { await RefreshSystemStatusAsync(); };
            _refreshTimer.Start();
        }

        private async Task RefreshSystemStatusAsync()
        {
            try
            {
                // CPU
                var cpuUsage = await _systemStatusService.GetCpuUsageAsync();
                if (_cpuCard != null)
                {
                    _cpuCard.Value = $"{cpuUsage:F1}%";
                    _cpuCard.Status = cpuUsage < 80 ? "正常" : "高负载";
                }

                // Memory
                var (totalMemory, availableMemory, memoryUsage) = await _systemStatusService.GetMemoryUsageAsync();
                var totalGB = totalMemory / (1024.0 * 1024.0 * 1024.0);
                var usedGB = (totalMemory - availableMemory) / (1024.0 * 1024.0 * 1024.0);
                if (_memoryCard != null)
                {
                    _memoryCard.Value = $"{usedGB:F1}GB / {totalGB:F1}GB";
                    _memoryCard.Status = memoryUsage < 80 ? "正常" : "高使用率";
                }

                // Disk
                var (totalSpace, freeSpace, diskUsage) = await _systemStatusService.GetDiskUsageAsync();
                var totalSpaceGB = totalSpace / (1024.0 * 1024.0 * 1024.0);
                var usedSpaceGB = (totalSpace - freeSpace) / (1024.0 * 1024.0 * 1024.0);
                if (_diskCard != null)
                {
                    _diskCard.Value = $"{usedSpaceGB:F0}GB / {totalSpaceGB:F0}GB";
                    _diskCard.Status = diskUsage < 90 ? "正常" : "空间不足";
                }

                // Network
                var (isConnected, connectionType, _, _) = await _systemStatusService.GetNetworkStatusAsync();
                if (_networkCard != null)
                {
                    _networkCard.Value = isConnected ? connectionType : "未连接";
                    _networkCard.Status = isConnected ? "已连接" : "断开连接";
                }
                
                // 通知集合项变更（简单做法：触发属性变更）
                OnPropertyChanged(nameof(SystemStatusCards));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"刷新系统状态失败: {ex.Message}");
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
    public class SystemStatusCard : ViewModelBase
    {
        private string _title = string.Empty;
        private string _value = string.Empty;
        private string _icon = string.Empty;
        private string _status = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
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