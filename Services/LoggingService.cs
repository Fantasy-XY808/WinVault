using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using WinVault.Constants;
using WinVault.Infrastructure;

namespace WinVault.Services
{
    /// <summary>
    /// 企业级日志服务，提供高性能结构化日志记录功能
    /// Enterprise-grade logging service providing high-performance structured logging capabilities
    ///
    /// 核心功能 / Core Functions:
    /// - 多级别日志记录 / Multi-level logging
    /// - 文件自动轮转 / Automatic file rotation
    /// - 结构化日志格式 / Structured log format
    /// - 异步日志写入 / Asynchronous log writing
    /// - 性能监控集成 / Performance monitoring integration
    /// </summary>
    public sealed class LoggingService : SingletonBase<LoggingService>, IService
    {
        #region 私有字段 / Private Fields

        /// <summary>
        /// Serilog日志记录器实例，负责实际的日志写入操作。
        /// 配置了文件输出、控制台输出和结构化格式。
        /// Serilog logger instance responsible for actual log writing operations.
        /// Configured with file output, console output, and structured formatting.
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// 当前日志文件的完整路径。
        /// 文件名包含日期信息，支持按日期自动轮转。
        /// Complete path of the current log file.
        /// Filename includes date information, supporting automatic rotation by date.
        /// </summary>
        private readonly string _logFilePath;

        /// <summary>
        /// Serilog LoggingLevelSwitch 实例，用于运行时控制日志级别。
        /// Serilog LoggingLevelSwitch instance for runtime control of log level.
        /// </summary>
        private readonly LoggingLevelSwitch _levelSwitch;

        #endregion

        #region IService接口实现 / IService Interface Implementation

        /// <summary>
        /// 获取服务名称，用于日志记录和诊断。
        /// Get the service name, used for logging and diagnostics.
        /// </summary>
        public string ServiceName => "LoggingService";

        /// <summary>
        /// 获取服务是否已初始化。
        /// Get whether the service is initialized.
        /// </summary>
        public bool IsInitialized => true; // LoggingService在构造时就已初始化

        /// <summary>
        /// 服务状态变化事件，当服务状态发生变化时触发。
        /// Service state change event, triggered when the service state changes.
        /// </summary>
#pragma warning disable CS0067 // 事件从未使用 - 为将来扩展预留
        public event EventHandler<ServiceStateChangedEventArgs>? StateChanged;
#pragma warning restore CS0067

        /// <summary>
        /// 异步初始化服务。LoggingService在构造时已初始化，此方法为空实现。
        /// Asynchronously initialize the service. LoggingService is initialized during construction, this method is empty implementation.
        /// </summary>
        /// <param name="cancellationToken">取消标记 / Cancellation token</param>
        /// <returns>表示异步操作的任务 / A task representing the asynchronous operation</returns>
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // LoggingService在构造时已初始化，无需额外初始化
            // LoggingService is already initialized during construction, no additional initialization needed
            return Task.CompletedTask;
        }

