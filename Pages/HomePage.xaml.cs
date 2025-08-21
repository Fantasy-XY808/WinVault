// ================================================================
// WinVault - Home Page Implementation
// Copyright (c) 2024 WinVault Team. All rights reserved.
// Licensed under the GPL-3.0 License.
// Author: WinVault Development Team
// Created: 2024-12-20
// Last Modified: 2024-12-20
// Version: 2.0.0
// Purpose: Main dashboard page providing system overview, quick actions,
//          and navigation to key application features
// ================================================================

#nullable enable
using WinVault.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using WinVault.Services;
using WinVault.Constants;
using System;
using System.Numerics;

namespace WinVault.Pages
{
    /// <summary>
    /// 应用程序主页面，作为用户的主要入口点和控制中心。
    /// 提供系统状态概览、快速操作面板、常用功能导航和个性化仪表板。
    /// 采用现代化的卡片式布局，支持响应式设计和流畅的交互动画。
    ///
    /// Main application page serving as the primary entry point and control center for users.
    /// Provides system status overview, quick action panel, common feature navigation, and personalized dashboard.
    /// Uses modern card-based layout with responsive design and smooth interaction animations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>页面功能 / Page Features:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>系统状态监控 / System Status Monitoring</term>
    /// <description>实时显示CPU、内存、磁盘、网络等系统资源使用情况 / Real-time display of system resource usage including CPU, memory, disk, network</description>
    /// </item>
    /// <item>
    /// <term>快速操作面板 / Quick Action Panel</term>
    /// <description>提供常用系统工具和功能的快速访问入口 / Provides quick access to common system tools and features</description>
    /// </item>
    /// <item>
    /// <term>智能推荐 / Smart Recommendations</term>
    /// <description>基于用户使用习惯推荐相关功能和工具 / Recommends relevant features and tools based on user usage patterns</description>
    /// </item>
    /// <item>
    /// <term>响应式布局 / Responsive Layout</term>
    /// <description>自适应不同屏幕尺寸，提供最佳的视觉体验 / Adapts to different screen sizes, providing optimal visual experience</description>
    /// </item>
    /// <item>
    /// <term>流畅动画 / Smooth Animations</term>
    /// <description>卡片悬停、点击反馈等微交互动画 / Micro-interaction animations including card hover and click feedback</description>
    /// </item>
    /// </list>
    ///
    /// <para>
    /// <strong>技术特性 / Technical Features:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><strong>MVVM架构:</strong> 使用HomeViewModel进行数据绑定和业务逻辑分离 / Uses HomeViewModel for data binding and business logic separation</item>
    /// <item><strong>页面缓存:</strong> 启用导航缓存提升页面切换性能 / Enables navigation cache to improve page switching performance</item>
    /// <item><strong>异步加载:</strong> 系统状态数据异步获取，避免UI阻塞 / Asynchronous system status data retrieval to avoid UI blocking</item>
    /// <item><strong>内存优化:</strong> 合理的资源管理和对象生命周期控制 / Proper resource management and object lifecycle control</item>
    /// </list>
    ///
    /// <para>
    /// <strong>用户体验设计 / User Experience Design:</strong><br/>
    /// 页面采用Material Design和Fluent Design的设计原则，注重信息层次、
    /// 视觉平衡和交互一致性。通过合理的颜色搭配、字体层次和空间布局，
    /// 为用户提供直观、高效的操作体验。
    /// The page adopts design principles from Material Design and Fluent Design, focusing on information hierarchy,
    /// visual balance, and interaction consistency. Through proper color matching, font hierarchy, and spatial layout,
    /// it provides users with an intuitive and efficient operating experience.
    /// </para>
    /// </remarks>
    public sealed partial class HomePage : Page
    {
        #region 属性 / Properties

        /// <summary>
        /// 主页视图模型，负责管理页面的数据状态和业务逻辑。
        /// 包含系统状态信息、快速操作命令、用户偏好设置等数据。
        /// 通过数据绑定与UI元素进行双向通信。
        ///
        /// Home page view model responsible for managing page data state and business logic.
        /// Contains system status information, quick action commands, user preference settings, and other data.
        /// Communicates bidirectionally with UI elements through data binding.
        /// </summary>
        /// <value>
        /// HomeViewModel实例，在页面创建时初始化，生命周期与页面相同。
        /// HomeViewModel instance, initialized when page is created, with same lifecycle as page.
        /// </value>
        public HomeViewModel ViewModel { get; } = new HomeViewModel();

        #endregion

        #region 构造函数 / Constructor

