using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace DisableWin11Search.Services;

public enum AppThemePreference
{
    System,
    Light,
    Dark
}

public sealed class ThemeService : IDisposable
{
    private const string SettingsKeyPath = @"Software\DisableWin11Search";
    private const string ThemeValueName = "Theme";
    private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValueName = "AppsUseLightTheme";

    private static readonly IReadOnlyDictionary<string, AppThemePreference> ThemePreferences =
        new Dictionary<string, AppThemePreference>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(AppThemePreference.System)] = AppThemePreference.System,
            [nameof(AppThemePreference.Light)] = AppThemePreference.Light,
            [nameof(AppThemePreference.Dark)] = AppThemePreference.Dark
        };

    private bool _disposed;

    public AppThemePreference CurrentPreference { get; private set; }

    public event EventHandler? ThemeApplied;

    public ThemeService()
    {
        CurrentPreference = GetSavedThemePreference();
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }

    public void Apply(Window window)
    {
        ThrowIfDisposed();

        var resolvedTheme = ResolveTheme(CurrentPreference);
        ApplicationThemeManager.Apply(resolvedTheme);
        ApplyWindowBrushes(window, resolvedTheme);
        ThemeApplied?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeTheme(AppThemePreference preference, Window window)
    {
        ThrowIfDisposed();

        CurrentPreference = preference;
        SaveThemePreference(preference);
        Apply(window);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        ThemeApplied = null;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (_disposed ||
            CurrentPreference != AppThemePreference.System ||
            e.Category is not (UserPreferenceCategory.General or UserPreferenceCategory.VisualStyle))
        {
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            return;
        }

        dispatcher.BeginInvoke((Action)(() =>
        {
            if (!_disposed && Application.Current?.MainWindow is Window window)
            {
                Apply(window);
            }
        }));
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static ApplicationTheme ResolveTheme(AppThemePreference preference)
    {
        return preference switch
        {
            AppThemePreference.Light => ApplicationTheme.Light,
            AppThemePreference.Dark => ApplicationTheme.Dark,
            _ => IsSystemLightTheme() ? ApplicationTheme.Light : ApplicationTheme.Dark
        };
    }

    private static void ApplyWindowBrushes(Window window, ApplicationTheme theme)
    {
        var isDark = theme == ApplicationTheme.Dark;
        window.Resources["AppBackdropTintBrush"] = CreateBrush(isDark ? Color.FromArgb(0x40, 0x00, 0x00, 0x00) : Color.FromArgb(0x40, 0xff, 0xff, 0xff));
        window.Resources["AppFooterBrush"] = CreateBrush(isDark ? Color.FromArgb(0x99, 0x1f, 0x1f, 0x1f) : Color.FromArgb(0x99, 0xff, 0xff, 0xff));
        window.Resources["AppOverlayBrush"] = CreateBrush(isDark ? Color.FromArgb(0x99, 0x00, 0x00, 0x00) : Color.FromArgb(0x66, 0x00, 0x00, 0x00));
        window.Resources["AppDialogSurfaceBrush"] = CreateBrush(isDark ? Color.FromArgb(0xf2, 0x20, 0x20, 0x20) : Color.FromArgb(0xf2, 0xff, 0xff, 0xff));
        window.Resources["StatusSuccessBrush"] = CreateBrush(isDark ? Color.FromRgb(0x6c, 0xd9, 0x70) : Color.FromRgb(0x0f, 0x7b, 0x0f));
        window.Resources["StatusDangerBrush"] = CreateBrush(isDark ? Color.FromRgb(0xff, 0x99, 0x8f) : Color.FromRgb(0xc4, 0x2b, 0x1c));
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static AppThemePreference GetSavedThemePreference()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKeyPath);
            if (key?.GetValue(ThemeValueName) is string themeName && ThemePreferences.TryGetValue(themeName, out var theme))
            {
                return theme;
            }
        }
        catch
        {
            return AppThemePreference.System;
        }

        return AppThemePreference.System;
    }

    private static void SaveThemePreference(AppThemePreference preference)
    {
        using var key = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
        key?.SetValue(ThemeValueName, preference.ToString(), RegistryValueKind.String);
    }

    private static bool IsSystemLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);
            return key?.GetValue(AppsUseLightThemeValueName) is not int value || value != 0;
        }
        catch
        {
            return true;
        }
    }
}
