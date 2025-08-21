// ================================================================
// WinVault
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Author: WinVault Team
// Created: 2024
// Purpose: Localization service for multi-language support
// ================================================================

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using WinVault.Constants;
using Microsoft.Windows.ApplicationModel.Resources; // WinUI 3 新资源API

namespace WinVault.Services
{
    /// <summary>
    /// 本地化服务，提供应用程序多语言支持和国际化功能。
    /// </summary>
    public class LocalizationService
    {
        private readonly ResourceManager _resourceManager;
        private readonly SettingsService _settingsService;
        private readonly LoggingService _logger;

        private CultureInfo _currentCulture;
        private List<LanguageItem> _supportedLanguages;

        /// <summary>
        /// 支持的语言列表 / Supported languages list
        /// </summary>
        public List<LanguageItem> SupportedLanguages => _supportedLanguages;

        /// <summary>
        /// 获取当前语言 / Get current language
        /// </summary>
        public string CurrentLanguage => GetCurrentLanguage();

        #region 单例模式实现

        private static LocalizationService? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取LocalizationService的单例实例
        /// </summary>
        public static LocalizationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LocalizationService();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        /// <summary>
        /// 私有构造函数，初始化本地化服务
        /// </summary>
        private LocalizationService()
        {
            _settingsService = SettingsService.Instance;
            _logger = LoggingService.Instance;

            // WinUI 3 新资源管理器
            _resourceManager = new ResourceManager();

            // 初始化当前文化
            _currentCulture = CultureInfo.CurrentUICulture;

            // 初始化支持的语言列表
            _supportedLanguages = new List<LanguageItem>
            {
                new LanguageItem { LanguageCode = "en-US", DisplayName = "English" },
                new LanguageItem { LanguageCode = "zh-CN", DisplayName = "简体中文" }
            };

            // 恢复用户选择的语言设置
            string savedLanguage = _settingsService.GetSetting<string>(
                AppConstants.SettingsKeys.Language, string.Empty) ?? string.Empty;

            if (!string.IsNullOrEmpty(savedLanguage))
            {
                SetLanguage(savedLanguage);
            }

            _logger.Information("本地化服务初始化完成");
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="key">资源字符串键</param>
        /// <returns>本地化字符串</returns>
        public string GetString(string key)
        {
            try
            {
                // WinUI 3 资源文件结构：Resources/xx
                // 你的字符串应放在 Strings/en-US/Resources.resw 下
                // key 写成 "Resources/xxx"
                var value = _resourceManager.MainResourceMap.GetValue($"Resources/{key}", _resourceManager.CreateResourceContext()).ValueAsString;
                return value ?? key;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取本地化字符串失败: {0}", key);
                return key;
            }
        }

        /// <summary>
        /// 设置应用程序语言
        /// </summary>
        /// <param name="languageCode">语言代码，如zh-CN</param>
        /// <returns>是否设置成功</returns>
        public bool SetLanguage(string languageCode)
        {
            try
            {
                // 验证语言代码是否在支持的语言列表中
                if (!IsSupportedLanguage(languageCode))
                {
                    _logger.Warning("不支持的语言: {0}", languageCode);
                    return false;
                }

                // 更改应用程序语言首选项
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;

                // 保存设置
                _settingsService.SaveSetting(AppConstants.SettingsKeys.Language, languageCode);

                _logger.Information("已切换语言: {0}", languageCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "设置语言失败: {0}", languageCode);
                return false;
            }
        }

        /// <summary>
        /// 切换应用程序语言并重新加载资源
        /// </summary>
        /// <param name="languageCode">语言代码，如zh-CN</param>
        /// <returns>是否切换成功</returns>
        public async Task<bool> SwitchLanguage(string languageCode)
        {
            bool result = SetLanguage(languageCode);
            if (result)
            {
                await ReloadResourcesAsync();
            }
            return result;
        }

        /// <summary>
        /// 获取当前应用程序语言
        /// </summary>
        /// <returns>当前语言代码</returns>
        public string GetCurrentLanguage()
        {
            string language = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;

            // 如果没有设置覆盖语言，则返回系统默认语言
            if (string.IsNullOrEmpty(language))
            {
                language = Windows.Globalization.ApplicationLanguages.Languages[0];
            }

            return language;
        }

        /// <summary>
        /// 检查语言是否被支持
        /// </summary>
        /// <param name="languageCode">语言代码</param>
        /// <returns>是否支持</returns>
        private bool IsSupportedLanguage(string languageCode)
        {
            return _supportedLanguages.Exists(l => l.LanguageCode == languageCode);
        }

        /// <summary>
        /// 重新加载资源
        /// </summary>
        public async Task ReloadResourcesAsync()
        {
            try
            {
                // WinUI 3 仍然无法动态reload所有资源，建议重启应用
                await Task.Delay(10);

                // 通知界面语言已更改
                LanguageChanged?.Invoke(this, CurrentLanguage);

                _logger.Information("本地化资源已重新加载");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "重新加载资源失败");
            }
        }

        /// <summary>
        /// 语言更改事件
        /// </summary>
        public event EventHandler<string>? LanguageChanged;
    }

    /// <summary>
    /// 语言项
    /// </summary>
    public class LanguageItem
    {
        public required string LanguageCode { get; set; }
        public required string DisplayName { get; set; }
    }
}