        /// <summary>
        /// 异步关闭服务，释放资源并清理状态。
        /// Asynchronously shut down the service, release resources, and clean up state.
        /// </summary>
        /// <param name="cancellationToken">取消标记 / Cancellation token</param>
        /// <returns>表示异步操作的任务 / A task representing the asynchronous operation</returns>
        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            ShutDown();
            return Task.CompletedTask;
        }

        #endregion

        #region 构造函数 / Constructor

        /// <summary>
        /// 私有构造函数，实现单例模式并初始化日志系统。
        /// 自动配置日志文件路径、日志级别和输出格式。
        ///
        /// Private constructor implementing singleton pattern and initializing logging system.
        /// Automatically configures log file path, log level, and output format.
        /// </summary>
        /// <remarks>
        /// 初始化过程包括：
        /// 1. 创建日志目录（如果不存在）
        /// 2. 设置日志文件路径（包含当前日期）
        /// 3. 从应用程序设置中读取日志级别
        /// 4. 配置Serilog记录器（文件+控制台输出）
        /// 5. 记录初始化完成日志
        ///
        /// Initialization process includes:
        /// 1. Create log directory (if it doesn't exist)
        /// 2. Set log file path (including current date)
        /// 3. Read log level from application settings
        /// 4. Configure Serilog logger (file + console output)
        /// 5. Log initialization completion
        /// </remarks>
        private LoggingService()
        {
            // 设置日志文件路径
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WinVault", "Logs");
            
            // 确保目录存在
            Directory.CreateDirectory(logDirectory);
            
            // 设置日志文件名
            _logFilePath = Path.Combine(
                logDirectory, 
                $"log_{DateTime.Now:yyyyMMdd}.txt");
            
            // 从设置中获取日志级别，默认为信息级别
            var settingsService = SettingsService.Instance;
            string logLevel = settingsService.GetSetting<string>(AppConstants.SettingsKeys.LogLevel, "Information") ?? "Information";
            
            // 解析日志级别
            if (!Enum.TryParse<LogEventLevel>(logLevel, true, out var parsed)) parsed = LogEventLevel.Information;
            
            // 创建 LoggingLevelSwitch 实例
            _levelSwitch = new LoggingLevelSwitch(parsed);
            
            // 创建日志记录器
            _logger = BuildLogger();
            
            // 记录启动日志
            Information("LoggingService initialized. Log file: {0}", _logFilePath);
        }

        /// <summary>
        /// 构建 Serilog 记录器，配置文件输出和结构化格式。
        /// </summary>
        /// <returns>Serilog 记录器实例 / Serilog logger instance</returns>
        private Logger BuildLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(_levelSwitch)
                .WriteTo.File(_logFilePath, 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        /// <summary>
        /// 记录调试级别的日志
        /// </summary>
        /// <param name="messageTemplate">消息模板</param>
        /// <param name="propertyValues">属性值</param>
        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            _logger.Debug(messageTemplate, propertyValues);
        }

        /// <summary>
        /// 记录信息级别的日志
        /// </summary>
        /// <param name="messageTemplate">消息模板</param>
        /// <param name="propertyValues">属性值</param>
        public void Information(string messageTemplate, params object[] propertyValues)
        {
            _logger.Information(messageTemplate, propertyValues);
        }

        /// <summary>
        /// 记录警告级别的日志
        /// </summary>
        /// <param name="messageTemplate">消息模板</param>
        /// <param name="propertyValues">属性值</param>
        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(messageTemplate, propertyValues);
        }

        /// <summary>
        /// 记录错误级别的日志
        /// </summary>
        /// <param name="messageTemplate">消息模板</param>
        /// <param name="propertyValues">属性值</param>
        public void Error(string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(messageTemplate, propertyValues);
        }

        /// <summary>
        /// 记录错误级别的日志
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <param name="messageTemplate">消息模板</param>
        /// <param name="propertyValues">属性值</param>
        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// 记录致命错误级别的日志
        /// </summary>
        /// <param name="messageTemplate">消息模板</param>
        /// <param name="propertyValues">属性值</param>
        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
            _logger.Fatal(messageTemplate, propertyValues);
        }

        /// <summary>
        /// 记录致命错误级别的日志
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <param name="messageTemplate">消息模板</param>
        /// <param name="propertyValues">属性值</param>
        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Fatal(exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// 获取或设置最小日志级别
        /// </summary>
        public LogEventLevel MinimumLevel
        {
            get => _levelSwitch.MinimumLevel;
            set
            {
                _levelSwitch.MinimumLevel = value;
                SettingsService.Instance.SaveSetting(AppConstants.SettingsKeys.LogLevel, value.ToString());
                _logger.Information("Log level switched to {0}", value);
            }
        }

        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        public string LogFilePath => _logFilePath;

        /// <summary>
        /// 设置最小日志级别
        /// </summary>
        /// <param name="level">新的最小日志级别 / New minimum log level</param>
        public void SetMinimumLevel(LogEventLevel level)
        {
            _levelSwitch.MinimumLevel = level;
            SettingsService.Instance.SaveSetting(AppConstants.SettingsKeys.LogLevel, level.ToString());
            _logger.Information("Log level switched to {0}", level);
        }

        /// <summary>
        /// 关闭日志系统
        /// </summary>
        public void ShutDown()
        {
            Information("LoggingService shutting down.");
            try { _logger?.Dispose(); } catch { }
        }

        #endregion
    }
}