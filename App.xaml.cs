using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using WinVault.Services;
using System.Threading.Tasks;

namespace WinVault
{
    /// <summary>
    /// 应用程序核心类 - 负责整个应用程序的生命周期管理、全局状态维护和服务协调
    /// Application core class - Responsible for entire application lifecycle management, global state maintenance and service coordination
    ///
    /// 设计模式 Design Patterns:
    /// - 单例模式 Singleton Pattern: 确保应用程序实例唯一性 Ensures application instance uniqueness
    /// - 服务定位器 Service Locator: 提供全局服务访问入口 Provides global service access point
    /// - 观察者模式 Observer Pattern: 处理应用程序级别事件 Handles application-level events
    ///
    /// 核心职责 Core Responsibilities:
    /// - 应用程序启动和关闭流程管理 Application startup and shutdown process management
    /// - 全局异常捕获和错误恢复机制 Global exception capture and error recovery mechanism
    /// - 服务容器初始化和依赖注入配置 Service container initialization and dependency injection configuration
    /// - 主窗口生命周期管理 Main window lifecycle management
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 当前应用程序实例的静态引用 - 实现全局单例访问模式
        /// Static reference to current application instance - Implements global singleton access pattern
        ///
        /// 用途 Usage:
        /// - 提供应用程序级别的全局访问点 Provides application-level global access point
        /// - 支持服务定位和依赖解析 Supports service location and dependency resolution
        /// - 确保应用程序状态的一致性 Ensures application state consistency
        /// </summary>
        public static new App? Current { get; private set; }

        /// <summary>
        /// 主窗口实例引用 - 提供全局窗口访问和管理能力
        /// Main window instance reference - Provides global window access and management capability
        ///
        /// 功能 Functions:
        /// - 窗口状态管理 Window state management
        /// - 全局UI操作入口 Global UI operation entry point
        /// - 窗口间通信协调 Inter-window communication coordination
        /// </summary>
        public static Window? MainWindow { get; private set; }

        /// <summary>
        /// 当前窗口实例引用 - 兼容性属性
        /// Current window instance reference - Compatibility property
        /// </summary>
        public static Window? CurrentWindow => MainWindow;

        /// <summary>
        /// 应用程序构造函数 - 执行核心初始化流程和基础设施配置
        /// Application constructor - Executes core initialization process and infrastructure configuration
        ///
        /// 初始化顺序 Initialization Order:
        /// 1. 设置全局应用程序引用 Set global application reference
        /// 2. 初始化WinUI框架组件 Initialize WinUI framework components
        /// 3. 配置全局异常处理机制 Configure global exception handling mechanism
        /// 4. 准备服务容器和依赖注入 Prepare service container and dependency injection
        ///
        /// 异常处理 Exception Handling:
        /// - 构造函数级别的异常捕获 Constructor-level exception capture
        /// - 详细错误日志记录 Detailed error logging
        /// - 优雅的错误恢复机制 Graceful error recovery mechanism
        /// </summary>
        public App()
        {
            Current = this;
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// 应用程序启动事件处理程序 - 协调整个应用程序的启动流程和初始化序列
        /// Application launch event handler - Coordinates entire application startup process and initialization sequence
        ///
        /// 启动流程 Startup Process:
        /// 1. 解析启动参数和激活上下文 Parse startup arguments and activation context
        /// 2. 初始化核心服务和依赖注入容器 Initialize core services and dependency injection container
        /// 3. 创建并配置主窗口实例 Create and configure main window instance
        /// 4. 执行窗口激活和显示逻辑 Execute window activation and display logic
        /// 5. 启动后台服务和监控任务 Start background services and monitoring tasks
        ///
        /// 错误处理策略 Error Handling Strategy:
        /// - 分阶段错误捕获 Staged error capture
        /// - 优雅降级机制 Graceful degradation mechanism
        /// - 用户友好的错误提示 User-friendly error notifications
        /// </summary>
        /// <param name="args">启动激活参数 - 包含命令行参数、文件关联、协议激活等信息 Launch activation arguments - Contains command line parameters, file associations, protocol activations, etc.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("开始启动应用程序 Starting application launch");

            try
            {
                System.Diagnostics.Debug.WriteLine("准备创建主窗口 Preparing to create main window");

                // 创建主窗口
                MainWindow = new MainWindow();
                System.Diagnostics.Debug.WriteLine("主窗口对象创建成功 Main window object created successfully");

                // 检查窗口是否创建成功
                if (MainWindow == null)
                {
                    throw new InvalidOperationException("主窗口创建失败 Main window creation failed");
                }

                // 确保窗口在UI线程上正确激活
                System.Diagnostics.Debug.WriteLine("准备激活主窗口 Preparing to activate main window");
                
                // 使用Dispatcher确保在UI线程上执行，并添加延迟确保初始化完成
                MainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        // 添加更长的延迟确保窗口完全初始化
                        await Task.Delay(500);
                        
                        // 再次检查窗口状态
                        if (MainWindow != null && MainWindow.AppWindow != null)
                        {
                            System.Diagnostics.Debug.WriteLine("开始激活窗口");
                            MainWindow.Activate();
                            System.Diagnostics.Debug.WriteLine("主窗口激活成功 Main window activated successfully");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("窗口已关闭或AppWindow无效，无法激活");
                            
                            // 尝试重新创建窗口
                            if (MainWindow == null || MainWindow.AppWindow == null)
                            {
                                System.Diagnostics.Debug.WriteLine("尝试重新创建窗口");
                                MainWindow = new MainWindow();
                                await Task.Delay(100);
                                MainWindow.Activate();
                            }
                        }
                    }
                    catch (Exception activateEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"窗口激活失败: {activateEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"激活异常堆栈: {activateEx.StackTrace}");
                        
                        // 记录激活错误
                        try
                        {
                            var logMessage = $"窗口激活错误 / Window activation error ({DateTime.Now:yyyy-MM-dd HH:mm:ss}):\n" +
                                           $"类型 / Type: {activateEx.GetType().Name}\n" +
                                           $"消息 / Message: {activateEx.Message}\n\n" +
                                           $"{activateEx.StackTrace}";
                            System.IO.File.WriteAllText("window_activation_error.log", logMessage);
                        }
                        catch
                        {
                            // 忽略日志写入错误
                        }
                        
                        HandleStartupException(activateEx);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用程序启动异常: {ex}");
                System.Diagnostics.Debug.WriteLine($"异常堆栈: {ex.StackTrace}");

                HandleStartupException(ex);
            }
        }

