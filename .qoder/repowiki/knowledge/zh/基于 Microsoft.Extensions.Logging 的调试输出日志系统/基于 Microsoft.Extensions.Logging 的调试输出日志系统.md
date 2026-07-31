---
kind: logging_system
name: 基于 Microsoft.Extensions.Logging 的调试输出日志系统
category: logging_system
scope:
    - '**'
source_files:
    - Agentic.Desktop/App.xaml.cs
    - Agentic.Desktop/ViewModels/SettingsViewModel.cs
---

本仓库采用 .NET 标准库 `Microsoft.Extensions.Logging` 作为日志框架，在 WinUI 3 桌面应用中提供统一的日志能力。

**系统与架构**
- 日志工厂在应用启动时于 `App.xaml.cs` 的 `OnLaunched` 中集中初始化，通过 `ILoggerFactory.Create` 构建全局单例 `App.LoggerFactory`。
- 当前仅配置了 `AddDebug()` 输出到 Visual Studio 调试输出窗口，最低日志级别设为 `LogLevel.Debug`。
- 没有文件、控制台或远程 sink，也没有结构化日志字段或日志轮转策略。

**使用方式**
- ViewModel 层通过 `App.LoggerFactory?.CreateLogger<T>()` 获取 ILogger 实例并注入给底层组件（如 `AcpClient`），示例见 `SettingsViewModel.cs` 第 86-87 行。
- 其他业务代码尚未直接使用 ILogger，目前日志输出集中在 AcpClient 等外部库内部。

**约定与限制**
- 日志仅输出到调试器，不适合生产环境；未提供配置文件或运行时切换日志级别的机制。
- 未定义统一的日志格式、字段规范或错误码约定。
- 开发者如需扩展，应遵循现有模式：在 App 启动阶段注册新的 sink，并通过 `App.LoggerFactory` 获取 logger 实例。