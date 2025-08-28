// ================================================================
// WinVault - Application Constants and Configuration
// Copyright (c) 2024 WinVault Team. All rights reserved.
// Licensed under the GPL-3.0 License.
// Author: WinVault Development Team
// Created: 2024-12-20
// Last Modified: 2024-12-20
// Version: 2.0.0
// Purpose: Centralized application constants, configuration values,
//          and magic numbers for enterprise-grade maintainability
// ================================================================

#nullable enable
namespace WinVault.Constants
{
    /// <summary>
    /// 企业级应用程序常量定义中心，提供类型安全的配置值管理。
    /// 包含应用程序运行所需的所有常量、配置参数和标准化值。
    /// 采用分组设计，便于维护和扩展，支持编译时优化。
    ///
    /// Enterprise-grade application constants definition center providing type-safe configuration value management.
    /// Contains all constants, configuration parameters, and standardized values required for application operation.
    /// Uses grouped design for easy maintenance and extension, supporting compile-time optimization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>常量分类 / Constant Categories:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>应用程序元数据 / Application Metadata</term>
    /// <description>应用名称、版本、作者、许可证等基本信息 / Application name, version, author, license, and other basic information</description>
    /// </item>
    /// <item>
    /// <term>UI布局常量 / UI Layout Constants</term>
    /// <description>窗口尺寸、边距、间距、字体大小等界面参数 / Window dimensions, margins, spacing, font sizes, and other interface parameters</description>
    /// </item>
    /// <item>
    /// <term>系统路径配置 / System Path Configuration</term>
    /// <description>文件存储路径、缓存目录、日志位置等路径定义 / File storage paths, cache directories, log locations, and other path definitions</description>
    /// </item>
    /// <item>
    /// <term>导航路由映射 / Navigation Route Mapping</term>
    /// <description>页面导航标识符和路由配置 / Page navigation identifiers and routing configuration</description>
    /// </item>
    /// <item>
    /// <term>设置键值对 / Settings Key-Value Pairs</term>
    /// <description>用户设置和应用程序配置的键名定义 / Key name definitions for user settings and application configuration</description>
    /// </item>
    /// <item>
    /// <term>网络和API配置 / Network and API Configuration</term>
    /// <description>API端点、超时值、重试次数等网络相关常量 / API endpoints, timeout values, retry counts, and other network-related constants</description>
    /// </item>
    /// </list>
    ///
    /// <para>
    /// <strong>设计原则 / Design Principles:</strong>
    /// </para>
    /// <list type="number">
    /// <item><strong>类型安全:</strong> 使用强类型常量，避免字符串魔法值 / Use strongly-typed constants, avoid string magic values</item>
    /// <item><strong>分组管理:</strong> 相关常量组织在嵌套类中，提高可读性 / Related constants organized in nested classes for improved readability</item>
    /// <item><strong>命名规范:</strong> 遵循.NET命名约定，使用描述性名称 / Follow .NET naming conventions, use descriptive names</item>
    /// <item><strong>文档完整:</strong> 每个常量都有详细的中英双语注释 / Each constant has detailed bilingual documentation</item>
    /// <item><strong>版本兼容:</strong> 常量变更遵循语义版本控制原则 / Constant changes follow semantic versioning principles</item>
    /// </list>
    ///
    /// <para>
    /// <strong>使用示例 / Usage Examples:</strong>
    /// </para>
    /// <code>
    /// // 应用程序信息 / Application information
    /// string appName = AppConstants.AppName;
    /// string version = AppConstants.BuildVersion;
    ///
    /// // UI布局 / UI layout
    /// int minWidth = AppConstants.MinWindowWidth;
    /// int minHeight = AppConstants.MinWindowHeight;
    ///
    /// // 导航路由 / Navigation routes
    /// string homePage = AppConstants.NavigationRoutes.Home;
    /// string settingsPage = AppConstants.NavigationRoutes.Settings;
    ///
    /// // 设置键 / Settings keys
    /// string themeKey = AppConstants.SettingsKeys.Theme;
    /// string languageKey = AppConstants.SettingsKeys.Language;
    ///
    /// // 存储路径 / Storage paths
    /// string appDataFolder = AppConstants.StoragePaths.AppDataFolderName;
    /// string logsFolder = AppConstants.StoragePaths.LogsFolderName;
    /// </code>
    ///
    /// <para>
    /// <strong>维护指南 / Maintenance Guidelines:</strong><br/>
    /// 1. 新增常量时，选择合适的分组或创建新的嵌套类 / When adding new constants, choose appropriate grouping or create new nested classes<br/>
    /// 2. 修改现有常量时，考虑向后兼容性影响 / When modifying existing constants, consider backward compatibility impact<br/>
    /// 3. 删除常量前，确保没有代码引用 / Before deleting constants, ensure no code references exist<br/>
    /// 4. 定期审查和清理未使用的常量 / Regularly review and clean up unused constants
    /// </para>
    /// </remarks>
    public static class AppConstants
    {
        /// <summary>
        /// 应用程序名称
        /// </summary>
        public const string AppName = "WinVault";

