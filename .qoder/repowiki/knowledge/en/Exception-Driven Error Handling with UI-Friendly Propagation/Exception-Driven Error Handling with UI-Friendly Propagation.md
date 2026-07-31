---
kind: error_handling
name: Exception-Driven Error Handling with UI-Friendly Propagation
category: error_handling
scope:
    - '**'
source_files:
    - Agentic.Desktop/Services/FileSystemHandler.cs
    - Agentic.Desktop/Services/TerminalManager.cs
    - Agentic.Desktop/ViewModels/ChatViewModel.cs
    - Agentic.Desktop/Mocks/MockAgentTransport.cs
---

## What System/Approach Is Used

This WinUI 3 desktop application uses a straightforward exception-driven error handling approach in C#/.NET. There is no centralized error type hierarchy, custom exception classes, or middleware pipeline. Instead, the codebase relies on:
- Standard .NET exceptions (UnauthorizedAccessException, OperationCanceledException, generic Exception)
- try/catch blocks at service and ViewModel boundaries
- Event-based error propagation through events like `TransportFaulted`
- User-facing error messages localized via `LocalizationService.Format("ErrorPrefix", ex.Message)`

## Key Files and Packages

- **Agentic.Desktop/Services/FileSystemHandler.cs** — Throws `UnauthorizedAccessException` when path validation fails to prevent directory traversal attacks.
- **Agentic.Desktop/Services/TerminalManager.cs** — Silently catches `OperationCanceledException` and generic `Exception` in async process I/O streams; swallows errors in process kill/release paths.
- **Agentic.Desktop/ViewModels/ChatViewModel.cs** — Central error presentation: catches `OperationCanceledException` (user cancel) and generic `Exception`, appending localized error text to the streaming agent message.
- **Agentic.Desktop/Mocks/MockAgentTransport.cs** — Uses an event `TransportFaulted` to propagate transport-level exceptions upward instead of throwing them directly.
- **Agentic.Desktop/Converters/*.cs** — Throw `NotImplementedException` for unimplemented converter back-conversions (boilerplate pattern).

## Architecture and Conventions

1. **Boundary-Level Catch-and-Present**: Services throw specific exceptions (e.g., `UnauthorizedAccessException`), while ViewModels catch broad `Exception` types and convert them into user-friendly localized strings appended to the UI state.
2. **Cancellation-Aware**: `OperationCanceledException` is consistently caught separately from general exceptions to distinguish user-initiated cancellation from real errors.
3. **Silent Failure Patterns**: Terminal I/O and process lifecycle operations swallow exceptions silently (`catch { }`) to avoid crashing the UI during background task cleanup.
4. **Event-Based Transport Errors**: The mock transport raises `TransportFaulted` events rather than throwing, allowing callers to handle failures asynchronously without try/catch.
5. **No Global Error Handler**: There is no `App.UnhandledException` handler, global exception filter, or logging framework configured in the visible codebase.

## Rules Developers Should Follow

- **Throw domain-specific exceptions** at service boundaries (e.g., `UnauthorizedAccessException` for permission violations).
- **Catch `OperationCanceledException` separately** from general exceptions to properly handle user cancellations.
- **Present errors locallyally** using `LocalizationService.Format("ErrorPrefix", ex.Message)` rather than raw exception messages.
- **Avoid swallowing exceptions silently** except in non-critical cleanup paths (process disposal, terminal release).
- **Use events for asynchronous error propagation** in transport-like components instead of throwing across async boundaries.
- **Do not rely on global exception handlers** — handle errors at the boundary where they can be meaningfully presented to users.