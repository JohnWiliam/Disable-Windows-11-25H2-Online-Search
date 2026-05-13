using System.Globalization;
using System.Resources;
using Microsoft.Win32;

namespace DisableWin11Search.Services;

public sealed class LocalizationService
{
    private const string SettingsKeyPath = @"Software\DisableWin11Search";
    private const string LanguageValueName = "Language";
    private static readonly string[] SupportedCultures = ["pt-BR", "en-US"];
    private static readonly IReadOnlyDictionary<string, string> SupportedNeutralCultures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["pt"] = "pt-BR",
        ["en"] = "en-US"
    };
    private readonly ResourceManager _resourceManager = new("DisableWin11Search.Resources.Strings", typeof(LocalizationService).Assembly);

    public CultureInfo CurrentCulture { get; private set; }

    public LocalizationService()
    {
        CurrentCulture = GetSavedCulture() ?? GetBestSystemCulture();
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

        CurrentCulture = CultureInfo.GetCultureInfo(normalizedCultureName);
        CultureInfo.CurrentCulture = CurrentCulture;
        CultureInfo.CurrentUICulture = CurrentCulture;

        if (save)
        {
            SaveCulture(normalizedCultureName);
        }
    }

    private static void SaveCulture(string cultureName)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
            key?.SetValue(LanguageValueName, cultureName, RegistryValueKind.String);
        }
        catch
        {
            // The UI can keep using the selected language even if Windows blocks saving preferences.
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
        var systemCulture = CultureInfo.InstalledUICulture;
        if (SupportedCultures.Contains(systemCulture.Name, StringComparer.OrdinalIgnoreCase))
        {
            return CultureInfo.GetCultureInfo(systemCulture.Name);
        }

        return SupportedNeutralCultures.TryGetValue(systemCulture.TwoLetterISOLanguageName, out var supportedCulture)
            ? CultureInfo.GetCultureInfo(supportedCulture)
            : CultureInfo.GetCultureInfo("en-US");
    }
}
