---
kind: logging_system
name: Microsoft.Extensions.Logging with Debug Sink
category: logging_system
scope:
    - '**'
source_files:
    - Agentic.Desktop/App.xaml.cs
    - Agentic.Desktop/ViewModels/SettingsViewModel.cs
---

The application uses Microsoft.Extensions.Logging as its logging framework, configured centrally in the WinUI 3 `App` class and consumed throughout the codebase via dependency injection-style factory access.

**Framework and configuration**
- The logging system is initialized in `App.xaml.cs` during `OnLaunched`, creating a global `ILoggerFactory` stored as a static property `App.LoggerFactory`.
- Configuration adds only the built-in `Debug` sink via `builder.AddDebug()` and sets the minimum log level to `LogLevel.Debug`.
- No additional sinks (file, console, event log) are registered; all output goes to the Visual Studio Output window / debugger attached process.

**Usage pattern**
- Components obtain an `ILogger<T>` by calling `App.LoggerFactory?.CreateLogger<T>()`, where `T` is typically the consuming type (e.g., `AcpClient`).
- In `SettingsViewModel.ConnectAsync()`, a logger is created for `AcpClient` and passed into the client constructor: `var logger = App.LoggerFactory?.CreateLogger<AcpClient>(); AcpClient = new AcpClient(transport, dispatcher, logger);`.
- This follows the standard Microsoft.Extensions.Logging convention of passing `ILogger<T>` through constructors rather than using DI containers.

**Architecture decisions**
- Logging is intentionally minimal — no structured logging fields, no correlation IDs, no external sinks. The choice of `Debug` sink targets development-time debugging against the desktop app.
- The global `App.LoggerFactory` singleton replaces DI-based logger resolution, keeping the WinUI 3 app lightweight without requiring a full DI container setup.
- Log levels are not configurable at runtime; the minimum level is fixed at `Debug` in the factory builder.

**Conventions developers should follow**
- Obtain loggers via `App.LoggerFactory.CreateLogger<YourType>()` rather than instantiating loggers directly.
- Use the standard `ILogger.Log(LogLevel, EventId, State, Exception)` methods or extension methods (`LogInformation`, `LogError`, etc.).
- Do not add custom sinks or change the global logger factory outside of `App.OnLaunched`.
- Keep log messages concise and machine-parseable since they go to the debug output.