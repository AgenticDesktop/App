# Agentic Desktop

English | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md)

An ACP (Agent Communication Protocol) desktop client built on WinUI 3 + Uno Platform. It targets two frameworks from a single codebase: a native **WinUI 3** build (MSIX-packaged, Mica backdrop) and a cross-platform **Uno Desktop / Skia** build (direct exe, no packaging).

## Features

- **Chat Interface** — Real-time streaming conversation with ACP Agents, with Markdown rendering support
- **Agent Connection Management** — Connect to any ACP-compatible Agent executable via stdio transport layer
- **Built-in Mock Agent** — Experience the full UI workflow without a real Agent
- **Permission Management** — Interactive confirmation dialog when Agents request file/terminal permissions
- **Terminal Management** — Support for terminal command execution initiated by Agents
- **Fluent Design** — Mica background, acrylic material, adaptive theme
- **Dual-target** — Single XAML codebase produces a native WinUI 3 app and an Uno Skia desktop app

## Tech Stack

| Component | Version |
| ----------- | --------- |
| .NET | 10.0 |
| Windows App SDK | 2.3.1 |
| Uno.WinUI | 6.6.166 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Markdig | 1.3.2 |
| ShihaoShen.Agentic.ACPLibrary | 0.2.0 |

## System Requirements

- Windows 10 1809 (Build 17763) or later
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WinApp CLI](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (`dotnet tool install -g winapp`)
- **Developer Mode** enabled (Settings > System > Developer Options) — required only for the WinUI / MSIX target

## Quick Start

> [!WARNING]
> Building on non-Windows platforms (Linux, macOS) is not officially supported, even for the Uno Desktop build. For better experience, use Windows (Physical Machine, VMware, Parallel Desktop, etc.) instead.

> [!IMPORTANT]
> If you are developing the app alongside the `ShihaoShen.Agentic.ACPLibrary` library, make sure to clone both repos into the same parent folder so that the `Agentic.Desktop` project can reference the local library project instead of the NuGet package.
>
> Like this:
>
> ```plaintext
> AgenticDesktop-DevFolder/
> ├── App/ (This repo)
> └── ACPLibrary/ (The library repo)
> ```

The repo ships two convenience scripts:

| Script | Target | Launch | Needs Dev Mode |
| ------ | ------ | ------ | -------------- |
| `winui.ps1` | `net10.0-windows10.0.26100` (WinUI 3, MSIX) | `winapp run` | Yes |
| `uno.ps1` | `net10.0-desktop` (Uno / Skia, direct exe) | runs `.exe` directly | No |

```powershell
# WinUI 3 build (packaged, native)
.\winui.ps1                  # build + run (foreground)
.\winui.ps1 -Detach          # build + launch in background
.\winui.ps1 -SkipRun         # build only

# Uno Desktop build (Skia, direct exe)
.\uno.ps1                    # build + run
.\uno.ps1 -SkipRun           # build only
```

Manual build (without scripts):

```powershell
git clone https://github.com/AgenticDesktop/App.git
cd App

# WinUI 3
dotnet build -p:Platform=x64 -f net10.0-windows10.0.26100 -m:1
winapp run Agentic.Desktop\bin\x64\Debug\net10.0-windows10.0.26100\win-x64

# Uno Desktop
dotnet build -p:Platform=x64 -f net10.0-desktop -m:1
Agentic.Desktop\bin\x64\Debug\net10.0-desktop\Agentic.Desktop.exe
```

> [!NOTE]
> `-m:1` (single-process build) is required to work around an intermittent `MSB4018` from Uno's `EmbeddedResourceInjectorTask` under multi-proc MSBuild on the .NET 10 preview SDK. Both scripts apply this automatically.

## Usage

1. After launching the app, go to the **Settings** page
2. Configure the Agent:
   - **Agent Path** — Enter the path to the ACP Agent executable (leave empty to use the built-in Mock Agent)
   - **Agent Arguments** — Optional startup arguments
   - **Working Directory** — The working directory for the Agent
3. Click **Connect** and wait for the status to change to "Connected"
4. Switch to the **Chat** page to start a conversation

## Project Structure

```plaintext
App/
├── ViewModels/          # MVVM view models
│   ├── ChatViewModel.cs         # Chat logic, streaming message handling
│   ├── ChatListViewModel.cs     # Chat session list management
│   ├── SettingsViewModel.cs     # Agent connection management
│   └── Messages/
│       ├── ChatMessage.cs       # Message model
│       └── ChatSession.cs       # Chat session model
├── Views/               # Dialogs and panels
│   ├── ChatListPanel.xaml       # Chat session list panel
│   ├── ChatListPanel.xaml.cs
│   ├── PermissionDialog.xaml    # Permission confirmation dialog
│   └── PermissionDialog.xaml.cs
├── Services/            # Core services
│   ├── FileSystemHandler.cs     # File system permission handling
│   ├── LocalizationService.cs   # Localization / i18n
│   ├── PermissionHandler.cs     # Permission request UI dispatching
│   ├── TerminalManager.cs       # Terminal session management
│   └── MarkdownHelper.cs        # Markdown rendering
├── Converters/          # XAML value converters
├── Mocks/               # Mock Agent transport layer
├── MainPage.xaml        # Chat page
├── SettingsPage.xaml    # Settings page
└── MainWindow.xaml      # Main window (navigation framework)
```

## Architecture

The application uses the MVVM architecture and communicates with Agents through the `IAcpClient` interface:

```plaintext
┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  WinUI UI   │────▶│   ViewModels     │────▶│   AcpClient     │
│  (XAML)     │◀────│  (CommunityToolkit)│◀────│  (ACP Library)  │
└─────────────┘     └──────────────────┘     └────────┬────────┘
                                                      │
                                             ┌────────▼────────┐
                                             │  IAgentTransport │
                                             │  (stdio / mock)  │
                                             └─────────────────┘
```

## License

[MIT](LICENSE) © 2026 Shihao Shen