        /// <summary>
        /// 初始化HomePage的新实例。
        /// 执行页面组件初始化、数据上下文设置和导航缓存配置。
        ///
        /// Initializes a new instance of HomePage.
        /// Performs page component initialization, data context setup, and navigation cache configuration.
        /// </summary>
        /// <remarks>
        /// 构造函数执行的操作：
        /// 1. 初始化XAML定义的UI组件
        /// 2. 设置数据上下文为ViewModel实例
        /// 3. 启用页面导航缓存以提升性能
        /// 4. 配置页面的基本属性和行为
        ///
        /// Operations performed by constructor:
        /// 1. Initialize UI components defined in XAML
        /// 2. Set data context to ViewModel instance
        /// 3. Enable page navigation cache for performance improvement
        /// 4. Configure basic page properties and behaviors
        /// </remarks>
        public HomePage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;

            // 启用页面缓存以提升导航性能和用户体验
            // Enable page caching to improve navigation performance and user experience
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        #endregion

        /// <summary>
        /// 页面加载完成事件
        /// </summary>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 动态显示系统版本
            try
            {
                var status = new SystemStatusService();
                var (osName, osVersion, computerName, installDate) = await status.GetSystemInfoAsync();
                if (!string.IsNullOrWhiteSpace(osName))
                {
                    SystemVersionText.Text = osName;
                }
            }
            catch { }

            // 显示当前时间并每分钟更新
            try
            {
                void updateNow()
                {
                    CurrentTimeText.Text = DateTime.Now.ToString("yyyy年M月d日 HH:mm");
                }
                updateNow();
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
                timer.Tick += (_, __) => updateNow();
                timer.Start();
            }
            catch { }
        }

        /// <summary>
        /// 卡片鼠标进入事件
        /// </summary>
        private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                // 添加鼠标悬停效果
                element.Translation = new System.Numerics.Vector3(0, -4, 0);
                element.Scale = new System.Numerics.Vector3(1.02f, 1.02f, 1);
                
                // 设置阴影效果
                var shadow = element.Shadow as Microsoft.UI.Xaml.Media.ThemeShadow;
                if (shadow == null)
                {
                    shadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
                    element.Shadow = shadow;
                }
                
                element.Translation = new System.Numerics.Vector3(0, -4, 16);
            }
        }

        /// <summary>
        /// 卡片鼠标离开事件
        /// </summary>
        private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                // 恢复原始状态
                element.Translation = new System.Numerics.Vector3(0, 0, 0);
                element.Scale = new System.Numerics.Vector3(1, 1, 1);
            }
        }

        /// <summary>
        /// 快速系统优化按钮点击
        /// </summary>
        private void QuickOptimize_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage("quicksettings");
        }

        /// <summary>
        /// 快速磁盘清理按钮点击
        /// </summary>
        private void QuickDiskCleanup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 启动磁盘清理工具
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cleanmgr.exe";
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"启动磁盘清理失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 快速服务管理按钮点击
        /// </summary>
        private void QuickServices_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage("services");
        }

        /// <summary>
        /// 快速任务管理器按钮点击
        /// </summary>
        private void QuickTaskManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 启动任务管理器
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "taskmgr.exe";
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"启动任务管理器失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 功能卡片点击事件
        /// </summary>
        private void FunctionCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                NavigateToPage(tag);
            }
        }

        /// <summary>
        /// 快速硬件信息按钮点击
        /// </summary>
        private void QuickHardware_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage("hardware");
        }

        /// <summary>
        /// 快速工具箱按钮点击
        /// </summary>
        private void QuickTools_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage("exetools");
        }

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        private void NavigateToPage(string pageTag)
        {
            try
            {
                // 通过Frame直接导航
                var frame = this.Frame;
                if (frame != null)
                {
                    // 更新侧边栏选中状态
                    UpdateNavigationViewSelection(pageTag);
                    
                    switch (pageTag)
                    {
                        case "quicksettings":
                            frame.Navigate(typeof(QuickSettingsPage));
                            break;
                        case "hardware":
                            frame.Navigate(typeof(HardwarePage));
                            break;
                        case "exetools":
                            frame.Navigate(typeof(ExeToolsPage));
                            break;
                        case "services":
                            frame.Navigate(typeof(ServicesPage));
                            break;
                        case "commandquery":
                            frame.Navigate(typeof(CommandQueryPage));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"导航失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新侧边栏选中状态
        /// </summary>
        private void UpdateNavigationViewSelection(string pageTag)
        {
            try
            {
                // 通过App获取主窗口
                if (App.MainWindow is MainWindow mainWindow && mainWindow.NavViewControl != null)
                {
                    var navView = mainWindow.NavViewControl;
                    
                    // 查找对应的NavigationViewItem并设置为选中
                    foreach (var item in navView.MenuItems)
                    {
                        if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == pageTag)
                        {
                            navView.SelectedItem = navItem;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新侧边栏选中状态失败: {ex.Message}");
            }
        }
    }
}