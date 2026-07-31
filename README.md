# Agentic Desktop

English | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md)

A WinUI 3-based ACP (Agent Communication Protocol) desktop client that provides a chat interface for interacting with AI Agents.

## Features

- **Chat Interface** — Real-time streaming conversation with ACP Agents, with Markdown rendering support
- **Agent Connection Management** — Connect to any ACP-compatible Agent executable via stdio transport layer
- **Built-in Mock Agent** — Experience the full UI workflow without a real Agent
- **Permission Management** — Interactive confirmation dialog when Agents request file/terminal permissions
- **Terminal Management** — Support for terminal command execution initiated by Agents
- **Fluent Design** — Mica background, acrylic material, adaptive theme

## Tech Stack

| Component | Version |
| ----------- | --------- |
| .NET | 10.0 |
| Windows App SDK | 2.3.1 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Markdig | 1.3.2 |
| ShihaoShen.Agentic.ACPLibrary  | 0.1.0-beta.3 |

## System Requirements

- Windows 10 1809 (Build 17763) or later
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WinApp CLI](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (`dotnet tool install -g winapp`)
- **Developer Mode** enabled (Settings > System > Developer Options)

## Quick Start

```powershell
# Clone the repository
git clone https://github.com/AgenticDesktop/App.git
cd App
dotnet build -p:Platform=x64
winapp run bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64
```

## Usage

1. After launching the app, go to the **Settings** page
2. Configure the Agent:
   - **Agent Path** — Enter the path to the ACP Agent executable (leave empty to use the built-in Mock Agent)
   - **Agent Arguments** — Optional startup arguments
   - **Working Directory** — The working directory for the Agent
3. Click **Connect** and wait for the status to change to "Connected"
4. Switch to the **Chat** page to start a conversation

## Project Structure

```
App/
├── ViewModels/          # MVVM view models
│   ├── ChatViewModel.cs         # Chat logic, streaming message handling
│   ├── SettingsViewModel.cs     # Agent connection management
│   └── Messages/ChatMessage.cs  # Message model
├── Views/               # Dialogs
│   └── PermissionDialog.xaml    # Permission confirmation dialog
├── Services/            # Core services
│   ├── FileSystemHandler.cs     # File system permission handling
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

```
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
