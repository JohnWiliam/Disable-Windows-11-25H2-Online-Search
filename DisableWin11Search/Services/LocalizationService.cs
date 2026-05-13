using System.Globalization;
using System.Resources;
using Microsoft.Win32;

namespace DisableWin11Search.Services;

public sealed class LocalizationService
{
    private const string SettingsKeyPath = @"Software\DisableWin11Search";
    private const string LanguageValueName = "Language";
    private static readonly string[] SupportedCultures = ["pt-BR", "en-US"];
    private readonly ResourceManager _resourceManager = new("DisableWin11Search.Resources.Strings", typeof(LocalizationService).Assembly);

    private static CultureInfo? s_currentCulture;

    public CultureInfo CurrentCulture => s_currentCulture ?? CultureInfo.GetCultureInfo("en-US");

    public LocalizationService()
    {
        s_currentCulture ??= GetSavedCulture() ?? GetBestSystemCulture();
        ApplyCulture(CurrentCulture.Name, save: false);
    }

    public string GetString(string key)
    {
        var value = _resourceManager.GetString(key, CurrentCulture) ?? $"!{key}!";
        return value.Replace("\\n", Environment.NewLine, StringComparison.Ordinal);
    }

    public void ApplyCulture(string cultureName, bool save = true)
    {
        var normalizedCultureName = SupportedCultures.Contains(cultureName, StringComparer.OrdinalIgnoreCase)
            ? SupportedCultures.First(culture => culture.Equals(cultureName, StringComparison.OrdinalIgnoreCase))
            : "en-US";

        s_currentCulture = CultureInfo.GetCultureInfo(normalizedCultureName);
        CultureInfo.CurrentCulture = CurrentCulture;
        CultureInfo.CurrentUICulture = CurrentCulture;

        if (save)
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
            key?.SetValue(LanguageValueName, normalizedCultureName, RegistryValueKind.String);
        }
    }

    private static CultureInfo? GetSavedCulture()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKeyPath);
            if (key?.GetValue(LanguageValueName) is string cultureName &&
                SupportedCultures.Contains(cultureName, StringComparer.OrdinalIgnoreCase))
            {
                return CultureInfo.GetCultureInfo(cultureName);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static CultureInfo GetBestSystemCulture()
    {
        var systemLanguage = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;

        return systemLanguage.Equals("pt", StringComparison.OrdinalIgnoreCase)
            ? CultureInfo.GetCultureInfo("pt-BR")
            : CultureInfo.GetCultureInfo("en-US");
    }
}
