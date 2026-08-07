# Agentic.Desktop

[English](README.md) | 简体中文 | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md)

一个基于 WinUI 3 + Uno Platform 的 ACP（Agent Communication Protocol）桌面客户端。同一份代码库面向两个框架：原生 **WinUI 3** 构建（MSIX 打包、Mica 背景）和跨平台 **Uno Desktop / Skia** 构建（直接运行 exe、无需打包）。

## 功能特性

- **聊天界面** — 与 ACP Agent 进行实时流式对话，支持 Markdown 渲染
- **Agent 连接管理** — 通过 stdio 传输层连接任意 ACP 兼容的 Agent 可执行文件
- **内置 Mock Agent** — 无需真实 Agent 即可体验完整 UI 流程
- **权限管理** — Agent 请求文件/终端权限时弹出交互式确认对话框
- **终端管理** — 支持 Agent 发起的终端命令执行
- **Fluent Design** — Mica 背景、亚克力材质、自适应主题
- **双目标** — 单份 XAML 代码库同时产出原生 WinUI 3 应用与 Uno Skia 桌面应用

## 技术栈

| 组件 | 版本 |
| ------ | ------ |
| .NET | 10.0 |
| Windows App SDK | 2.3.1 |
| Uno.WinUI | 6.6.166 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Markdig | 1.3.2 |
| ShihaoShen.Agentic.ACPLibrary | 0.1.0-nightly |

## 系统要求

- Windows 10 1809 (Build 17763) 及以上
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WinApp CLI](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (`dotnet tool install -g winapp`)
- 开启 **开发者模式**（设置 > 系统 > 开发者选项）— 仅 WinUI / MSIX 目标需要

## 快速开始

仓库附带两个便捷脚本（以及通用的 `BuildAndRun.ps1`）：

| 脚本 | 目标 | 启动方式 | 需要开发者模式 |
| ------ | ------ | ------ | -------------- |
| `winui.ps1` | `net10.0-windows10.0.26100`（WinUI 3，MSIX） | `winapp run` | 是 |
| `uno.ps1` | `net10.0-desktop`（Uno / Skia，直接 exe） | 直接运行 `.exe` | 否 |

```powershell
# WinUI 3 构建（打包、原生）
.\winui.ps1                  # 构建 + 前台运行
.\winui.ps1 -Detach          # 构建 + 后台启动
.\winui.ps1 -SkipRun         # 仅构建

# Uno Desktop 构建（Skia，直接 exe）
.\uno.ps1                    # 构建 + 前台运行
.\uno.ps1 -Detach            # 构建 + 后台启动
.\uno.ps1 -SkipRun           # 仅构建
```

手动构建（不使用脚本）：

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
> `-m:1`（单进程构建）是必需的，用于规避 .NET 10 preview SDK 上 Uno `EmbeddedResourceInjectorTask` 在多进程 MSBuild 下偶发的 `MSB4018` 错误。两个脚本会自动应用此设置。

## 使用说明

1. 启动应用后进入 **设置** 页面
2. 配置 Agent：
   - **Agent 路径** — 填写 ACP Agent 可执行文件路径（留空使用内置 Mock Agent）
   - **Agent 参数** — 可选的启动参数
   - **工作目录** — Agent 的工作目录
3. 点击 **连接**，等待状态变为"已连接"
4. 切换到 **聊天** 页面开始对话

## 项目结构

```plaintext
App/
├── ViewModels/          # MVVM 视图模型
│   ├── ChatViewModel.cs         # 聊天逻辑、流式消息处理
│   ├── SettingsViewModel.cs     # Agent 连接管理
│   └── Messages/ChatMessage.cs  # 消息模型
├── Views/               # 对话框
│   └── PermissionDialog.xaml    # 权限确认对话框
├── Services/            # 基础服务
│   ├── FileSystemHandler.cs     # 文件系统权限处理
│   ├── PermissionHandler.cs     # 权限请求 UI 调度
│   ├── TerminalManager.cs       # 终端会话管理
│   └── MarkdownHelper.cs        # Markdown 渲染
├── Converters/          # XAML 值转换器
├── Mocks/               # Mock Agent 传输层
├── MainPage.xaml        # 聊天页面
├── SettingsPage.xaml    # 设置页面
└── MainWindow.xaml      # 主窗口（导航框架）
```

## 架构

应用采用 MVVM 架构，通过 `IAcpClient` 接口与 Agent 通信：

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

## 许可证

[MIT](LICENSE) © 2026 Shihao Shen
