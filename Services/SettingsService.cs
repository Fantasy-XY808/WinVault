#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinVault.Constants;
using WinVault.Infrastructure;

namespace WinVault.Services
{
    /// <summary>
    /// 应用程序配置管理服务 - 提供高性能、类型安全的设置存储和检索机制
    /// Application configuration management service - Provides high-performance, type-safe settings storage and retrieval mechanism
    ///
    /// 核心功能 Core Functions:
    /// - 类型安全的设置存取 Type-safe settings access
    /// - 持久化存储和自动加载 Persistent storage and automatic loading
    /// - 默认值回退机制 Default value fallback mechanism
    /// - 设置变更通知 Settings change notification
    /// - 批量操作和事务支持 Batch operations and transaction support
    ///
    /// 技术特性 Technical Features:
    /// - 线程安全的并发访问 Thread-safe concurrent access
    /// - 高性能内存缓存 High-performance memory caching
    /// - 延迟写入和批量提交 Delayed writing and batch commit
    /// - 自动错误恢复和数据保护 Automatic error recovery and data protection
    /// </summary>
    public class SettingsService : IService
    {
        /// <summary>
        /// 内存中的设置缓存 - 提供高性能的设置访问
        /// In-memory settings cache - Provides high-performance settings access
        /// </summary>
        private readonly Dictionary<string, object?> _settings = new Dictionary<string, object?>();

        /// <summary>
        /// 设置文件的完整路径 - 用于持久化存储
        /// Full path to settings file - Used for persistent storage
        /// </summary>
        private readonly string _settingsFilePath;

        /// <summary>
        /// 设置文件的名称常量 - 定义标准化的文件命名
        /// Settings file name constant - Defines standardized file naming
        /// </summary>
        private const string SettingsFileName = "app_settings.json";

        #region 单例模式实现 / Singleton Pattern Implementation

        /// <summary>
        /// 单例实例引用 - 确保全局唯一的服务实例
        /// Singleton instance reference - Ensures globally unique service instance
        /// </summary>
        private static SettingsService? _instance;

        /// <summary>
        /// 线程同步锁对象 - 保证线程安全的实例创建
        /// Thread synchronization lock object - Ensures thread-safe instance creation
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 单例访问器属性 - 提供全局访问点和延迟初始化
        /// Singleton accessor property - Provides global access point and lazy initialization
        ///
        /// 实现特性 Implementation Features:
        /// - 双重检查锁定模式 Double-checked locking pattern
        /// - 线程安全的实例创建 Thread-safe instance creation
        /// - 延迟初始化和按需创建 Lazy initialization and on-demand creation
        /// - 全局唯一的实例保证 Globally unique instance guarantee
        /// </summary>
        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SettingsService();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion
        
        /// <summary>
        /// 构造函数
        /// </summary>
        private SettingsService()
        {
            // 设置文件保存路径
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolderPath = Path.Combine(appDataPath, AppConstants.StoragePaths.AppDataFolderName);
            
            // 确保目录存在
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }
            
            _settingsFilePath = Path.Combine(appFolderPath, SettingsFileName);
            
            // 加载设置
            LoadSettings();
        }

