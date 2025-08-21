using Microsoft.Win32;
using System;
using System.IO;

namespace WinVault.Services
{
    public sealed class StartupService
    {
        private const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string AppName = "WinVault";

        private static readonly Lazy<StartupService> _lazy = new(() => new StartupService());
        public static StartupService Instance => _lazy.Value;
        private StartupService() { }

        public string GetExecutablePath()
        {
            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? AppContext.BaseDirectory;
            }
            catch
            {
                return AppContext.BaseDirectory;
            }
        }

        public bool IsAutoStartEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            var value = key?.GetValue(AppName) as string;
            var exe = GetExecutablePath();
            return !string.IsNullOrWhiteSpace(value) && value.Contains(Path.GetFileName(exe), StringComparison.OrdinalIgnoreCase);
        }

        public void SetAutoStart(bool enabled, bool startMinimized)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true) ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, true);
            if (enabled)
            {
                string exe = GetExecutablePath();
                string args = startMinimized ? " --minimized" : string.Empty;
                key.SetValue(AppName, $"\"{exe}\"{args}");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
    }
} 