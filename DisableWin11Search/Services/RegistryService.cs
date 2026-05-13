using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace DisableWin11Search.Services;

public sealed class RegistryService
{
    private const string RegPolicy = @"Software\Policies\Microsoft\Windows\Explorer";
    private const string RegSearch = @"Software\Microsoft\Windows\CurrentVersion\Search";
    private const string RegWindowsSearchPolicy = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";

    public enum OptimizationStatus
    {
        Optimized,
        NotOptimized,
        Unknown
    }

    public OptimizationStatus CheckSearchBoxSuggestions()
    {
        return CheckDWordValue(Registry.CurrentUser, RegPolicy, "DisableSearchBoxSuggestions", 1);
    }

    public void ApplySearchBoxSuggestions()
    {
        SetDWordValue(Registry.CurrentUser, RegPolicy, "DisableSearchBoxSuggestions", 1);
    }

    public void RevertSearchBoxSuggestions()
    {
        DeleteValue(Registry.CurrentUser, RegPolicy, "DisableSearchBoxSuggestions");
    }

    public OptimizationStatus CheckCloudSearch()
    {
        try
        {
            using var userKey = Registry.CurrentUser.OpenSubKey(RegSearch);
            using var policyKey = Registry.LocalMachine.OpenSubKey(RegWindowsSearchPolicy);

            var hasLegacyUserValue = IsDWordValue(userKey, "DisableCloudSearch", 1);
            var hasPolicyValue = IsDWordValue(policyKey, "AllowCloudSearch", 0);

            return hasLegacyUserValue || hasPolicyValue
                ? OptimizationStatus.Optimized
                : OptimizationStatus.NotOptimized;
        }
        catch
        {
            return OptimizationStatus.Unknown;
        }
    }

    public void ApplyCloudSearch()
    {
        SetDWordValue(Registry.CurrentUser, RegSearch, "DisableCloudSearch", 1);

        // Microsoft documents cloud search as a Windows Search policy.
        // Keep the user value for compatibility with existing installs and add the policy-backed value.
        SetDWordValue(Registry.LocalMachine, RegWindowsSearchPolicy, "AllowCloudSearch", 0);
    }

    public void RevertCloudSearch()
    {
        DeleteValue(Registry.CurrentUser, RegSearch, "DisableCloudSearch");
        DeleteValue(Registry.LocalMachine, RegWindowsSearchPolicy, "AllowCloudSearch");
    }

    public OptimizationStatus CheckBingSearch()
    {
        return CheckDWordValue(Registry.CurrentUser, RegSearch, "BingSearchEnabled", 0);
    }

    public void ApplyBingSearch()
    {
        SetDWordValue(Registry.CurrentUser, RegSearch, "BingSearchEnabled", 0);
    }

    public void RevertBingSearch()
    {
        DeleteValue(Registry.CurrentUser, RegSearch, "BingSearchEnabled");
    }

    public OptimizationStatus CheckWebResults()
    {
        return CheckDWordValue(Registry.LocalMachine, RegWindowsSearchPolicy, "ConnectedSearchUseWeb", 0);
    }

    public void ApplyWebResults()
    {
        SetDWordValue(Registry.LocalMachine, RegWindowsSearchPolicy, "ConnectedSearchUseWeb", 0);
    }

    public void RevertWebResults()
    {
        DeleteValue(Registry.LocalMachine, RegWindowsSearchPolicy, "ConnectedSearchUseWeb");
    }

    public void RestartExplorer()
    {
        foreach (var process in Process.GetProcessesByName("explorer"))
        {
            using (process)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
                catch
                {
                    // Ignore processes that exited between enumeration and Kill, or that Windows denied.
                }
            }
        }

        var explorerPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "explorer.exe");

        if (File.Exists(explorerPath))
        {
            using var startedProcess = Process.Start(new ProcessStartInfo
            {
                FileName = explorerPath,
                UseShellExecute = false
            });
        }
    }

    private static OptimizationStatus CheckDWordValue(RegistryKey rootKey, string subKeyName, string valueName, int optimizedValue)
    {
        try
        {
            using var key = rootKey.OpenSubKey(subKeyName);
            return IsDWordValue(key, valueName, optimizedValue)
                ? OptimizationStatus.Optimized
                : OptimizationStatus.NotOptimized;
        }
        catch
        {
            return OptimizationStatus.Unknown;
        }
    }

    private static bool IsDWordValue(RegistryKey? key, string valueName, int expectedValue)
    {
        return key?.GetValue(valueName) is int value && value == expectedValue;
    }

    private static void SetDWordValue(RegistryKey rootKey, string subKeyName, string valueName, int value)
    {
        using var key = rootKey.CreateSubKey(subKeyName, writable: true)
            ?? throw new InvalidOperationException($"Unable to create or open registry key: {rootKey.Name}\\{subKeyName}");

        key.SetValue(valueName, value, RegistryValueKind.DWord);
    }

    private static void DeleteValue(RegistryKey rootKey, string subKeyName, string valueName)
    {
        using var key = rootKey.OpenSubKey(subKeyName, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }
}
