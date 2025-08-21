using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Text;
using System;
using WinVault.Services;
using WinVault.Constants;

namespace WinVault
{
    /// <summary>
    /// 主窗口类，提供应用程序的主要用户界面和导航功能
    /// Main window class providing primary user interface and navigation functionality for the application
    ///
    /// 职责 / Responsibilities:
    /// - 窗口生命周期管理 / Window lifecycle management
    /// - 服务初始化和依赖注入 / Service initialization and dependency injection
    /// - 导航框架集成 / Navigation framework integration
    /// - 全局事件处理 / Global event handling
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        #region 私有字段 / Private Fields

        // 移除未使用的字段以避免警告
        private bool _isInitialized = false;

        // 对外公开的 NavView 引用，便于其他页面即时访问侧边栏
        public NavigationView? NavViewControl => NavView;

        #endregion

        #region 构造函数 / Constructor

        public MainWindow()
        {
            System.Diagnostics.Debug.WriteLine("MainWindow构造函数开始");

            try
            {
                System.Diagnostics.Debug.WriteLine("准备调用InitializeComponent");
                this.InitializeComponent();
                System.Diagnostics.Debug.WriteLine("InitializeComponent完成");

                // 不在这里设置标题，避免任何可能的COM异常
                System.Diagnostics.Debug.WriteLine("跳过标题设置，避免COM异常");

                // 延迟绑定Activated事件，确保窗口完全初始化
                System.Diagnostics.Debug.WriteLine("准备绑定Activated事件");
                this.Activated += MainWindow_Activated;
                System.Diagnostics.Debug.WriteLine("Activated事件绑定完成");

                System.Diagnostics.Debug.WriteLine("MainWindow构造函数完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow构造函数异常: {ex}");
                System.Diagnostics.Debug.WriteLine($"异常详情: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"异常堆栈: {ex.StackTrace}");
                
                // 记录到日志文件
                try
                {
                    var logMessage = $"MainWindow构造函数错误 / MainWindow constructor error ({DateTime.Now:yyyy-MM-dd HH:mm:ss}):\n" +
                                   $"类型 / Type: {ex.GetType().Name}\n" +
                                   $"消息 / Message: {ex.Message}\n\n" +
                                   $"{ex.StackTrace}";
                    System.IO.File.WriteAllText("mainwindow_constructor_error.log", logMessage);
                }
                catch
                {
                    // 忽略日志写入错误
                }
                
                // 记录初始化错误日志
                LogInitializationError(ex);
                
                throw;
            }
        }

        /// <summary>
        /// 记录初始化错误到日志文件
        /// </summary>
        /// <param name="ex">捕获的异常</param>
        private void LogInitializationError(Exception ex)
        {
            try
            {
                var logMessage = $"MainWindow加载错误 / MainWindow loading error ({DateTime.Now:yyyy-MM-dd HH:mm:ss}):\n" +
                               $"类型 / Type: {ex.GetType().Name}\n" +
                               $"消息 / Message: {ex.Message}\n\n";
                
                // 添加内部异常信息
                if (ex.InnerException != null)
                {
                    logMessage += $"内部异常 / Inner exception: {ex.InnerException.Message}\n\n";
                }
                
                logMessage += $"{ex.StackTrace}";
                System.IO.File.WriteAllText("mainwindow_init_error.log", logMessage);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        #endregion

        #region 事件处理器 / Event Handlers

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow已激活，开始安全初始化");

                // 检查激活状态
                if (e.WindowActivationState == WindowActivationState.Deactivated)
                {
                    System.Diagnostics.Debug.WriteLine("窗口处于非激活状态，跳过初始化");
                    return;
                }

                // 防止重复初始化
                if (_isInitialized)
                {
                    System.Diagnostics.Debug.WriteLine("已经初始化过，跳过重复初始化");
                    return;
                }

                // 恢复侧边栏展开
                var settings = SettingsService.Instance;
                bool navExpanded = settings.GetSetting(AppConstants.SettingsKeys.NavPaneExpanded, false);
                if (NavView != null) NavView.IsPaneOpen = navExpanded;

                // 恢复窗口大小/位置
                var width = Math.Max(800, settings.GetSetting(AppConstants.SettingsKeys.WindowWidth, 1000));
                var height = Math.Max(450, settings.GetSetting(AppConstants.SettingsKeys.WindowHeight, 650));
                this.AppWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

                var startPos = settings.GetSetting<string>(AppConstants.SettingsKeys.WindowStartPosition, "Center") ?? "Center";
                if (startPos == "Last")
                {
                    var x = settings.GetSetting(AppConstants.SettingsKeys.WindowX, -1);
                    var y = settings.GetSetting(AppConstants.SettingsKeys.WindowY, -1);
                    if (x >= 0 && y >= 0)
                    {
                        this.AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
                    }
                    else
                    {
                        CenterOnScreen();
                    }
                }
                else
                {
                    CenterOnScreen();
                }

                // 设置窗口标题：优先使用 AppWindow.Title（Windows 10/11 都支持），同时在 Win11 上设置 Window.Title
                try
                {
                    if (this.AppWindow != null)
                    {
                        this.AppWindow.Title = "WinVault";
                    }

                    // 仅在 Windows 11 (build 22000+) 再设置 Window.Title，避免在 Windows 10 抛出 COMException
                    if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                    {
                        if (string.IsNullOrEmpty(this.Title))
                        {
                            this.Title = "WinVault";
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("窗口标题设置完成");
                }
                catch (Exception titleEx)
                {
                    System.Diagnostics.Debug.WriteLine($"设置标题时出错: {titleEx.Message}");
                    // 忽略标题设置错误
                }

                // 检查窗口是否仍然有效
                if (this == null || this.Content == null || this.AppWindow == null)
                {
                    System.Diagnostics.Debug.WriteLine("窗口已无效，跳过NavigationView初始化");
                    return;
                }

                // 安全地初始化NavigationView
                try
                {
                    if (NavView?.MenuItems?.Count > 0)
                    {
                        // 设置初始选中项
                        NavView.SelectedItem = NavView.MenuItems[0];
                        
                        // 直接导航到主页
                        NavigateToPage("home");
                        
                        _isInitialized = true;
                        System.Diagnostics.Debug.WriteLine("NavigationView初始化完成");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("NavigationView未找到或为空");
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationView初始化出错: {navEx.Message}");
                    // 不抛出异常，继续执行
                }

                System.Diagnostics.Debug.WriteLine("MainWindow激活完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow_Activated异常: {ex}");
                // 记录详细错误信息到日志文件
                try
                {
                    var logMessage = $"MainWindow激活错误 / MainWindow activation error ({DateTime.Now:yyyy-MM-dd HH:mm:ss}):\n" +
                                   $"类型 / Type: {ex.GetType().Name}\n" +
                                   $"消息 / Message: {ex.Message}\n\n" +
                                   $"{ex.StackTrace}";
                    System.IO.File.WriteAllText("mainwindow_activation_error.log", logMessage);
                }
                catch
                {
                    // 忽略日志写入错误
                }
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                var settings = SettingsService.Instance;
                if (this.AppWindow != null)
                {
                    settings.SaveSetting(AppConstants.SettingsKeys.WindowWidth, (int)this.AppWindow.Size.Width);
                    settings.SaveSetting(AppConstants.SettingsKeys.WindowHeight, (int)this.AppWindow.Size.Height);
                    settings.SaveSetting(AppConstants.SettingsKeys.WindowX, (int)this.AppWindow.Position.X);
                    settings.SaveSetting(AppConstants.SettingsKeys.WindowY, (int)this.AppWindow.Position.Y);
                }
                if (NavView != null)
                {
                    settings.SaveSetting(AppConstants.SettingsKeys.NavPaneExpanded, NavView.IsPaneOpen);
                }
            }
            catch { }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                // 防止初始化时的重复导航
                if (!_isInitialized)
                {
                    System.Diagnostics.Debug.WriteLine("跳过初始化时的导航事件");
                    return;
                }

                if (args.SelectedItem is NavigationViewItem item)
                {
                    var tag = item.Tag?.ToString();
                    if (!string.IsNullOrEmpty(tag))
                    {
                        System.Diagnostics.Debug.WriteLine($"导航到: {tag}");
                        NavigateToPage(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"导航选择变化时出错: {ex.Message}");
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            try
            {
                if (ContentFrame?.CanGoBack == true)
                {
                    ContentFrame.GoBack();
                    System.Diagnostics.Debug.WriteLine("返回上一页");
                }
                else
                {
                    // 如果没有导航历史，返回主页
                    NavigateToPage("home");
                    System.Diagnostics.Debug.WriteLine("没有导航历史，返回主页");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"返回导航时出错: {ex.Message}");
                // 出错时也返回主页
                NavigateToPage("home");
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"导航失败: {e.SourcePageType?.Name ?? "Unknown"}");

                // 导航到默认页面
                NavigateToPage("home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理导航失败时出错: {ex.Message}");
            }
        }

        #endregion

        #region 导航方法 / Navigation Methods

        private void NavigateToPage(string pageTag)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"尝试导航到页面: {pageTag}");

                Type? pageType = pageTag switch
                {
                    "home" => typeof(Pages.HomePage),
                    "hardware" => typeof(Pages.HardwarePage),
                    "services" => typeof(Pages.ServicesPage),
                    "commandquery" => typeof(Pages.CommandQueryPage),
                    "quicksettings" => typeof(Pages.QuickSettingsPage),
                    "exetools" => typeof(Pages.ExeToolsPage),
                    "settings" => typeof(Pages.SettingsPage),
                    "about" => typeof(Pages.AboutPage),
                    _ => null
                };

                if (pageType != null)
                {
                    ContentFrame.Navigate(pageType);
                    System.Diagnostics.Debug.WriteLine($"成功导航到 {pageType.Name}");
                }
                else
                {
                    ShowErrorContent($"未找到页面: {pageTag}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"页面导航异常: {ex.Message}");
                ShowErrorContent($"导航到 {pageTag} 页面时出错: {ex.Message}");
            }
        }

        private void ShowPageContent(string pageTag)
        {
            try
            {
                if (ContentFrame != null)
                {
                    var page = new Page();
                    var stackPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    string pageTitle = pageTag switch
                    {
                        "home" => "主页",
                        "hardware" => "硬件信息",
                        "services" => "服务管理",
                        "commandquery" => "命令查询",
                        "quicksettings" => "快速设置",
                        "exetools" => "工具箱",
                        "settings" => "软件设置",
                        "about" => "关于",
                        _ => "未知页面"
                    };

                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = pageTitle,
                        FontSize = 32,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    });

                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = $"这里是 {pageTitle} 页面内容",
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    });

                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "页面功能正在开发中...",
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                    });

                    page.Content = stackPanel;
                    ContentFrame.Content = page;

                    System.Diagnostics.Debug.WriteLine($"成功显示 {pageTitle} 页面内容");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示页面内容时出错: {ex.Message}");
            }
        }

        private void ShowErrorContent(string errorMessage)
        {
            try
            {
                if (ContentFrame != null)
                {
                    // 创建一个简单的错误显示
                    var errorPage = new Page();
                    var stackPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = "页面加载错误",
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    });

                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = errorMessage,
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 400
                    });

                    errorPage.Content = stackPanel;
                    ContentFrame.Content = errorPage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示错误内容时出错: {ex.Message}");
            }
        }

        #endregion

        private void CenterOnScreen()
        {
            try
            {
                var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(this.AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
                var workArea = displayArea.WorkArea;
                int x = workArea.X + (workArea.Width - this.AppWindow.Size.Width) / 2;
                int y = workArea.Y + (workArea.Height - this.AppWindow.Size.Height) / 2;
                this.AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
            }
            catch { }
        }
    }
}
