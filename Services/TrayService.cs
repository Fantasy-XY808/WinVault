using System;
using H.NotifyIcon;
using Microsoft.UI.Xaml;

namespace WinVault.Services
{
    public sealed class TrayService
    {
        private static readonly Lazy<TrayService> _lazy = new(() => new TrayService());
        public static TrayService Instance => _lazy.Value;

        private TaskbarIcon? _icon;
        private Window? _window;

        private TrayService() { }

        public void Initialize(Window window)
        {
            _window = window;
            if (_icon != null) return;

            _icon = new TaskbarIcon
            {
                ToolTipText = "WinVault"
            };
            // 左键点击恢复窗口
            _icon.LeftClickCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(() => ShowMainWindow());
        }

        public void ShowMainWindow()
        {
            if (_window == null) return;
            _window.AppWindow?.Show();
            _window.Activate();
        }

        public void HideToTray()
        {
            _window?.AppWindow?.Hide();
        }

        private void ExitApplication()
        {
            try { _icon?.Dispose(); } catch { }
            Environment.Exit(0);
        }
    }
} 