        /// <summary>
        /// 最小窗口宽度
        /// </summary>
        public const int MinWindowWidth = 800;

        /// <summary>
        /// 最小窗口高度
        /// </summary>
        public const int MinWindowHeight = 450;

        /// <summary>
        /// 默认窗口宽度
        /// </summary>
        public const int DefaultWindowWidth = 1000;

        /// <summary>
        /// 默认窗口高度
        /// </summary>
        public const int DefaultWindowHeight = 650;

        /// <summary>
        /// 设置文件名
        /// </summary>
        public const string SettingsFileName = "app_settings.json";

        /// <summary>
        /// 应用信息文件名
        /// </summary>
        public const string AppInfoFileName = "appinfo.json";

        public const string LogPath = "./Logs/";
        
        /// <summary>
        /// 应用版本号
        /// </summary>
        public const string AppVersion = "1.1.0";
        
        /// <summary>
        /// 应用构建版本号
        /// </summary>
        public const string BuildVersion = "1.1.0.20240629";
        
        /// <summary>
        /// 应用程序GitHub仓库地址
        /// </summary>
        public const string GitHubRepo = "https://github.com/Fantasy-XY808/WinVault";
        
        /// <summary>
        /// 导航路由常量
        /// </summary>
        public static class NavigationRoutes
        {
            public const string Home = "home";
            public const string Hardware = "hardware";
            public const string Services = "services";
            public const string CommandQuery = "commandquery";
            public const string QuickSettings = "quicksettings";
            public const string ExeTools = "exetools";
            public const string About = "about";
            public const string Settings = "settings";
        }
        
        /// <summary>
        /// 设置键常量
        /// </summary>
        public static class SettingsKeys
        {
            public const string Theme = "Theme";
            public const string NavViewMode = "NavViewPaneDisplayMode";
            public const string Language = "Language";
            public const string AutoStart = "AutoStart";
            public const string LastOpenedPage = "LastOpenedPage";
            public const string LogLevel = "LogLevel";
            public const string EnableAnalytics = "EnableAnalytics";
            public const string CheckUpdatesAutomatically = "CheckUpdatesAutomatically";
            public const string LastUpdateCheck = "LastUpdateCheck";
            public const string WindowWidth = "WindowWidth";
            public const string WindowHeight = "WindowHeight";
            // 新增：窗口位置与启动位置策略
            public const string WindowX = "WindowX";
            public const string WindowY = "WindowY";
            // Center 或 Last
            public const string WindowStartPosition = "WindowStartPosition";
            // 是否展开侧边栏（可与 NavViewMode 配合使用）
            public const string NavPaneExpanded = "NavPaneExpanded";
        }
        
        /// <summary>
        /// 本地存储路径常量
        /// </summary>
        public static class StoragePaths
        {
            /// <summary>
            /// 应用数据根目录名
            /// </summary>
            public const string AppDataFolderName = "WinVault";
            
            /// <summary>
            /// 日志目录名
            /// </summary>
            public const string LogsFolderName = "Logs";
            
            /// <summary>
            /// 缓存目录名
            /// </summary>
            public const string CacheFolderName = "Cache";
            
            /// <summary>
            /// 临时文件目录名
            /// </summary>
            public const string TempFolderName = "Temp";
            
            /// <summary>
            /// 数据导出目录名
            /// </summary>
            public const string ExportsFolderName = "Exports";
        }
    }
} 
