using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using WinVault.Services;
using WinVault.Constants;
using Serilog.Events;

namespace WinVault.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsService? _settingsService;

        public SettingsPage()
        {
            try
            {
                this.InitializeComponent();
                this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
                _settingsService = SettingsService.Instance;
                Loaded += SettingsPage_Loaded;
            }
            catch (Exception ex)
            {
                try { LoggingService.Instance.Error(ex, "SettingsPage ctor failed"); } catch { }

                // 构造最小可用的降级UI，避免应用直接崩溃
                var panel = new StackPanel { Spacing = 12, Padding = new Thickness(24) };
                panel.Children.Add(new TextBlock
                {
                    Text = "设置页加载失败",
                    FontSize = 20,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                });
                panel.Children.Add(new TextBlock
                {
                    Text = ex.Message,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.8
                });
                var back = new Button { Content = "返回" };
                back.Click += (s, e) => { if (this.Frame?.CanGoBack == true) this.Frame.GoBack(); };
                panel.Children.Add(back);
                this.Content = panel;
            }
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // 主题
                var theme = _settingsService?.GetSetting("AppTheme", "Default");
                var themeStr = theme as string ?? "Default";
                switch (themeStr)
                {
                    case "Light": 
                        if (ThemeLight != null) ThemeLight.IsChecked = true; 
                        break;
                    case "Dark": 
                        if (ThemeDark != null) ThemeDark.IsChecked = true; 
                        break;
                    default: 
                        if (ThemeSystem != null) ThemeSystem.IsChecked = true; 
                        break;
                }

                var useAdminObj = _settingsService.GetSetting("UseAdminPrivileges", false);
                if (AdminToggle != null)
                    AdminToggle.IsOn = useAdminObj is bool useAdminValue && useAdminValue;

                var startWithWindows = false;
                try { startWithWindows = StartupService.Instance.IsAutoStartEnabled(); } catch { }
                if (StartupToggle != null)
                    StartupToggle.IsOn = startWithWindows;

                var startMinimizedObj = _settingsService.GetSetting("StartMinimized", false);
                if (MinStartToggle != null)
                    MinStartToggle.IsOn = startMinimizedObj is bool startMinimizedValue && startMinimizedValue;

                // 高级开关
                var telemetry = _settingsService.GetSetting(AppConstants.SettingsKeys.EnableAnalytics, true);
                if (TelemetryToggle != null)
                    TelemetryToggle.IsOn = telemetry is bool tv && tv;
                var logLevel = _settingsService.GetSetting<string>(AppConstants.SettingsKeys.LogLevel, "Information") ?? "Information";
                if (DebugModeToggle != null)
                    DebugModeToggle.IsOn = string.Equals(logLevel, "Debug", StringComparison.OrdinalIgnoreCase);

                // 语言
                var lang = _settingsService.GetSetting<string>(AppConstants.SettingsKeys.Language, "system") ?? "system";
                if (LanguageCombo != null)
                {
                    foreach (var item in LanguageCombo.Items.OfType<ComboBoxItem>())
                    {
                        if ((item.Tag?.ToString() ?? "") == lang)
                        {
                            LanguageCombo.SelectedItem = item;
                            break;
                        }
                    }
                }

                // 侧边栏展开
                var navExpandObj = _settingsService.GetSetting(AppConstants.SettingsKeys.NavPaneExpanded, false);
                if (NavPaneToggle != null)
                    NavPaneToggle.IsOn = navExpandObj is bool nv && nv;

                // 启动位置策略
                var startPos = _settingsService.GetSetting<string>(AppConstants.SettingsKeys.WindowStartPosition, "Center") ?? "Center";
                if (StartPosCombo != null)
                {
                    foreach (var item in StartPosCombo.Items.OfType<ComboBoxItem>())
                    {
                        if ((item.Tag?.ToString() ?? "Center") == startPos)
                        {
                            StartPosCombo.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 捕获加载异常，避免页面崩溃
                try { LoggingService.Instance.Error(ex, "SettingsPage LoadSettings failed"); } catch { }
                System.Diagnostics.Debug.WriteLine($"设置加载失败: {ex.Message}");
            }
        }

        private void Theme_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    string themeStr = radioButton.Tag?.ToString() ?? "Default";
                    var theme = themeStr == "Light" ? ElementTheme.Light : themeStr == "Dark" ? ElementTheme.Dark : ElementTheme.Default;
                    ThemeService.Instance.SaveTheme(theme);
                    ThemeService.Instance.ApplyThemeToWindow(App.CurrentWindow ?? App.MainWindow, theme);
                    this.RequestedTheme = theme;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"主题切换失败: {ex.Message}");
            }
        }

        private void AdminToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AdminToggle != null)
                {
                    bool isOn = AdminToggle.IsOn;
                    _settingsService?.SaveSetting("UseAdminPrivileges", isOn);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"管理员权限设置失败: {ex.Message}");
            }
        }

        private void StartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StartupToggle != null && MinStartToggle != null)
                {
                    bool isOn = StartupToggle.IsOn;
                    bool startMin = MinStartToggle.IsOn;
                    try { StartupService.Instance.SetAutoStart(isOn, startMin); } catch { }
                    MinStartToggle.IsEnabled = isOn;
                }
            }
            catch (Exception ex)
            {
                try { LoggingService.Instance.Error(ex, "StartupToggle_Toggled failed"); } catch { }
                System.Diagnostics.Debug.WriteLine($"开机自启动设置失败: {ex.Message}");
            }
        }

        private void MinStartToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MinStartToggle != null && StartupToggle != null)
                {
                    bool isOn = MinStartToggle.IsOn;
                    _settingsService?.SaveSetting("StartMinimized", isOn);
                    if (StartupToggle.IsOn) { try { StartupService.Instance.SetAutoStart(true, isOn); } catch { } }
                }
            }
            catch (Exception ex)
            {
                try { LoggingService.Instance.Error(ex, "MinStartToggle_Toggled failed"); } catch { }
                System.Diagnostics.Debug.WriteLine($"最小化启动设置失败: {ex.Message}");
            }
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog dialog = new ContentDialog { Title = "检查更新", Content = "当前已是最新版本: v1.0.0", PrimaryButtonText = "确定", XamlRoot = this.XamlRoot };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查更新失败: {ex.Message}");
            }
        }

        private async void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog dialog = new ContentDialog { Title = "重置所有设置", Content = "确定要将所有设置恢复到默认值吗？", PrimaryButtonText = "确定", CloseButtonText = "取消", XamlRoot = this.XamlRoot };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    _settingsService?.ResetSettings();
                    LoadSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"重置设置失败: {ex.Message}");
            }
        }

        private void TelemetryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try 
            { 
                if (TelemetryToggle != null)
                {
                    TelemetryService.Instance.SetEnabled(TelemetryToggle.IsOn); 
                }
            } 
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"遥测设置失败: {ex.Message}");
            }
        }

        private void DebugModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DebugModeToggle != null)
                {
                    var level = DebugModeToggle.IsOn ? LogEventLevel.Debug : LogEventLevel.Information;
                    LoggingService.Instance.SetMinimumLevel(level);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"调试模式设置失败: {ex.Message}");
            }
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (LanguageCombo?.SelectedItem is ComboBoxItem item)
                {
                    var tag = item.Tag?.ToString() ?? "system";
                    if (tag == "system")
                    {
                        _settingsService?.SaveSetting(AppConstants.SettingsKeys.Language, "system");
                    }
                    else
                    {
                        _settingsService?.SaveSetting(AppConstants.SettingsKeys.Language, tag);
                    }
                    // 立即应用语言
                    var langToApply = tag == "system" ? LocalizationService.Instance.GetCurrentLanguage() : tag;
                    if (langToApply != null)
                    {
                        _ = LocalizationService.Instance.SwitchLanguage(langToApply);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"语言设置失败: {ex.Message}");
            }
        }

        private void NavPaneToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavPaneToggle != null)
                {
                    _settingsService?.SaveSetting(AppConstants.SettingsKeys.NavPaneExpanded, NavPaneToggle.IsOn);
                    // 立即作用到主窗口
                    if (App.MainWindow is MainWindow mw && mw.NavViewControl != null)
                    {
                        mw.NavViewControl.IsPaneOpen = NavPaneToggle.IsOn;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"侧边栏设置失败: {ex.Message}");
            }
        }

        private void StartPosCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (StartPosCombo?.SelectedItem is ComboBoxItem item)
                {
                    var tag = item.Tag?.ToString() ?? "Center";
                    _settingsService?.SaveSetting(AppConstants.SettingsKeys.WindowStartPosition, tag);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"启动位置设置失败: {ex.Message}");
            }
        }

        private async void ShowMessage(string message)
        {
            try
            {
                var dialog = new ContentDialog { Title = "提示", Content = message, CloseButtonText = "确定", XamlRoot = this.XamlRoot };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示消息失败: {ex.Message}");
            }
        }
    }
}