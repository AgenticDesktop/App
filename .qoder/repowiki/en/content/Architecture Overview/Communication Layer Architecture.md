# Communication Layer Architecture

<cite>
**Referenced Files in This Document**
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Services/TerminalManager.cs)
- [ChatMessage.cs](file://Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [Agentic.Desktop.csproj](file://Agentic.Desktop/Agentic.Desktop.csproj)
</cite>

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Dependency Analysis](#dependency-analysis)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Conclusion](#conclusion)

## Introduction
This document explains the communication layer that abstracts agent interactions through a transport abstraction and a unified client interface. The AcpClient provides a single API for both real stdio-based agents and mock implementations used during development and testing. The design uses an event-driven architecture to handle connection state changes, streaming messages, and permission requests. ChatViewModel demonstrates how the UI consumes the AcpClient for real-time messaging, while MockAgentTransport simulates agent behavior to enable rapid iteration without external dependencies.

## Project Structure
The communication layer spans ViewModels, Services, and a Mock implementation:
- SettingsViewModel orchestrates transport selection (mock vs stdio), client initialization, session creation, and global connection state propagation.
- ChatViewModel binds to IAcpClient to send prompts, receive streaming updates, and manage UI state.
- App exposes a global current client and events to notify UI layers on connection changes.
- Services implement platform-specific handlers for permissions and terminal processes.
- MockAgentTransport implements the transport interface to simulate agent responses for local development.

```mermaid
graph TB
subgraph "UI Layer"
MainPage["MainPage.xaml.cs"]
ChatVM["ChatViewModel.cs"]
SettingsVM["SettingsViewModel.cs"]
end
subgraph "Application Core"
AppCore["App.xaml.cs"]
end
subgraph "Communication Layer"
AcpClient["AcpClient (external library)"]
Transport["IAgentTransport (external interface)"]
Stdio["StdioAgentTransport (external)"]
Mock["MockAgentTransport.cs"]
end
subgraph "Services"
Perm["DesktopPermissionHandler.cs"]
Term["TerminalManager.cs"]
end
MainPage --> ChatVM
MainPage --> AppCore
SettingsVM --> AcpClient
SettingsVM --> Transport
AcpClient --> Transport
Transport --> Stdio
Transport --> Mock
AcpClient --> Perm
AcpClient --> Term
AppCore --> ChatVM
```

**Diagram sources**
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Services/TerminalManager.cs)

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Services/TerminalManager.cs)

## Core Components
- IAgentTransport: Abstraction for underlying communication mechanisms (stdio or mock). Implementations encapsulate start/stop lifecycle, message sending, and transport-level events.
- AcpClient: Unified client over IAgentTransport with JSON-RPC dispatching. It initializes sessions, sends prompts, handles streaming updates, and manages lifecycle events.
- MockAgentTransport: In-memory transport that responds to initialize, session/new, session/prompt, and session/cancel with scripted JSON-RPC messages and streaming chunks.
- DesktopPermissionHandler: Bridges agent permission requests to the UI thread, showing dialogs and returning user decisions asynchronously.
- TerminalManager: Manages multiple terminal processes, capturing stdout/stderr and providing lifecycle control.

Key responsibilities:
- Connection lifecycle: connect/disconnect, initialize, create session, shutdown.
- Message flow: send prompts, stream updates, cancel operations.
- Permission handling: request approval from the user before executing sensitive actions.
- Terminal integration: spawn and manage shell processes for tool execution.

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Services/TerminalManager.cs)

## Architecture Overview
The communication layer follows a transport abstraction pattern:
- SettingsViewModel selects a transport based on configuration (empty path implies mock; otherwise stdio).
- AcpClient is constructed with the selected transport, a JSON-RPC dispatcher, and optional logging.
- ChatViewModel binds to IAcpClient to send prompts and observe SessionUpdated events for streaming content.
- App maintains a global current client and raises AcpClientChanged to synchronize UI state across pages.