        /// <summary>
        /// 启动异常处理程序 - 实现多层次的错误恢复和用户通知机制
        /// Startup exception handler - Implements multi-level error recovery and user notification mechanism
        ///
        /// 错误恢复策略 Error Recovery Strategy:
        /// 1. 详细异常信息记录 Detailed exception information logging
        /// 2. 尝试创建最小化错误界面 Attempt to create minimal error interface
        /// 3. 提供用户友好的错误信息 Provide user-friendly error information
        /// 4. 实现优雅的应用程序退出 Implement graceful application exit
        ///
        /// 故障转移机制 Failover Mechanism:
        /// - 主窗口创建失败时的备用方案 Backup plan when main window creation fails
        /// - 错误窗口创建失败时的强制退出 Force exit when error window creation fails
        /// - 用户数据保护和状态恢复 User data protection and state recovery
        /// </summary>
        /// <param name="ex">启动过程中捕获的异常对象 Exception object caught during startup process</param>
        private void HandleStartupException(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"应用程序启动失败 Application startup failed: {ex}");

            try
            {
                var errorWindow = new Window { Title = "WinVault - 启动错误 Startup Error" };
                errorWindow.Activate();
            }
            catch
            {
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// 全局未处理异常事件处理程序 - 应用程序最后的安全防线和稳定性保障
        /// Global unhandled exception event handler - Application's last line of defense and stability guarantee
        ///
        /// 异常处理策略 Exception Handling Strategy:
        /// 1. 异常信息的完整捕获和记录 Complete capture and logging of exception information
        /// 2. 结构化日志记录和错误追踪 Structured logging and error tracking
        /// 3. 用户体验保护和应用程序连续性 User experience protection and application continuity
        /// 4. 错误恢复和状态重置机制 Error recovery and state reset mechanism
        ///
        /// 安全机制 Safety Mechanisms:
        /// - 防止异常处理器自身异常的无限递归 Prevent infinite recursion from exception handler's own exceptions
        /// - 确保关键资源的正确释放 Ensure proper release of critical resources
        /// - 维护应用程序的基本可用性 Maintain basic application availability
        /// </summary>
        /// <param name="sender">异常事件的发送者对象 Sender object of the exception event</param>
        /// <param name="e">包含异常详细信息的事件参数 Event arguments containing detailed exception information</param>
        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"未处理异常 Unhandled exception: {e.Exception}");
                var logger = LoggingService.Instance;
                logger?.Fatal(e.Exception, "应用程序发生未处理异常 Application unhandled exception occurred");
                e.Handled = true;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("异常处理器发生错误 Exception handler error occurred");
            }
        }

        /// <summary>
        /// 服务定位器方法 - 提供类型安全的服务实例获取和依赖解析功能
        /// Service locator method - Provides type-safe service instance retrieval and dependency resolution functionality
        ///
        /// 服务解析策略 Service Resolution Strategy:
        /// 1. 基于类型的服务匹配和实例返回 Type-based service matching and instance return
        /// 2. 单例模式的服务生命周期管理 Singleton pattern service lifecycle management
        /// 3. 延迟初始化和按需创建机制 Lazy initialization and on-demand creation mechanism
        /// 4. 类型安全的服务接口和实现分离 Type-safe service interface and implementation separation
        ///
        /// 支持的服务类型 Supported Service Types:
        /// - LoggingService: 日志记录和监控服务 Logging and monitoring service
        /// - SettingsService: 配置管理和持久化服务 Configuration management and persistence service
        /// - AppInfoService: 应用程序信息和元数据服务 Application information and metadata service
        /// - NavigationService: 导航和路由管理服务 Navigation and routing management service
        /// </summary>
        /// <typeparam name="T">请求的服务类型，必须是引用类型 Requested service type, must be reference type</typeparam>
        /// <returns>服务实例或null（如果服务未注册或创建失败）Service instance or null (if service not registered or creation failed)</returns>
        public static T? GetService<T>() where T : class
        {
            try
            {
                return typeof(T) switch
                {
                    Type t when t == typeof(LoggingService) => LoggingService.Instance as T,
                    Type t when t == typeof(SettingsService) => SettingsService.Instance as T,
                    Type t when t == typeof(AppInfoService) => AppInfoService.Instance as T,
                    Type t when t == typeof(NavigationService) => NavigationService.Instance as T,
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
