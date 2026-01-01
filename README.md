# 🔎 Disable Windows 11 25H2 Online Search
 ![Status](https://img.shields.io/badge/Status-Stable-success) ![Platform](https://img.shields.io/badge/Platform-Windows%2011-blue) ![License](https://img.shields.io/badge/License-MIT-green)
 [![GitHub downloads](https://img.shields.io/github/downloads/JohnWiliam/Disable-Windows-11-25H2-Online-Search/total?style=for-the-badge&color=green)](https://github.com/JohnWiliam/Disable-Windows-11-25H2-Online-Search/releases/latest)

A modern, portable, and lightweight utility designed to reclaim your privacy by disabling online search integrations in the Windows 11 Start Menu. Built with **C#** and the beautiful **WPF-UI** library, featuring a native **Mica** visual effect.

---

## ✨ Features

This tool focuses on three core optimizations to speed up your local search and enhance privacy:

*   **🚫 Block Search Suggestions**
    *   *Effect:* Prevents Windows from sending keystrokes to Microsoft as you type in the search box.
    *   *Registry Key:* `HKCU\Software\Policies\Microsoft\Windows\Explorer` -> `DisableSearchBoxSuggestions`

*   **☁️ Disable Cloud Search**
    *   *Effect:* Stops the search menu from fetching content from your OneDrive, Outlook, and other Microsoft account services.
    *   *Registry Key:* `HKCU\Software\Microsoft\Windows\CurrentVersion\Search` -> `DisableCloudSearch`

*   **🌐 Remove Bing Integration**
    *   *Effect:* Removes web results, news, and trending stories from the Start Menu, ensuring only local files and apps are shown.
    *   *Registry Key:* `HKCU\Software\Microsoft\Windows\CurrentVersion\Search` -> `BingSearchEnabled`

---

## 🎨 Visuals & Design

*   **Mica Backdrop:** Utilizes the desktop wallpaper for a transparent, native Windows 11 feel.
*   **Compact UI:** Designed to fit perfectly without scrollbars (`720px` width).
*   **Light Theme:** Enforced clean and bright aesthetics.
*   **Localized:** Fully translated into **Portuguese (Brazil)** 🇧🇷.

---

## 🛠️ Code Structure

The project is structured for clarity and maintainability:

*   **`DisableWin11Search/`** _(Root)_
    *   **`MainWindow.xaml`**: Defines the modern GUI using `Wpf.Ui.Controls` (CardExpander, FluentWindow).
    *   **`Services/RegistryService.cs`**: Contains the business logic. It handles safe Registry reading/writing and manages the `explorer.exe` restart process.
    *   **`App.xaml`**: Configures the application resources and global exception handling.
    *   **`app.manifest`**: Enforces `requireAdministrator` privileges to ensure registry access.

---

## 🏗️ Build Instructions

You don't need to install anything on the target machine to run the app, but to build it from source:

1.  **Prerequisites**: Install the **.NET 10.0 SDK**.
2.  **Compile**:
    Run the included batch script:
    ```cmd
    build.bat
    ```
3.  **Output**:
    The portable executable will be generated in the `Build/` folder.
    *   *Note:* The build uses Single-File Compression to keep the `.exe` size optimized.

---

## 👥 Credits

*   **Development**: John Wiliam & AI.
*   **UI Library**: [WPF-UI](https://wpfui.lepo.co/api/Wpf.Ui.html) by Lepoco.

---

> **⚠️ Disclaimer**: This tool modifies Windows Registry keys. While safe and reversible (via the "Revert" buttons), always use caution when modifying system settings.
