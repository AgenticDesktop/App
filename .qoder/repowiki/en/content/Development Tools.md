# Development Tools

<cite>
**Referenced Files in This Document**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [Resources.resw (en)](file://Agentic.Desktop/Agentic.Desktop/Strings/en/Resources.resw)
- [Resources.resw (ja)](file://Agentic.Desktop/Agentic.Desktop/Strings/ja/Resources.resw)
- [Resources.resw (zh-CN)](file://Agentic.Desktop/Agentic.Desktop/Strings/zh-CN/Resources.resw)
- [Resources.resw (zh-TW)](file://Agentic.Desktop/Agentic.Desktop/Strings/zh-TW/Resources.resw)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)
- [FileSystemHandler.cs](file://Agentic.Desktop/Agentic.Desktop/Services/FileSystemHandler.cs)
- [ChatMessage.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatSession.cs)
- [validate_resw.ps1](file://Agentic.Desktop/validate_resw.ps1)
- [ChatMessageTests.cs](file://Agentic.Desktop.Tests/ChatMessageTests.cs)
- [FileSystemHandlerTests.cs](file://Agentic.Desktop.Tests/FileSystemHandlerTests.cs)
- [MarkdownHelperTests.cs](file://Agentic.Desktop.Tests/MarkdownHelperTests.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)
- [Agentic.Desktop.Tests.csproj](file://Agentic.Desktop.Tests/Agentic.Desktop.Tests.csproj)
</cite>

## Update Summary
**Changes Made**
- Added comprehensive xUnit testing infrastructure section
- Updated testing capabilities with four major test classes
- Enhanced unit testing approaches for ViewModels and services
- Added test project configuration and framework details
- Expanded troubleshooting guide with testing-specific guidance

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Testing Infrastructure](#testing-infrastructure)
7. [Dependency Analysis](#dependency-analysis)
8. [Performance Considerations](#performance-considerations)
9. [Troubleshooting Guide](#troubleshooting-guide)
10. [Conclusion](#conclusion)
11. [Appendices](#appendices)

## Introduction
This document explains Agentic.Desktop's development tools and testing capabilities with a focus on:
- MockAgentTransport: a mock ACP transport that simulates protocol behavior for UI development and testing without external dependencies, including scripted responses and error simulation.
- Localization system: English, Japanese, Simplified Chinese, and Traditional Chinese via .resw resource files, accessed through LocalizationService for dynamic language switching and string management.
- **New**: Comprehensive xUnit testing infrastructure with four major test classes covering chat messages, file system handlers, markdown helpers, and mock agent transport.
- Practical guidance for creating test scenarios, debugging WinUI 3 applications, and performance profiling strategies.
- Extending the mock agent for custom tests and adding new languages to the localization system.
- Unit testing approaches for ViewModels and services using xUnit framework.

## Project Structure
The relevant parts of the project structure for development and testing are:
- Mocks: MockAgentTransport implements IAgentTransport to simulate ACP protocol messages and streaming updates.
- Services: LocalizationService reads localized strings from .resw resources; TerminalManager manages terminal processes; FileSystemHandler enforces working directory access.
- ViewModels: ChatViewModel orchestrates chat interactions and uses either a real IAcpClient or local mock simulation; SettingsViewModel wires transports (mock or stdio), initializes sessions, and exposes connection state.
- Strings: Resources.resw per culture provides localized UI text used by both XAML and C#.
- **New**: Tests: Dedicated test project with xUnit framework containing comprehensive unit tests for core components.

```mermaid
graph TB
subgraph "Mocks"
MAT["MockAgentTransport"]
end
subgraph "Services"
LS["LocalizationService"]
TM["TerminalManager"]
FSH["DesktopFileSystemHandler"]
MH["MarkdownHelper"]
end
subgraph "ViewModels"
SVM["SettingsViewModel"]
CVM["ChatViewModel"]
CM["ChatMessage"]
CS["ChatSession"]
end
subgraph "Resources"
RES_EN["Strings/en/Resources.resw"]
RES_JA["Strings/ja/Resources.resw"]
RES_ZH_CN["Strings/zh-CN/Resources.resw"]
RES_ZH_TW["Strings/zh-TW/Resources.resw"]
end
subgraph "Tests"
CMT["ChatMessageTests"]
FHT["FileSystemHandlerTests"]
MHT["MarkdownHelperTests"]
MAT["MockAgentTransportTests"]
end
SVM --> MAT
SVM --> LS
CVM --> LS
CVM --> CM
CVM --> CS
FSH --> LS
TM --> LS
LS --> RES_EN
LS --> RES_JA
LS --> RES_ZH_CN
LS --> RES_ZH_TW
CMT --> CM
FHT --> FSH
MHT --> MH
MAT --> MAT
```

**Diagram sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)
- [FileSystemHandler.cs](file://Agentic.Desktop/Agentic.Desktop/Services/FileSystemHandler.cs)
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatMessage.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatSession.cs)
- [ChatMessageTests.cs](file://Agentic.Desktop.Tests/ChatMessageTests.cs)
- [FileSystemHandlerTests.cs](file://Agentic.Desktop.Tests/FileSystemHandlerTests.cs)
- [MarkdownHelperTests.cs](file://Agentic.Desktop.Tests/MarkdownHelperTests.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)

**Section sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [Agentic.Desktop.Tests.csproj](file://Agentic.Desktop.Tests/Agentic.Desktop.Tests.csproj)

## Core Components
- MockAgentTransport: Implements IAgentTransport and emits JSON-RPC-like messages for initialize, session/new, session/prompt, and session/cancel. It supports streaming chunks and cancellation tokens, and raises events for message delivery and faults.
- LocalizationService: Static wrapper around Windows ResourceLoader to fetch localized strings by key and format them with arguments.
- SettingsViewModel: Wires up either MockAgentTransport or StdioAgentTransport based on configuration, initializes AcpClient, creates sessions, and exposes connection state and lifecycle hooks.
- ChatViewModel: Orchestrates sending prompts, handling streaming updates, tool call notifications, and fallback local mock simulation when no client is bound.
- TerminalManager: Manages multiple terminal processes, reading stdout/stderr asynchronously and exposing output and lifecycle methods.
- DesktopFileSystemHandler: Enforces working-directory-scoped file access and throws localized unauthorized access exceptions.
- **New**: MarkdownHelper: Provides markdown conversion utilities for HTML and plain text processing.

**Section sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)
- [FileSystemHandler.cs](file://Agentic.Desktop/Agentic.Desktop/Services/FileSystemHandler.cs)

## Architecture Overview
The application uses a layered architecture where ViewModels coordinate user actions and orchestrate services and transports. The mock transport enables UI development and testing without an external agent process.

```mermaid
sequenceDiagram
participant User as "User"
participant CVM as "ChatViewModel"
participant SVM as "SettingsViewModel"
participant Trans as "IAgentTransport<br/>MockAgentTransport"
participant Client as "AcpClient"
participant LS as "LocalizationService"
User->>SVM : Connect (no AgentPath)
SVM->>Trans : new MockAgentTransport()
SVM->>Client : InitializeAsync()
Client-->>SVM : AgentInfo
SVM-->>CVM : OnAgentConnected(AcpClient)
User->>CVM : Send Message
CVM->>Client : SendPromptAsync(sessionId, prompt)
Client->>Trans : SendAsync(method="session/prompt")
Trans-->>Client : stream session/update chunks
Client-->>CVM : SessionUpdated(chunk)
CVM->>LS : Get("ToolCallPrefix"/"ErrorPrefix")
CVM-->>User : Streamed response + tool calls
```

**Diagram sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)

## Detailed Component Analysis

### MockAgentTransport
MockAgentTransport simulates ACP protocol behavior:
- State machine: Created -> Running -> Stopped.
- Methods: StartAsync sets running state; StopAsync cancels any in-flight prompt and transitions to stopped.
- SendAsync parses incoming JSON lines, dispatches by method:
  - initialize: returns protocol version, agent capabilities/info, auth methods.
  - session/new: returns a mock session ID.
  - session/prompt: streams multiple update notifications with randomized delays, then sends final stop reason.
  - session/cancel: cancels linked token source.
- Events: MessageReceived for outgoing JSON, TransportFaulted for errors, ProcessExited not used here.
- Error simulation: Exceptions during SendAsync raise TransportFaulted; OperationCanceledException is handled gracefully.

```mermaid
classDiagram
class MockAgentTransport {
-int _requestId
-TransportState _state
-CancellationTokenSource? _promptCts
+StartAsync(cancellationToken) Task
+SendAsync(jsonLine, cancellationToken) Task
+StopAsync() Task
+MessageReceived event
+TransportFaulted event
+ProcessExited event
-FireMessageAsync(json) Task
-BuildResponse(id, result) string
}
```

**Diagram sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)

**Section sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)

### LocalizationSystem
- Resource files: Each culture has a Resources.resw under Strings/{culture}. These contain all UI strings referenced by XAML and code.
- LocalizationService: Uses Windows.ApplicationModel.Resources.ResourceLoader with name "Resources" to retrieve strings by key and format with parameters.
- Dynamic language switching: While the service itself does not change culture, WinRT will resolve strings according to current UI culture; changing the UI culture at runtime causes subsequent calls to return localized values for the new culture.

```mermaid
flowchart TD
Start(["Get localized string"]) --> Loader["ResourceLoader.GetString(key)"]
Loader --> Found{"Key found?"}
Found --> |Yes| Return["Return value"]
Found --> |No| Fallback["Fallback to default culture"]
Fallback --> Return
```

**Diagram sources**
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [Resources.resw (en)](file://Agentic.Desktop/Agentic.Desktop/Strings/en/Resources.resw)
- [Resources.resw (ja)](file://Agentic.Desktop/Agentic.Desktop/Strings/ja/Resources.resw)
- [Resources.resw (zh-CN)](file://Agentic.Desktop/Agentic.Desktop/Strings/zh-CN/Resources.resw)
- [Resources.resw (zh-TW)](file://Agentic.Desktop/Agentic.Desktop/Strings/zh-TW/Resources.resw)

**Section sources**
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [Resources.resw (en)](file://Agentic.Desktop/Agentic.Desktop/Strings/en/Resources.resw)
- [Resources.resw (ja)](file://Agentic.Desktop/Agentic.Desktop/Strings/ja/Resources.resw)
- [Resources.resw (zh-CN)](file://Agentic.Desktop/Agentic.Desktop/Strings/zh-CN/Resources.resw)
- [Resources.resw (zh-TW)](file://Agentic.Desktop/Agentic.Desktop/Strings/zh-TW/Resources.resw)

### SettingsViewModel and Transport Wiring
SettingsViewModel controls connection lifecycle:
- ConnectAsync chooses MockAgentTransport if AgentPath is empty, otherwise uses StdioAgentTransport with provided arguments and working directory.
- Initializes AcpClient with JsonRpcDispatcher and optional logger.
- Subscribes to AgentProcessExited to report disconnection with localized status.
- Creates a session and notifies ChatViewModel via OnAgentConnected.
- DisconnectAsync cleans up client and terminal manager, resets state.

```mermaid
sequenceDiagram
participant UI as "Settings Page"
participant SVM as "SettingsViewModel"
participant Trans as "IAgentTransport"
participant Client as "AcpClient"
participant Term as "TerminalManager"
UI->>SVM : ConnectAsync()
alt Empty AgentPath
SVM->>Trans : new MockAgentTransport()
else Non-empty AgentPath
SVM->>Trans : new StdioAgentTransport(path, args, workDir)
end
SVM->>Client : InitializeAsync()
Client-->>SVM : AgentInfo
SVM->>Term : new TerminalManager()
Client->>Term : TerminalHandler = Term
SVM->>Client : CreateSessionAsync(workDir)
Client-->>SVM : sessionId
SVM-->>UI : Update status, IsConnected=true
```

**Diagram sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)

### ChatViewModel and Streaming Flow
ChatViewModel handles user input and agent responses:
- SendMessageAsync adds user message, creates streaming agent message placeholder, and either sends via AcpClient or simulates locally.
- OnSessionUpdated merges streaming chunks with batching to reduce UI churn and marshals updates to the UI thread.
- ToolCallNotification inserts system messages and updates session preview.
- CancelGenerationAsync requests cancellation and clears streaming state.

```mermaid
sequenceDiagram
participant User as "User"
participant CVM as "ChatViewModel"
participant Client as "AcpClient"
participant LS as "LocalizationService"
User->>CVM : Send Message
CVM->>CVM : Add user message + create streaming agent msg
alt Client available
CVM->>Client : SendPromptAsync(sessionId, prompt)
Client-->>CVM : SessionUpdated(chunk/tool call)
CVM->>LS : Format("ToolCallPrefix"/"ErrorPrefix")
CVM-->>User : Append streamed text + tool call entries
else No client
CVM->>CVM : SimulateMockResponseAsync(agentMsg, text)
CVM-->>User : Local mock streamed response
end
```

**Diagram sources**
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)

**Section sources**
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)

### Data Models
- ChatMessage: Observable object with Id, Role, Timestamp, TextContent, IsStreaming. Used to represent individual messages in conversation history.
- ChatSession: Observable object with Id, Title (localized default), timestamps, previewText, and Messages collection.

```mermaid
classDiagram
class ChatMessage {
+string Id
+MessageRole Role
+DateTime Timestamp
+string TextContent
+bool IsStreaming
}
class ChatSession {
+string Id
+string Title
+DateTime CreatedAt
+DateTime UpdatedAt
+string PreviewText
+ObservableCollection~ChatMessage~ Messages
}
ChatSession "1" o-- "*" ChatMessage : contains
```

**Diagram sources**
- [ChatMessage.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatSession.cs)

**Section sources**
- [ChatMessage.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/Messages/ChatSession.cs)

## Testing Infrastructure

### xUnit Framework Setup
The project includes a comprehensive xUnit testing infrastructure with the following configuration:
- **Framework**: xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1
- **Runner**: xunit.runner.visualstudio 3.1.0 for Visual Studio integration
- **Coverage**: coverlet.collector 6.0.4 for code coverage analysis
- **Target**: net10.0-windows10.0.26100 for Windows platform compatibility
- **MSIX Integration**: Disabled MSIX/PRI tooling for clean test execution

### Test Classes Overview
Four major test classes provide comprehensive coverage of core functionality:

#### ChatMessageTests
Validates ChatMessage model behavior including:
- Constructor parameter validation and default values
- Unique ID generation per instance
- Timestamp accuracy and recent time validation
- Property change notification for TextContent and IsStreaming properties
- Text content accumulation and streaming state management

#### FileSystemHandlerTests  
Tests DesktopFileSystemHandler security and functionality:
- File read/write operations within working directory scope
- Subdirectory creation and nested path handling
- Security validation preventing access outside working directory
- File overwrite behavior and empty file handling
- Proper cleanup with IDisposable pattern

#### MarkdownHelperTests
Verifies markdown conversion utilities:
- HTML conversion for bold, headings, and various markdown syntax
- Plain text extraction removing formatting markers
- Handling of code blocks, inline code, and links
- Edge cases with empty, null, and whitespace inputs
- Mixed markdown content processing

#### MockAgentTransportTests
Comprehensive testing of mock ACP transport:
- State machine transitions (Created → Running → Stopped)
- JSON-RPC message handling for initialize, session/new, session/prompt
- Streaming chunk generation and final response validation
- Unknown method handling and error scenarios
- Message event subscription and payload verification

### Test Execution and Configuration
```mermaid
flowchart TD
Start(["Test Execution"]) --> Build["Build Test Project"]
Build --> Discover["Discover xUnit Tests"]
Discover --> Run["Run Test Cases"]
Run --> Assert["Execute Assertions"]
Assert --> Coverage["Generate Coverage Report"]
Coverage --> Results["Display Test Results"]
subgraph "Test Framework"
xUnit["xUnit Framework"]
VS["Visual Studio Runner"]
Coverlet["Coverlet Collector"]
end
subgraph "Test Categories"
ModelTests["Model Tests"]
ServiceTests["Service Tests"]
TransportTests["Transport Tests"]
UtilityTests["Utility Tests"]
end
```

**Diagram sources**
- [Agentic.Desktop.Tests.csproj](file://Agentic.Desktop.Tests/Agentic.Desktop.Tests.csproj)
- [ChatMessageTests.cs](file://Agentic.Desktop.Tests/ChatMessageTests.cs)
- [FileSystemHandlerTests.cs](file://Agentic.Desktop.Tests/FileSystemHandlerTests.cs)
- [MarkdownHelperTests.cs](file://Agentic.Desktop.Tests/MarkdownHelperTests.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)

**Section sources**
- [Agentic.Desktop.Tests.csproj](file://Agentic.Desktop.Tests/Agentic.Desktop.Tests.csproj)
- [ChatMessageTests.cs](file://Agentic.Desktop.Tests/ChatMessageTests.cs)
- [FileSystemHandlerTests.cs](file://Agentic.Desktop.Tests/FileSystemHandlerTests.cs)
- [MarkdownHelperTests.cs](file://Agentic.Desktop.Tests/MarkdownHelperTests.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)

## Dependency Analysis
- MockAgentTransport depends on the ACP library transport interface and System.Text.Json for serialization.
- LocalizationService depends on Windows.ApplicationModel.Resources.ResourceLoader.
- SettingsViewModel depends on AcpClient, JsonRpcDispatcher, and either MockAgentTransport or StdioAgentTransport.
- ChatViewModel depends on AcpClient, CommunityToolkit.Mvvm, and LocalizationService.
- TerminalManager and FileSystemHandler depend on LocalizationService for localized messages.
- **New**: Test classes depend on xUnit framework and target specific components for isolated testing.

```mermaid
graph LR
SVM["SettingsViewModel"] --> MAT["MockAgentTransport"]
SVM --> ACP["AcpClient"]
SVM --> TERM["TerminalManager"]
CVM["ChatViewModel"] --> ACP
CVM --> LS["LocalizationService"]
FSH["DesktopFileSystemHandler"] --> LS
TM["TerminalManager"] --> LS
CMT["ChatMessageTests"] --> CM["ChatMessage"]
FHT["FileSystemHandlerTests"] --> FSH
MHT["MarkdownHelperTests"] --> MH["MarkdownHelper"]
MAT["MockAgentTransportTests"] --> MAT
```

**Diagram sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)
- [FileSystemHandler.cs](file://Agentic.Desktop/Agentic.Desktop/Services/FileSystemHandler.cs)
- [ChatMessageTests.cs](file://Agentic.Desktop.Tests/ChatMessageTests.cs)
- [FileSystemHandlerTests.cs](file://Agentic.Desktop.Tests/FileSystemHandlerTests.cs)
- [MarkdownHelperTests.cs](file://Agentic.Desktop.Tests/MarkdownHelperTests.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Agentic.Desktop/Services/LocalizationService.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)
- [FileSystemHandler.cs](file://Agentic.Desktop/Agentic.Desktop/Services/FileSystemHandler.cs)

## Performance Considerations
- Streaming batching: ChatViewModel batches incoming text chunks over a short delay to reduce UI updates and improve responsiveness.
- DispatcherQueue usage: All UI updates are marshaled to the UI thread using DispatcherQueue.TryEnqueue to avoid cross-thread exceptions.
- Token-based cancellation: MockAgentTransport uses CancellationTokenSource to support prompt cancellation and graceful shutdown.
- Terminal output buffering: TerminalManager buffers stdout/stderr asynchronously to prevent blocking and ensure smooth operation.
- Resource loading: LocalizationService uses ResourceLoader which caches strings; ensure keys are consistent across cultures to avoid misses.
- **New**: Test isolation: Each test class uses proper disposal patterns and temporary directories to prevent test interference.
- **New**: Async test patterns: All async operations use proper await patterns and timeout handling in test scenarios.

[No sources needed since this section provides general guidance]

## Troubleshooting Guide
- Missing localized strings: If a key is missing in a culture's Resources.resw, ResourceLoader may throw or fall back to default. Use validate_resw.ps1 to verify XML validity and count data entries.
- Transport faults: MockAgentTransport raises TransportFaulted on exceptions during SendAsync; subscribe to this event to capture and log errors.
- Cancellation issues: Ensure session/cancel is sent before stopping transport; MockAgentTransport cancels linked tokens to abort streaming prompts.
- File access denied: DesktopFileSystemHandler validates paths against working directory; UnauthorizedAccessException includes localized message indicating denied path.
- Terminal process leaks: TerminalManager.Dispose kills remaining processes and releases resources; ensure it is disposed on app shutdown.
- **New**: Test failures: Check xUnit test output window for detailed failure information and stack traces.
- **New**: Test isolation issues: Ensure proper cleanup in Dispose methods and use unique temporary directories for file system tests.
- **New**: Coverage gaps: Use coverlet collector to identify untested code paths and add corresponding test cases.

**Section sources**
- [validate_resw.ps1](file://Agentic.Desktop/validate_resw.ps1)
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [FileSystemHandler.cs](file://Agentic.Desktop/Agentic.Desktop/Services/FileSystemHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)
- [Agentic.Desktop.Tests.csproj](file://Agentic.Desktop.Tests/Agentic.Desktop.Tests.csproj)

## Conclusion
Agentic.Desktop provides robust development and testing tools:
- MockAgentTransport enables deterministic, scriptable ACP behavior for UI development and tests without external dependencies.
- LocalizationService centralizes string retrieval and formatting, supporting multiple cultures via .resw files.
- SettingsViewModel and ChatViewModel integrate seamlessly with both mock and real transports, enabling flexible testing scenarios.
- TerminalManager and DesktopFileSystemHandler add practical utilities for process management and secure file access.
- **New**: Comprehensive xUnit testing infrastructure ensures code quality through automated testing of core components.
- **New**: Four major test classes provide thorough coverage of models, services, utilities, and transport layers.
These components together facilitate unit testing, debugging, and performance optimization for WinUI 3 applications.

[No sources needed since this section summarizes without analyzing specific files]

## Appendices

### Creating Test Scenarios with MockAgentTransport
- Instantiate MockAgentTransport and wire it into AcpClient via SettingsViewModel.ConnectAsync with empty AgentPath.
- Subscribe to MessageReceived to assert emitted JSON messages and payloads.
- Trigger session/prompt and verify streaming updates and final stop reason.
- Validate cancellation by invoking session/cancel and checking that streaming stops.

**Section sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)

### Debugging Techniques for WinUI 3 Applications
- Use Visual Studio debugger with breakpoints in ViewModels and Services.
- Inspect DispatcherQueue operations to ensure UI thread marshaling.
- Log AcpClient initialization and session lifecycle events.
- Monitor TerminalManager stdout/stderr outputs for process diagnostics.
- **New**: Use xUnit test explorer for targeted debugging of failing tests.
- **New**: Leverage Visual Studio's integrated test runner for step-through debugging of test scenarios.

[No sources needed since this section provides general guidance]

### Performance Profiling Strategies
- Profile UI updates by measuring batching intervals in ChatViewModel.
- Use CPU/GPU profilers to identify bottlenecks in streaming handlers.
- Measure memory usage for large terminal outputs and ensure proper disposal.
- **New**: Use xUnit parallel test execution to identify performance bottlenecks in concurrent scenarios.
- **New**: Monitor test execution times to identify slow-running test cases that need optimization.

[No sources needed since this section provides general guidance]

### Extending MockAgentTransport for Custom Testing
- Add new methods in SendAsync switch cases to simulate additional ACP commands.
- Introduce configurable response scripts or fixtures for deterministic testing.
- Expose events or callbacks for test assertions on internal state changes.
- **New**: Add corresponding test cases in MockAgentTransportTests to validate new functionality.

**Section sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)

### Adding New Languages to the Localization System
- Create a new folder under Strings with the target culture code (e.g., fr).
- Copy and translate Resources.resw entries for the new culture.
- Ensure all keys match existing cultures to avoid missing string exceptions.
- Optionally run validate_resw.ps1 to verify XML integrity and entry counts.
- **New**: Add localization tests to verify string availability across all supported cultures.

**Section sources**
- [Resources.resw (en)](file://Agentic.Desktop/Agentic.Desktop/Strings/en/Resources.resw)
- [validate_resw.ps1](file://Agentic.Desktop/validate_resw.ps1)

### Unit Testing Approaches for ViewModels and Services
- **SettingsViewModel**:
  - Verify ConnectAsync constructs correct transport based on AgentPath.
  - Assert OnAgentConnected invokes with valid AcpClient instance.
  - Check DisconnectAsync resets state and disposes resources.
- **ChatViewModel**:
  - Bind a mock AcpClient and assert SendMessageAsync triggers expected prompts.
  - Validate OnSessionUpdated merges chunks correctly and updates UI properties.
  - Confirm CancelGenerationAsync clears streaming state.
- **Services**:
  - DesktopFileSystemHandler: Assert UnauthorizedAccessException for out-of-scope paths.
  - TerminalManager: Create terminals, read outputs, and verify cleanup on Dispose.
- **New**: **ChatMessage**: Test constructor parameters, property change notifications, and state management.
- **New**: **MarkdownHelper**: Verify HTML conversion and plain text extraction for various markdown syntax.
- **New**: **MockAgentTransport**: Validate state transitions, message handling, and streaming behavior.

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [FileSystemHandler.cs](file://Agentic.Desktop/Agentic.Desktop/Services/FileSystemHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Agentic.Desktop/Services/TerminalManager.cs)
- [ChatMessageTests.cs](file://Agentic.Desktop.Tests/ChatMessageTests.cs)
- [MarkdownHelperTests.cs](file://Agentic.Desktop.Tests/MarkdownHelperTests.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)

### xUnit Testing Best Practices
- **Test Organization**: Group related tests in logical classes with clear naming conventions.
- **Setup and Teardown**: Use constructors for setup and IDisposable for cleanup.
- **Assertions**: Use descriptive assertion messages and appropriate xUnit assertion methods.
- **Async Testing**: Properly handle async operations with await and timeout configurations.
- **Isolation**: Ensure tests don't interfere with each other through proper resource management.
- **Coverage**: Aim for high code coverage while focusing on meaningful test scenarios.

**Section sources**
- [ChatMessageTests.cs](file://Agentic.Desktop.Tests/ChatMessageTests.cs)
- [FileSystemHandlerTests.cs](file://Agentic.Desktop.Tests/FileSystemHandlerTests.cs)
- [MarkdownHelperTests.cs](file://Agentic.Desktop.Tests/MarkdownHelperTests.cs)
- [MockAgentTransportTests.cs](file://Agentic.Desktop.Tests/MockAgentTransportTests.cs)
- [Agentic.Desktop.Tests.csproj](file://Agentic.Desktop.Tests/Agentic.Desktop.Tests.csproj)