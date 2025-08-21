using Microsoft.UI.Xaml;
using System;
using WinVault.Services;

namespace WinVault.Services
{
    public sealed class ThemeService
    {
        private static readonly Lazy<ThemeService> _lazy = new(() => new ThemeService());
        public static ThemeService Instance => _lazy.Value;

        private ThemeService() { }

        public ElementTheme GetThemeFromSettings()
        {
            var settings = SettingsService.Instance;
            var theme = settings.GetSetting<string>("AppTheme", "Default") ?? "Default";
            return theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }

        public void SaveTheme(ElementTheme theme)
        {
            var settings = SettingsService.Instance;
            string value = theme switch
            {
                ElementTheme.Light => "Light",
                ElementTheme.Dark => "Dark",
                _ => "Default"
            };
            settings.SaveSetting("AppTheme", value);
        }

        public void ApplyThemeToWindow(Window? window, ElementTheme theme)
        {
            if (window?.Content is FrameworkElement root)
            {
                root.RequestedTheme = theme;
            }
        }

        public void ApplyThemeFromSettings(Window? window)
        {
            ApplyThemeToWindow(window, GetThemeFromSettings());
        }
    }
} 