```mermaid
sequenceDiagram
participant User as "User"
participant Settings as "SettingsViewModel"
participant App as "App"
participant Main as "MainPage"
participant Chat as "ChatViewModel"
participant Client as "AcpClient"
participant Transport as "IAgentTransport"
User->>Settings : ConnectAsync()
Settings->>Settings : Select transport (Mock or Stdio)
Settings->>Client : new AcpClient(transport, dispatcher, logger)
Settings->>Client : InitializeAsync()
Client->>Transport : StartAsync()
Transport-->>Client : State=Running
Settings->>Client : CreateSessionAsync(workDir)
Client-->>Settings : SessionId
Settings->>App : SetAcpClient(client)
App-->>Main : AcpClientChanged(client)
Main->>Chat : BindClient(client)
User->>Chat : SendMessageAsync(text)
Chat->>Client : SendPromptAsync(sessionId, prompt)
Client->>Transport : SendAsync(jsonLine)
Transport-->>Client : session/update chunks
Client-->>Chat : SessionUpdated(chunk)
Chat-->>User : Streamed text appears
```

**Diagram sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)

## Detailed Component Analysis

### Transport Abstraction Pattern
IAgentTransport defines the contract for all transports. The application chooses between:
- StdioAgentTransport: Real stdio-based communication with an external agent process.
- MockAgentTransport: Deterministic, scripted responses for development and testing.

```mermaid
classDiagram
class IAgentTransport {
+State : TransportState
+StartAsync(cancellationToken) Task
+SendAsync(jsonLine, cancellationToken) Task
+StopAsync() Task
+MessageReceived(string) event
+TransportFaulted(Exception) event
+ProcessExited(int) event
}
class StdioAgentTransport {
+StartAsync() Task
+SendAsync() Task
+StopAsync() Task
}
class MockAgentTransport {
-_requestId : int
-_state : TransportState
-_promptCts : CancellationTokenSource
+StartAsync() Task
+SendAsync() Task
+StopAsync() Task
-FireMessageAsync(json) Task
-BuildResponse(id, result) string
}
IAgentTransport <|.. StdioAgentTransport
IAgentTransport <|.. MockAgentTransport
```

**Diagram sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)

### AcpClient Usage in ChatViewModel
ChatViewModel binds to IAcpClient to send prompts and render streamed responses:
- On user input, it creates a placeholder agent message and sets streaming state.
- It calls SendPromptAsync with the current session ID and a text content block.
- SessionUpdated events are handled to append text chunks to the current agent message, with batching to reduce UI churn.
- Cancellation is supported via CancelGenerationAsync which invokes CancelSessionAsync.

```mermaid
sequenceDiagram
participant UI as "MainPage"
participant Chat as "ChatViewModel"
participant Client as "AcpClient"
participant Transport as "IAgentTransport"
UI->>Chat : InputText changed
Chat->>Chat : SendMessageAsync()
Chat->>Chat : Add user message and agent placeholder
Chat->>Client : SendPromptAsync(sessionId, [TextContent])
Client->>Transport : SendAsync("session/prompt")
Transport-->>Client : "session/update" chunks
Client-->>Chat : SessionUpdated(AgentMessageChunk)
Chat->>Chat : Append chunk to CurrentAgentMessage
Chat-->>UI : Streaming indicator updates
```

**Diagram sources**
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)

**Section sources**
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatMessage.cs](file://Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)

### Mock Agent Behavior Simulation
MockAgentTransport simulates a full ACP conversation:
- initialize returns agent capabilities and info.
- session/new returns a fixed session ID.
- session/prompt streams multiple text chunks with delays, then returns a final stopReason.
- session/cancel cancels any in-flight prompt using a linked cancellation token.

```mermaid
flowchart TD
Start(["SendAsync(jsonLine)"]) --> Parse["Parse JSON method/id"]
Parse --> Method{"method"}
Method --> |initialize| InitResp["Build response with agentInfo"]
Method --> |session/new| NewResp["Build response with sessionId"]
Method --> |session/prompt| PromptFlow["Stream chunks<br/>then final response"]
Method --> |session/cancel| CancelFlow["Cancel in-flight prompt"]
Method --> |default| DefaultResp["Empty response"]
PromptFlow --> Delay["Task.Delay per chunk"]
Delay --> Emit["Emit session/update notification"]
Emit --> NextChunk{"More chunks?"}
NextChunk --> |Yes| Delay
NextChunk --> |No| FinalResp["Build stopReason response"]
CancelFlow --> CTS["Cancel linked token"]
CTS --> End(["Done"])
InitResp --> End
NewResp --> End
DefaultResp --> End
```

**Diagram sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)

**Section sources**
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)

### Event-Driven Connection State and Messaging
- SettingsViewModel manages connection lifecycle and emits OnAgentConnected when ready.
- App.SetAcpClient updates the global client and raises AcpClientChanged.
- MainPage subscribes to AcpClientChanged and binds/unbinds ChatViewModel accordingly.
- ChatViewModel subscribes to AcpClient.SessionUpdated to update messages in real time.

