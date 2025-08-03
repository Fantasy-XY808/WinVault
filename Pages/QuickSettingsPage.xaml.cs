using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace WinVault.Pages
{
    public sealed partial class QuickSettingsPage : Page
    {
        public QuickSettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        #region 显示设置

        private void OpenDisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:display");
        }

        private void ToggleNightLight_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:nightlight");
        }

        private void OpenProjectSettings_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("DisplaySwitch.exe");
        }

        #endregion

        #region 声音设置

        private void OpenSoundSettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:sound");
        }

        private void OpenVolumeMixer_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("SndVol.exe");
        }

        private void OpenRecordingDevices_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("mmsys.cpl,,1");
        }

        #endregion

        #region 网络设置

        private void OpenNetworkSettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:network");
        }

        private void OpenWiFiSettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:network-wifi");
        }

        private void RunNetworkDiagnostic_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("msdt.exe -id NetworkDiagnosticsWeb");
        }

        #endregion

        #region 系统监控

        private void OpenTaskManager_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("taskmgr.exe");
        }

        private void OpenResourceMonitor_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("resmon.exe");
        }

        private void OpenPerformanceMonitor_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("perfmon.exe");
        }

        #endregion

        #region 磁盘管理

        private void OpenDiskManagement_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("diskmgmt.msc");
        }

        private void OpenDiskCleanup_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("cleanmgr.exe");
        }

        private void OpenStorageSettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:storagesense");
        }

        #endregion

        #region 安全和更新

        private void OpenWindowsSecurity_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:windowsdefender");
        }

        private void OpenWindowsUpdate_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:windowsupdate");
        }

        private void OpenFirewallSettings_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("firewall.cpl");
        }

        #endregion

        #region 个性化

        private void OpenPersonalizationSettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:personalization");
        }

        private void OpenThemeSettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:themes");
        }

        private void OpenWallpaperSettings_Click(object sender, RoutedEventArgs e)
        {
            LaunchSystemSettings("ms-settings:personalization-background");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 启动系统设置
        /// </summary>
        private void LaunchSystemSettings(string settingsUri)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = settingsUri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"无法打开设置页面：{ex.Message}");
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        private void ExecuteCommand(string command)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"执行命令失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        private async void ShowErrorMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "错误",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        #endregion
    }
}