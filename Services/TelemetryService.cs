using System;
using System.IO;
using WinVault.Constants;

namespace WinVault.Services
{
    public sealed class TelemetryService
    {
        private static readonly Lazy<TelemetryService> _lazy = new(() => new TelemetryService());
        public static TelemetryService Instance => _lazy.Value;

        private readonly string _filePath;
        private bool _enabled;

        private TelemetryService()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppConstants.StoragePaths.AppDataFolderName);
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "telemetry.log");
            _enabled = SettingsService.Instance.GetSetting<bool>(AppConstants.SettingsKeys.EnableAnalytics, true);
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            SettingsService.Instance.SaveSetting(AppConstants.SettingsKeys.EnableAnalytics, enabled);
            Track("telemetry_toggle", enabled ? "on" : "off");
        }

        public void Track(string eventName, string data)
        {
            if (!_enabled) return;
            try
            {
                File.AppendAllText(_filePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{eventName}\t{data}{Environment.NewLine}");
            }
            catch { }
        }
    }
} 