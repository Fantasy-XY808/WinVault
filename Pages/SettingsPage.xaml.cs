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

namespace WinVault.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsService _settingsService;

        public SettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled; // 提升页面切换流畅度

            // 获取设置服务
            _settingsService = SettingsService.Instance;

            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            // 加载主题设置
            var theme = _settingsService.GetSetting("AppTheme", "Default");
            if (theme is string themeStr)
            {
                switch (themeStr)
                {
                    case "Light":
                        ThemeLight.IsChecked = true;
                        break;
                    case "Dark":
                        ThemeDark.IsChecked = true;
                        break;
                    case "Default":
                        ThemeSystem.IsChecked = true;
                        break;
                    default:
                        ThemeSystem.IsChecked = true;
                        break;
                }
            }

            // 加载管理员权限设置
            var useAdmin = _settingsService.GetSetting("UseAdminPrivileges", false);
            if (useAdmin is bool useAdminValue)
            {
                AdminToggle.IsOn = useAdminValue;
            }

            // 加载开机自启动设置
            var startWithWindows = _settingsService.GetSetting("StartWithWindows", false);
            if (startWithWindows is bool startWithWindowsValue)
            {
                StartupToggle.IsOn = startWithWindowsValue;
            }

            // 加载最小化启动设置
            var startMinimized = _settingsService.GetSetting("StartMinimized", false);
            if (startMinimized is bool startMinimizedValue)
            {
                MinStartToggle.IsOn = startMinimizedValue;
            }
        }

        private void Theme_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                string theme = radioButton.Tag?.ToString() ?? "Default";
                _settingsService.SaveSetting("AppTheme", theme);

                // 应用主题设置
                ElementTheme elementTheme = ElementTheme.Default;
                
                if (theme == "Light")
                {
                    elementTheme = ElementTheme.Light;
                }
                else if (theme == "Dark")
                {
                    elementTheme = ElementTheme.Dark;
                }
                
                // 设置根页面的主题
                if (App.CurrentWindow?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = elementTheme;
                }
                
                // 设置当前页面的主题
                this.RequestedTheme = elementTheme;
            }
        }

        private void AdminToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool isOn = AdminToggle.IsOn;
            _settingsService.SaveSetting("UseAdminPrivileges", isOn);
        }
        
        private void StartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool isOn = StartupToggle.IsOn;
            _settingsService.SaveSetting("StartWithWindows", isOn);
            
            // 如果关闭了开机自启动，同时也禁用最小化启动选项
            MinStartToggle.IsEnabled = isOn;
        }
        
        private void MinStartToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool isOn = MinStartToggle.IsOn;
            _settingsService.SaveSetting("StartMinimized", isOn);
        }
        
        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "检查更新",
                Content = "当前已是最新版本: v1.0.0",
                PrimaryButtonText = "确定",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        
        private async void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "重置所有设置",
                Content = "确定要将所有设置恢复到默认状态吗？",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                _settingsService.ResetSettings();
                LoadSettings();
            }
        }

        /// <summary>
        /// 管理收藏按钮点击
        /// </summary>
        private void ManageFavorites_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("收藏管理功能即将推出！");
        }

        /// <summary>
        /// 导出收藏按钮点击
        /// </summary>
        private void ExportFavorites_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("导出收藏功能即将推出！");
        }

        /// <summary>
        /// 导入收藏按钮点击
        /// </summary>
        private void ImportFavorites_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("导入收藏功能即将推出！");
        }

        /// <summary>
        /// 清空对话历史按钮点击
        /// </summary>
        private void ClearChatHistory_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("清空对话历史功能即将推出！");
        }

        /// <summary>
        /// 打开数据文件夹按钮点击
        /// </summary>
        private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinVault");
                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                System.Diagnostics.Process.Start("explorer.exe", dataPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"打开数据文件夹失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 打开用户手册按钮点击
        /// </summary>
        private void OpenUserManual_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("用户手册功能即将推出！");
        }

        /// <summary>
        /// 反馈问题按钮点击
        /// </summary>
        private void ReportIssue_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("问题反馈功能即将推出！");
        }



        private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}