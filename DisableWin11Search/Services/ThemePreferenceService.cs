using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace DisableWin11Search.Services;

public enum ThemePreference
{
    System,
    Light,
    Dark
}

public sealed class ThemePreferenceService
{
    private const string SettingsKeyPath = @"Software\DisableWin11Search";
    private const string ThemeValueName = "Theme";

    public ThemePreference CurrentPreference { get; private set; }

    public ThemePreferenceService()
    {
        CurrentPreference = GetSavedPreference() ?? ThemePreference.System;
    }

    public void Apply(Window window, ThemePreference preference, bool save = true)
    {
        CurrentPreference = preference;

        if (preference == ThemePreference.System)
        {
            ApplicationThemeManager.ApplySystemTheme(updateAccent: true);
            SystemThemeWatcher.Watch(window, WindowBackdropType.Mica, updateAccents: true);
        }
        else
        {
            SystemThemeWatcher.UnWatch(window);
            ApplicationThemeManager.Apply(
                preference == ThemePreference.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light,
                WindowBackdropType.Mica,
                updateAccent: true);
        }

        if (save)
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
            key?.SetValue(ThemeValueName, preference.ToString(), RegistryValueKind.String);
        }
    }

    private static ThemePreference? GetSavedPreference()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKeyPath);
            if (key?.GetValue(ThemeValueName) is string preferenceName &&
                Enum.TryParse(preferenceName, ignoreCase: true, out ThemePreference preference))
            {
                return preference;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