```mermaid
sequenceDiagram
participant Settings as "SettingsViewModel"
participant App as "App"
participant Main as "MainPage"
participant Chat as "ChatViewModel"
Settings->>App : SetAcpClient(client)
App-->>Main : AcpClientChanged(client)
Main->>Chat : BindClient(client)
Chat->>Chat : Subscribe to SessionUpdated
Chat-->>Main : ScrollToBottom on message changes
```

**Diagram sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)

### Permission Requests and Terminal Integration
- DesktopPermissionHandler receives permission requests from the agent and marshals them to the UI thread.
- ViewModel shows a dialog and completes the request with the user’s decision.
- TerminalManager spawns shell processes, captures output streams, and supports lifecycle operations like kill/release/waitForExit.

```mermaid
sequenceDiagram
participant Agent as "Agent Process"
participant Client as "AcpClient"
participant Perm as "DesktopPermissionHandler"
participant UI as "ViewModel"
Agent->>Client : RequestPermissionRequest
Client->>Perm : HandlePermissionRequestAsync(request)
Perm->>UI : PermissionRequested(args)
UI-->>Perm : OnComplete(response)
Perm-->>Client : RequestPermissionResponse
Client-->>Agent : Response
```

**Diagram sources**
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)

**Section sources**
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Services/TerminalManager.cs)

## Dependency Analysis
The project depends on an external ACP library for the core client and transport abstractions. The dependency graph highlights how the desktop app composes these components:

```mermaid
graph TB
CSProj["Agentic.Desktop.csproj"]
ACPLib["ShihaoShen.Agentic.ACPLibrary (NuGet)"]
SettingsVM["SettingsViewModel.cs"]
ChatVM["ChatViewModel.cs"]
MockT["MockAgentTransport.cs"]
PermH["DesktopPermissionHandler.cs"]
TermM["TerminalManager.cs"]
CSProj --> ACPLib
SettingsVM --> ACPLib
ChatVM --> ACPLib
SettingsVM --> MockT
SettingsVM --> PermH
SettingsVM --> TermM
```

**Diagram sources**
- [Agentic.Desktop.csproj](file://Agentic.Desktop/Agentic.Desktop.csproj)
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [MockAgentTransport.cs](file://Agentic.Desktop/Mocks/MockAgentTransport.cs)
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Services/TerminalManager.cs)

**Section sources**
- [Agentic.Desktop.csproj](file://Agentic.Desktop/Agentic.Desktop.csproj)

## Performance Considerations
- Streaming batching: ChatViewModel batches incoming text chunks to minimize UI updates, reducing rendering overhead during high-frequency streaming.
- Dispatcher marshalling: All UI updates are dispatched to the UI thread to avoid cross-thread exceptions and ensure smooth rendering.
- Cancellation support: Both mock and real transports respect cancellation tokens, allowing responsive user-initiated cancellations.
- Resource cleanup: Proper disposal of terminal processes and client shutdown prevents resource leaks and ensures stable long-running sessions.

[No sources needed since this section provides general guidance]

## Troubleshooting Guide
Common issues and resolutions:
- No agent path configured: The app falls back to MockAgentTransport automatically. Verify settings if you intended to connect to a real agent.
- Connection failures: Check error messages in connection status and ensure the agent executable exists and arguments are correct.
- Permission dialog not appearing: Ensure DesktopPermissionHandler is wired to the ViewModel and the UI thread dispatcher is available.
- Terminal output missing: Confirm TerminalManager is assigned to AcpClient.TerminalHandler and that working directory paths are valid.

**Section sources**
- [SettingsViewModel.cs](file://Agentic.Desktop/ViewModels/SettingsViewModel.cs)
- [PermissionHandler.cs](file://Agentic.Desktop/Services/PermissionHandler.cs)
- [TerminalManager.cs](file://Agentic.Desktop/Services/TerminalManager.cs)

## Conclusion
The communication layer leverages a transport abstraction to unify stdio-based and mock agent interactions behind a consistent AcpClient interface. An event-driven architecture enables seamless connection state management, streaming message updates, and permission handling. ChatViewModel demonstrates real-time messaging consumption, while MockAgentTransport facilitates rapid development and testing without external dependencies. This design promotes flexibility, testability, and maintainability across different deployment scenarios.

[No sources needed since this section summarizes without analyzing specific files]