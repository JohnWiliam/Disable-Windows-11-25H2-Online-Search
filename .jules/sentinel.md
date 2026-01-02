## 2024-05-23 - Prevent Command Hijacking in Process Execution
**Vulnerability:** Unqualified `explorer.exe` execution in `Process.Start`.
**Learning:** Relying on system PATH or default search behavior for critical system executables can lead to ambiguity or hijacking if a malicious file with the same name exists in the application directory.
**Prevention:** Always use `Environment.GetFolderPath` (e.g., `SpecialFolder.Windows`) and `Path.Combine` to resolve the full path of system executables before running them.
