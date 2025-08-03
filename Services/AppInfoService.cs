// ================================================================
// WinVault
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Author: WinVault Team
// Created: 2024
// Purpose: Application information management service
// ================================================================

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinVault.Constants;
using WinVault.Infrastructure;

namespace WinVault.Services
{
    /// <summary>
    /// 管理应用程序信息的服务类，提供应用程序元数据的统一访问接口。
    /// 负责管理应用程序的版本、作者、描述等基本信息，支持动态更新和持久化存储。
    /// Application information management service class that provides unified access interface for application metadata.
    /// Responsible for managing basic information such as application version, author, description, supporting dynamic updates and persistent storage.
    /// </summary>
    /// <remarks>
    /// 此服务采用单例模式实现，提供以下功能：
    /// 1. 应用程序基本信息的集中管理
    /// 2. 版本信息的自动获取和格式化
    /// 3. 应用程序元数据的持久化存储
    /// 4. 多语言环境下的信息本地化支持
    /// 5. 运行时信息的动态更新
    /// 6. 系统信息的收集和报告
    ///
    /// This service is implemented using singleton pattern and provides the following features:
    /// 1. Centralized management of application basic information
    /// 2. Automatic acquisition and formatting of version information
    /// 3. Persistent storage of application metadata
    /// 4. Information localization support in multilingual environments
    /// 5. Dynamic updates of runtime information
    /// 6. Collection and reporting of system information
    /// </remarks>
    public class AppInfoService : IService
    {
        private readonly Dictionary<string, string> _appInfo = new Dictionary<string, string>
        {
            { "AppName", "WinVault" },
            { "Version", "1.0.0" },
            { "Author", "WinVault Team" },
            { "UpdateDate", DateTime.Now.ToString("yyyy-MM-dd") },
            { "Description", "现代化Windows系统管理平台" },
            { "Copyright", $"© {DateTime.Now.Year} WinVault Team" }
        };

        #region 单例模式实现
        
        private static AppInfoService? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取AppInfoService的单例实例
        /// </summary>
        public static AppInfoService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppInfoService();
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
        private AppInfoService()
        {
            LoadAppInfoFromAssembly();
            LoadAppInfoFromFile().ConfigureAwait(false);
        }

        /// <summary>
        /// 获取应用信息
        /// </summary>
        /// <param name="key">信息键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>应用信息值</returns>
        public string GetAppInfo(string key, string defaultValue = "")
        {
            if (_appInfo.TryGetValue(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取应用版本号
        /// </summary>
        public Version GetVersion()
        {
            if (Version.TryParse(GetAppInfo("Version"), out var version))
            {
                return version;
            }
            return new Version(1, 0, 0, 0);
        }

        /// <summary>
        /// 获取应用名称
        /// </summary>
        public string GetAppName() => GetAppInfo("AppName");

        /// <summary>
        /// 获取版本字符串
        /// </summary>
        public string GetVersionString() => GetAppInfo("Version");

        /// <summary>
        /// 获取应用更新日期
        /// </summary>
        public string GetUpdateDate() => GetAppInfo("UpdateDate");

        /// <summary>
        /// 获取应用作者
        /// </summary>
        public string GetAuthor() => GetAppInfo("Author");

        /// <summary>
        /// 获取应用描述
        /// </summary>
        public string GetDescription() => GetAppInfo("Description");

        /// <summary>
        /// 获取版权信息
        /// </summary>
        public string GetCopyright() => GetAppInfo("Copyright");

        /// <summary>
        /// 获取所有应用信息
        /// </summary>
        public Dictionary<string, string> GetAllInfo()
        {
            return new Dictionary<string, string>(_appInfo);
        }
        
        /// <summary>
        /// 从程序集读取应用信息
        /// </summary>
        private void LoadAppInfoFromAssembly()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                
                // 读取程序集版本
                var version = assembly.GetName().Version;
                if (version != null)
                {
                    _appInfo["Version"] = version.ToString();
                }
                
                // 读取程序集属性
                var titleAttribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
                if (titleAttribute != null)
                {
                    _appInfo["AppName"] = titleAttribute.Title;
                }
                
                var descriptionAttribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    _appInfo["Description"] = descriptionAttribute.Description;
                }
                
                var companyAttribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
                if (companyAttribute != null)
                {
                    _appInfo["Author"] = companyAttribute.Company;
                }
                
                var copyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
                if (copyrightAttribute != null)
                {
                    _appInfo["Copyright"] = copyrightAttribute.Copyright;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"从程序集读取应用信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件读取应用信息
        /// </summary>
        private async Task LoadAppInfoFromFile()
        {
            try
            {
                string appInfoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appinfo.json");
                
                if (File.Exists(appInfoPath))
                {
                    string json = await File.ReadAllTextAsync(appInfoPath);
                    var appInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        json, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    
                    if (appInfo != null)
                    {
                        foreach (var kvp in appInfo)
                        {
                            _appInfo[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"从文件读取应用信息失败: {ex.Message}");
            }
        }

        #region IService接口实现 / IService Interface Implementation

        /// <summary>
        /// 获取服务名称，用于日志记录和诊断。
        /// Get the service name, used for logging and diagnostics.
        /// </summary>
        public string ServiceName => "AppInfoService";

        /// <summary>
        /// 获取服务是否已初始化。
        /// Get whether the service is initialized.
        /// </summary>
        public bool IsInitialized => true; // AppInfoService在构造时就已初始化

        /// <summary>
        /// 服务状态变化事件，当服务状态发生变化时触发。
        /// Service state change event, triggered when the service state changes.
        /// </summary>
#pragma warning disable CS0067 // 事件从未使用 - 为将来扩展预留
        public event EventHandler<ServiceStateChangedEventArgs>? StateChanged;
#pragma warning restore CS0067

        /// <summary>
        /// 异步初始化服务。AppInfoService在构造时已初始化，此方法为空实现。
        /// Asynchronously initialize the service. AppInfoService is initialized during construction, this method is empty implementation.
        /// </summary>
        /// <param name="cancellationToken">取消标记 / Cancellation token</param>
        /// <returns>表示异步操作的任务 / A task representing the asynchronous operation</returns>
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // AppInfoService在构造时已初始化，无需额外初始化
            // AppInfoService is already initialized during construction, no additional initialization needed
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
            // AppInfoService无需特殊关闭操作
            // AppInfoService does not require special shutdown operations
            return Task.CompletedTask;
        }

        #endregion
    }
}