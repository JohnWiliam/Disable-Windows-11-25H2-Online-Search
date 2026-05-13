using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace DisableWin11Search.Services;

public class RegistryService
{
    // Registry paths
    private const string RegPolicy = @"Software\Policies\Microsoft\Windows\Explorer";
    private const string RegSearch = @"Software\Microsoft\Windows\CurrentVersion\Search";
    private const string RegWindowsSearchPolicy = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";

    public enum OptimizationStatus
    {
        Optimized,
        NotOptimized,
        Unknown
    }

    // 1. DisableSearchBoxSuggestions (Policy)
    // Check: Value == 1 (Optimized)
    public OptimizationStatus CheckSearchBoxSuggestions()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPolicy);
            if (key?.GetValue("DisableSearchBoxSuggestions") is int iVal && iVal == 1)
            {
                return OptimizationStatus.Optimized;
            }
            return OptimizationStatus.NotOptimized;
        }
        catch
        {
            return OptimizationStatus.Unknown;
        }
    }

    public void ApplySearchBoxSuggestions()
    {
        using var key = CreateCurrentUserSubKey(RegPolicy);
        key.SetValue("DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
    }

    public void RevertSearchBoxSuggestions()
    {
        DeleteCurrentUserValue(RegPolicy, "DisableSearchBoxSuggestions");
    }

    // 2. DisableCloudSearch
    // Check: Value == 1 (Optimized)
    public OptimizationStatus CheckCloudSearch()
    {
        try
        {
            using var userKey = Registry.CurrentUser.OpenSubKey(RegSearch);
            using var policyKey = Registry.LocalMachine.OpenSubKey(RegWindowsSearchPolicy);

            var hasLegacyUserValue = userKey?.GetValue("DisableCloudSearch") is int userValue && userValue == 1;
            var hasPolicyValue = policyKey?.GetValue("AllowCloudSearch") is int policyValue && policyValue == 0;

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
        using var userKey = CreateCurrentUserSubKey(RegSearch);
        userKey.SetValue("DisableCloudSearch", 1, RegistryValueKind.DWord);

        // Microsoft documents cloud search as a Windows Search policy.
        // Keep the user value for compatibility with existing installs and add the policy-backed value.
        using var policyKey = CreateLocalMachineSubKey(RegWindowsSearchPolicy);
        policyKey.SetValue("AllowCloudSearch", 0, RegistryValueKind.DWord);
    }

    public void RevertCloudSearch()
    {
        DeleteCurrentUserValue(RegSearch, "DisableCloudSearch");
        DeleteLocalMachineValue(RegWindowsSearchPolicy, "AllowCloudSearch");
    }

    // 3. BingSearchEnabled
    // Check: Value == 0 (Optimized)
    public OptimizationStatus CheckBingSearch()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegSearch);
            if (key?.GetValue("BingSearchEnabled") is int iVal && iVal == 0)
            {
                return OptimizationStatus.Optimized;
            }
            return OptimizationStatus.NotOptimized;
        }
        catch
        {
            return OptimizationStatus.Unknown;
        }
    }

    public void ApplyBingSearch()
    {
        using var key = CreateCurrentUserSubKey(RegSearch);
        key.SetValue("BingSearchEnabled", 0, RegistryValueKind.DWord);
    }

    public void RevertBingSearch()
    {
        DeleteCurrentUserValue(RegSearch, "BingSearchEnabled");
    }

    // 4. ConnectedSearchUseWeb (Policy)
    // Check: Value == 0 (Optimized)
    public OptimizationStatus CheckWebResults()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegWindowsSearchPolicy);
            if (key?.GetValue("ConnectedSearchUseWeb") is int iVal && iVal == 0)
            {
                return OptimizationStatus.Optimized;
            }
            return OptimizationStatus.NotOptimized;
        }
        catch
        {
            return OptimizationStatus.Unknown;
        }
    }

    public void ApplyWebResults()
    {
        using var key = CreateLocalMachineSubKey(RegWindowsSearchPolicy);
        key.SetValue("ConnectedSearchUseWeb", 0, RegistryValueKind.DWord);
    }

    public void RevertWebResults()
    {
        DeleteLocalMachineValue(RegWindowsSearchPolicy, "ConnectedSearchUseWeb");
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
                    process.WaitForExit(milliseconds: 2_000);
                }
                catch
                {
                    // Ignore if the process has already exited or cannot be killed.
                }
            }
        }

        // Explicitly restart Explorer to avoid a blank screen.
        try
        {
            // Use the full path to prevent command hijacking.
            string explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            Process.Start(new ProcessStartInfo
            {
                FileName = explorerPath,
                UseShellExecute = true
            });
        }
        catch
        {
            // If it fails, Windows usually restarts it anyway, or the user can do it manually.
            // We swallow this because sometimes it throws if Explorer was already restarted.
        }
    }

    private static RegistryKey CreateCurrentUserSubKey(string subKey)
    {
        return Registry.CurrentUser.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Could not create or open HKCU\\{subKey}.");
    }

    private static RegistryKey CreateLocalMachineSubKey(string subKey)
    {
        return Registry.LocalMachine.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Could not create or open HKLM\\{subKey}.");
    }

    private static void DeleteCurrentUserValue(string subKey, string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(subKey, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    private static void DeleteLocalMachineValue(string subKey, string valueName)
    {
        using var key = Registry.LocalMachine.OpenSubKey(subKey, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }
}