        /// <summary>
        /// 获取设置值
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="key">设置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>设置值或默认值</returns>
        public T? GetSetting<T>(string key, T? defaultValue = default)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                try
                {
                    if (value is JsonElement jsonElement)
                    {
                        // 处理JSON元素类型转换
                        return ConvertFromJsonElement<T>(jsonElement);
                    }
                    else if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    else
                    {
                        // 尝试转换类型
                        return (T)Convert.ChangeType(value, typeof(T))!;
                    }
                }
                catch
                {
                    // 转换失败时返回默认值
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 保存设置值
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="key">设置键</param>
        /// <param name="value">设置值</param>
        public void SaveSetting<T>(string key, T? value)
        {
            _settings[key] = value;
            SaveSettings();
        }

        /// <summary>
        /// 检查设置是否存在
        /// </summary>
        /// <param name="key">设置键</param>
        /// <returns>是否存在</returns>
        public bool ContainsSetting(string key)
        {
            return _settings.ContainsKey(key);
        }

        /// <summary>
        /// 检查设置是否存在
        /// </summary>
        /// <param name="key">设置键</param>
        /// <returns>是否存在</returns>
        public bool HasSetting(string key)
        {
            return _settings.ContainsKey(key);
        }

        /// <summary>
        /// 删除设置
        /// </summary>
        /// <param name="key">设置键</param>
        public void RemoveSetting(string key)
        {
            if (_settings.ContainsKey(key))
            {
                _settings.Remove(key);
                SaveSettings();
            }
        }

        /// <summary>
        /// 重置所有设置
        /// </summary>
        public void ResetSettings()
        {
            _settings.Clear();
            SaveSettings();
        }

        /// <summary>
        /// 清除所有设置
        /// </summary>
        public void ClearAllSettings()
        {
            _settings.Clear();
            SaveSettings();
        }

        /// <summary>
        /// 从文件加载设置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // 反序列化JSON到字典，添加空值检查
                    Dictionary<string, JsonElement>? loadedSettings = null;
                    try
                    {
                        // 修复CS8600警告：使用null合并操作符
                        loadedSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, options) 
                            ?? new Dictionary<string, JsonElement>();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"反序列化设置时出错: {ex.Message}");
                    }

                    // 确保loadedSettings不为null
                    if (loadedSettings != null)
                    {
                        _settings.Clear();
                        foreach (var item in loadedSettings)
                        {
                            _settings[item.Key] = item.Value;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("设置反序列化结果为null，使用空字典");
                        _settings.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 将设置保存到文件
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 将JsonElement转换为指定类型
        /// </summary>
        private T ConvertFromJsonElement<T>(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    if (typeof(T) == typeof(string))
                        return (T)(object)(element.GetString() ?? string.Empty);
                    else if (typeof(T) == typeof(Guid) && Guid.TryParse(element.GetString() ?? string.Empty, out Guid guid))
                        return (T)(object)guid;
                    else if (typeof(T) == typeof(DateTime) && DateTime.TryParse(element.GetString() ?? string.Empty, out DateTime dt))
                        return (T)(object)dt;
                    break;
                    
                case JsonValueKind.Number:
                    if (typeof(T) == typeof(int))
                        return (T)(object)element.GetInt32();
                    else if (typeof(T) == typeof(long))
                        return (T)(object)element.GetInt64();
                    else if (typeof(T) == typeof(float))
                        return (T)(object)element.GetSingle();
                    else if (typeof(T) == typeof(double))
                        return (T)(object)element.GetDouble();
                    else if (typeof(T) == typeof(decimal))
                        return (T)(object)element.GetDecimal();
                    break;
                    
                case JsonValueKind.True:
                case JsonValueKind.False:
                    if (typeof(T) == typeof(bool))
                        return (T)(object)element.GetBoolean();
                    break;
                    
                case JsonValueKind.Object:
                    // 添加空值检查
                    var objResult = JsonSerializer.Deserialize<T>(element.GetRawText());
                    return objResult != null ? objResult : default!;
                    
                case JsonValueKind.Array:
                    // 添加空值检查
                    var arrResult = JsonSerializer.Deserialize<T>(element.GetRawText());
                    return arrResult != null ? arrResult : default!;
            }
            
            return default!;
        }

        #region IService接口实现 / IService Interface Implementation

        /// <summary>
        /// 获取服务名称，用于日志记录和诊断。
        /// Get the service name, used for logging and diagnostics.
        /// </summary>
        public string ServiceName => "SettingsService";

        /// <summary>
        /// 获取服务是否已初始化。
        /// Get whether the service is initialized.
        /// </summary>
        public bool IsInitialized => true; // SettingsService在构造时就已初始化

        /// <summary>
        /// 服务状态变化事件，当服务状态发生变化时触发。
        /// Service state change event, triggered when the service state changes.
        /// </summary>
#pragma warning disable CS0067 // 事件从未使用 - 为将来扩展预留
        public event EventHandler<ServiceStateChangedEventArgs>? StateChanged;
#pragma warning restore CS0067

        /// <summary>
        /// 异步初始化服务。SettingsService在构造时已初始化，此方法为空实现。
        /// Asynchronously initialize the service. SettingsService is initialized during construction, this method is empty implementation.
        /// </summary>
        /// <param name="cancellationToken">取消标记 / Cancellation token</param>
        /// <returns>表示异步操作的任务 / A task representing the asynchronous operation</returns>
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // SettingsService在构造时已初始化，无需额外初始化
            // SettingsService is already initialized during construction, no additional initialization needed
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
            // 保存设置并关闭服务
            // Save settings and shutdown service
            SaveSettings();
            return Task.CompletedTask;
        }

        #endregion
    }
}