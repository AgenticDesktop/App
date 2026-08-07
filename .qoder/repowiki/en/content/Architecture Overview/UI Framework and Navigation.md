# UI Framework and Navigation

<cite>
**Referenced Files in This Document**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)
- [SettingsPage.xaml.cs](file://Agentic.Desktop/SettingsPage.xaml.cs)
- [SettingsPage.xaml](file://Agentic.Desktop/SettingsPage.xaml)
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [Agentic.Desktop.csproj](file://Agentic.Desktop/Agentic.Desktop.csproj)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)
- [ChatMessage.cs](file://Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/ViewModels/Messages/ChatSession.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [BoolToVisibilityConverter.cs](file://Agentic.Desktop/Converters/BoolToVisibilityConverter.cs)
- [MarkdownHelper.cs](file://Agentic.Desktop/Services/MarkdownHelper.cs)
- [ChatListPanel.xaml.cs](file://Agentic.Desktop/Views/ChatListPanel.xaml.cs)
- [ChatListPanel.xaml](file://Agentic.Desktop/Views/ChatListPanel.xaml)
- [PermissionDialog.xaml.cs](file://Agentic.Desktop/Views/PermissionDialog.xaml.cs)
- [PermissionDialog.xaml](file://Agentic.Desktop/Views/PermissionDialog.xaml)
- [Resources.resw](file://Agentic.Desktop/Strings/en/Resources.resw)
</cite>

## Update Summary
**Changes Made**
- Added comprehensive section on Uno Platform Host Integration and Cross-Platform Architecture
- Updated Application Bootstrap section to document dual entry points (Windows vs Desktop targets)
- Enhanced Window Handle Management section with conditional compilation for platform-specific initialization
- Added detailed explanation of Skia rendering backend support and platform-specific configurations
- Updated project structure diagrams to reflect multi-target framework architecture
- Added guidance on cross-platform development patterns and conditional compilation symbols

## Table of Contents
1. Introduction
2. Project Structure
3. Core Components
4. Architecture Overview
5. Detailed Component Analysis
6. Dependency Analysis
7. Performance Considerations
8. Troubleshooting Guide
9. Conclusion

## Introduction
This document explains the WinUI 3 framework implementation and navigation architecture for the desktop application with comprehensive cross-platform support through Uno Platform. The application now supports multiple target frameworks including Windows (WinAppSDK) and cross-platform desktop (Skia), enabling deployment across Windows, Linux, and macOS platforms. MainWindow serves as the main window with a navigation frame, while MainPage implements the primary chat interface with full internationalization support through x:Uid attribute binding.

## Project Structure
The application follows a clear separation between Views (XAML + code-behind), ViewModels (MVVM), Services (localization, markdown, permissions), and Converters (value conversion). The entry point now supports dual initialization paths - Windows uses XAML-generated Main while cross-platform targets use Uno Platform Host with Skia rendering backend.

```mermaid
graph TB
App["App<br/>Application lifecycle"] --> Entry["Entry Point<br/>Windows: XAML Main<br/>Desktop: Uno Program.Main"]
Entry --> Window["MainWindow<br/>Window + NavigationView + Frame"]
Window --> Nav["NavigationView<br/>Menu/Footer items"]
Window --> Frame["Frame<br/>RootFrame"]
Frame --> Main["MainPage<br/>Chat UI"]
Main --> VM["ChatViewModel<br/>Observable state + commands"]
Main --> CLVM["ChatListViewModel<br/>Sessions list"]
VM --> Msg["ChatMessage / ChatSession<br/>Data models"]
Main --> Conv["BoolToVisibilityConverter<br/>Value converter"]
VM --> Loc["LocalizationService<br/>.resw resources"]
VM --> MD["MarkdownHelper<br/>Markdown utilities"]
Loc --> Res["Resource Files<br/>en/ja/zh-CN/zh-TW"]
```

**Diagram sources**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)
- [ChatMessage.cs](file://Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/ViewModels/Messages/ChatSession.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [BoolToVisibilityConverter.cs](file://Agentic.Desktop/Converters/BoolToVisibilityConverter.cs)
- [MarkdownHelper.cs](file://Agentic.Desktop/Services/MarkdownHelper.cs)

**Section sources**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)

## Core Components
- **Application bootstrap**: Dual entry points - Windows uses XAML-generated Main, cross-platform targets use Uno Platform Host with Skia rendering backend. Initializes logging, creates the main window, and exposes global services such as the dispatcher queue and current ACP client.
- **Uno Platform Host Integration**: Configures cross-platform rendering backends including X11, Linux FrameBuffer, macOS, and Win32 for unified UI across different operating systems.
- **MainWindow**: Hosts TitleBar, NavigationView, and a Frame that navigates between pages. Provides connection status updates and navigation helpers with platform-specific window handle management.
- **MainPage**: Implements the chat UI using SplitView, ItemsRepeater, DataTemplateSelector, and binds to ChatViewModel. Handles keyboard input and scroll behavior.
- **Cross-Platform Window Handle Management**: Conditional compilation ensures proper HWND handling only on Windows platform while returning safe defaults on other platforms.
- **ViewModels**: ChatViewModel manages messages, streaming, session selection, and commands; ChatListViewModel manages sessions and selection.
- **Localization**: Centralized access to .resw strings via LocalizationService with comprehensive x:Uid attribute binding support.
- **Converters**: BoolToVisibilityConverter supports visibility toggling with optional inversion.

**Section sources**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [BoolToVisibilityConverter.cs](file://Agentic.Desktop/Converters/BoolToVisibilityConverter.cs)

## Architecture Overview
The app uses a standard WinUI 3 MVVM pattern with comprehensive internationalization support and cross-platform compatibility through Uno Platform:
- App orchestrates startup and global state with platform-specific initialization.
- Uno Platform Host provides cross-platform rendering backend abstraction.
- MainWindow provides navigation infrastructure and shared UI chrome with conditional platform features.
- MainPage composes the chat experience and binds to ViewModels.
- ViewModels encapsulate business logic and expose observable properties and commands.
- Services provide cross-cutting concerns like localization and markdown processing.
- Resource files enable multi-language support through x:Uid attribute binding.

```mermaid
sequenceDiagram
participant Uno as "Uno Platform Host"
participant App as "App"
participant Win as "MainWindow"
participant Nav as "NavigationView"
participant Frame as "Frame"
participant Page as "MainPage"
participant VM as "ChatViewModel"
participant Loc as "LocalizationService"
Uno->>App : Create() via UnoPlatformHostBuilder
App->>Win : new MainWindow()
Win->>Nav : Loaded event
Nav-->>Win : SelectionChanged
Win->>Frame : Navigate(typeof(MainPage))
Frame-->>Page : Construct MainPage
Page->>VM : Initialize ViewModel
Page->>Loc : Load localized resources via x : Uid
Page->>VM : Subscribe to ScrollToBottom
Page->>VM : Bind Client if available
Note over Page,VM : Two-way bindings and commands drive UI updates
```

**Diagram sources**
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)

## Detailed Component Analysis

### Uno Platform Host Integration and Cross-Platform Architecture
**Updated** The application now supports cross-platform deployment through Uno Platform Host with Skia rendering backend. The entry point configuration enables deployment across Windows, Linux, and macOS platforms.

- **Dual Entry Points**: Windows target uses XAML-generated Main, while cross-platform targets use `Program.Main` with Uno Platform Host.
- **Rendering Backend Configuration**: Supports X11 (Linux), Linux FrameBuffer, macOS native, and Win32 backends through Uno.UI.Hosting.
- **Multi-Target Framework Support**: Project targets both `net10.0-windows10.0.26100` and `net10.0-desktop` with conditional compilation.
- **Platform-Specific Package Management**: Windows-only packages (Microsoft.WindowsAppSDK) are conditionally included based on target framework.

```mermaid
flowchart TD
Build["MSBuild Process"] --> Detect["Detect Target Framework"]
Detect --> |windows| WinPath["Windows Path<br/>XAML Generated Main"]
Detect --> |desktop| UnoPath["Desktop Path<br/>Uno Platform Host"]
WinPath --> WinApp["Windows App SDK<br/>Native Features"]
UnoPath --> UnoHost["UnoPlatformHostBuilder<br/>Create().Build()"]
UnoHost --> Backends["Configure Backends<br/>UseX11(), UseLinuxFrameBuffer()<br/>UseMacOS(), UseWin32()"]
Backends --> Run["host.Run()"]
```

**Diagram sources**
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [Agentic.Desktop.csproj](file://Agentic.Desktop/Agentic.Desktop.csproj)

**Section sources**
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [Agentic.Desktop.csproj](file://Agentic.Desktop/Agentic.Desktop.csproj)

### Application Bootstrap and Window Handle Management
**Updated** The application now features platform-specific window handle management with conditional initialization for cross-platform compatibility.

- **Conditional Compilation**: Uses `#if WINDOWS` directives to ensure platform-specific code only executes on Windows target.
- **Safe Window Handle Access**: Returns `nint.Zero` on non-Windows platforms to prevent runtime errors.
- **Dispatcher Queue Initialization**: Properly initializes UI thread dispatcher for cross-platform scenarios.
- **Logging Configuration**: Sets up Microsoft.Extensions.Logging with debug output for all platforms.

```mermaid
classDiagram
class App {
+static Window Window
+static DispatcherQueue DispatcherQueue
+static nint WindowHandle
+static IAcpClient CurrentAcpClient
+OnLaunched(args)
}
class MainWindow {
+SystemBackdrop SystemBackdrop
+ExtendsContentIntoTitleBar
+AppWindow AppWindow
+UpdateConnectionStatus(state, agentName)
}
App --> MainWindow : creates
MainWindow --> App : uses WindowHandle
```

**Diagram sources**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)

**Section sources**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)

### MainWindow: Window and Navigation Frame Management
- Extends content into the title bar and sets custom icon and size.
- Displays connection status dot and localized text using x:Uid attributes.
- Uses NavigationView to switch between Chat and Settings pages via tags.
- Exposes methods to navigate to settings and update connection status safely on the UI thread.
- **Platform-Specific Features**: Conditionally applies Mica backdrop effect only on Windows platform.

```mermaid
flowchart TD
Start(["MainWindow Loaded"]) --> DefaultItem["Select default menu item"]
DefaultItem --> NavigateChat["Navigate RootFrame to MainPage"]
UserClick{"User clicks nav item?"}
UserClick --> |chat| GoChat["Navigate to MainPage"]
UserClick --> |settings| GoSettings["Navigate to SettingsPage"]
UpdateStatus{"UpdateConnectionStatus(state, agentName)"}
UpdateStatus --> SetDot["Set StatusDot color"]
UpdateStatus --> SetText["Set StatusText localized string"]
```

**Diagram sources**
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)

**Section sources**
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)

### MainPage: Primary Chat Interface
- Composes a SplitView with a sidebar (ChatListPanel) and message area.
- Binds ItemsSource to ViewModel.Messages and uses a DataTemplateSelector to render user vs agent messages.
- Handles Enter key to send messages and toggles sidebar visibility.
- Subscribes to ViewModel.ScrollToBottom and scrolls after layout.
- All UI text uses x:Uid attributes for automatic localization.

```mermaid
classDiagram
class MainPage {
+ChatViewModel ViewModel
+OnAcpClientChanged(client)
+OpenSettings_Click(sender,e)
+InputTextBox_KeyDown(sender,e)
+ToggleSidebar_Click(sender,e)
+ScrollToBottom()
}
class ChatMessageTemplateSelector {
+UserTemplate
+AgentTemplate
+SelectTemplateCore(item) DataTemplate?
}
MainPage --> ChatMessageTemplateSelector : "uses"
```

**Diagram sources**
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)

**Section sources**
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)

### ViewModels: Data Binding and Commands
- ChatViewModel:
  - Observable properties for input text, connection state, streaming state, and current agent message.
  - Manages session changes and subscribes to collection changes to auto-scroll.
  - Sends messages via IAcpClient or simulates mock responses when not connected.
  - Merges streaming chunks with batching to reduce UI churn.
- ChatListViewModel:
  - Maintains a list of sessions and selected session.
  - Provides commands to create, delete, and select sessions.

```mermaid
classDiagram
class ChatViewModel {
+string InputText
+bool IsAgentResponding
+bool IsAgentConnected
+ChatMessage CurrentAgentMessage
+ChatListViewModel ChatList
+ObservableCollection~ChatMessage~ Messages
+BindClient(client)
+ClearMessages()
+SendMessageAsync()
+CancelGenerationAsync()
+OnSessionUpdated(update)
}
class ChatListViewModel {
+ObservableCollection~ChatSession~ Sessions
+ChatSession SelectedSession
+CreateNewChat()
+DeleteChat(session)
+SelectChat(session)
}
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
ChatViewModel --> ChatListViewModel : "contains"
ChatListViewModel --> ChatSession : "manages"
ChatSession --> ChatMessage : "contains"
```

**Diagram sources**
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)
- [ChatMessage.cs](file://Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/ViewModels/Messages/ChatSession.cs)

**Section sources**
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)
- [ChatMessage.cs](file://Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/ViewModels/Messages/ChatSession.cs)

### Settings Integration and Permissions
- SettingsPage configures permission and file system handlers for the ACP client.
- Updates connection status in MainWindow's title bar and notifies App-level client changes.
- Uses FolderPicker with HWND initialization for WinRT interop.
- All interface text is fully localized through x:Uid attributes.

```mermaid
sequenceDiagram
participant SP as "SettingsPage"
participant VM as "SettingsViewModel"
participant App as "App"
participant MW as "MainWindow"
SP->>VM : OnAgentConnected(client)
SP->>SP : Configure PermissionHandler and FileSystemHandler
SP->>MW : UpdateConnectionStatus(state, agentName)
SP->>App : SetAcpClient(client)
App-->>MainPage : AcpClientChanged event
MainPage->>VM : BindClient(client)
```

**Diagram sources**
- [SettingsPage.xaml.cs](file://Agentic.Desktop/SettingsPage.xaml.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)

**Section sources**
- [SettingsPage.xaml.cs](file://Agentic.Desktop/SettingsPage.xaml.cs)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)

### Localization and Resources
**Updated** The application now features comprehensive internationalization support through x:Uid attribute binding in XAML files. All interface text has been localized including SettingsPage.xaml, MainPage.xaml, MainWindow.xaml, ChatListPanel.xaml, and PermissionDialog.xaml.

- **x:Uid Attribute Binding**: Every UI element uses x:Uid attributes for automatic localization, eliminating the need for manual text assignment in code-behind.
- **Resource Files**: Complete localization support in four languages (English, Japanese, Simplified Chinese, Traditional Chinese) through .resw files.
- **LocalizationService**: Centralized service providing programmatic access to localized strings with formatting support.
- **Automatic Language Detection**: Windows automatically selects the appropriate language based on system locale settings.

```mermaid
flowchart TD
XamlElement["XAML Element with x:Uid"] --> ResourceLoader["Windows Resource Loader"]
ResourceLoader --> ReswFile[".resw Resource File"]
ReswFile --> CurrentLocale["Current System Locale"]
CurrentLocale --> LocalizedText["Localized String"]
LocalizeCode["LocalizationService.Get(key)"] --> ResourceLoader
FormatCode["LocalizationService.Format(key, args)"] --> ResourceLoader
```

**Diagram sources**
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)
- [SettingsPage.xaml](file://Agentic.Desktop/SettingsPage.xaml)
- [ChatListPanel.xaml](file://Agentic.Desktop/Views/ChatListPanel.xaml)
- [PermissionDialog.xaml](file://Agentic.Desktop/Views/PermissionDialog.xaml)

**Section sources**
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [MainPage.xaml](file://Agentic.Desktop/MainPage.xaml)
- [MainWindow.xaml](file://Agentic.Desktop/MainWindow.xaml)
- [SettingsPage.xaml](file://Agentic.Desktop/SettingsPage.xaml)
- [ChatListPanel.xaml](file://Agentic.Desktop/Views/ChatListPanel.xaml)
- [PermissionDialog.xaml](file://Agentic.Desktop/Views/PermissionDialog.xaml)
- [MarkdownHelper.cs](file://Agentic.Desktop/Services/MarkdownHelper.cs)

### Sidebar Panel and Command Execution
- ChatListPanel exposes a ViewModel property and triggers SelectChat/DeleteChat commands based on user interactions.
- All UI text uses x:Uid attributes for localization including titles, button tooltips, and labels.

```mermaid
sequenceDiagram
participant Panel as "ChatListPanel"
participant VM as "ChatListViewModel"
Panel->>VM : SelectChatCommand.Execute(session)
Panel->>VM : DeleteChatCommand.Execute(session)
```

**Diagram sources**
- [ChatListPanel.xaml.cs](file://Agentic.Desktop/Views/ChatListPanel.xaml.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)

**Section sources**
- [ChatListPanel.xaml.cs](file://Agentic.Desktop/Views/ChatListPanel.xaml.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)

## Dependency Analysis
- App depends on Microsoft.Extensions.Logging and WinUI types; it creates MainWindow and exposes DispatcherQueue and AcpClient.
- Uno Platform Host depends on Uno.UI.Hosting for cross-platform rendering backend abstraction.
- MainWindow depends on WinUI controls (TitleBar, NavigationView, Frame) and LocalizationService for status text.
- MainPage depends on ViewModels and Converters; it coordinates UI events and scrolling.
- ChatViewModel depends on IAcpClient, CommunityToolkit.Mvvm, and services for localization and markdown.
- SettingsPage integrates with WinRT interop (FolderPicker, InitializeWithWindow) and updates App and MainWindow.
- All XAML components depend on .resw resource files for localization through x:Uid attributes.

```mermaid
graph LR
App["App"] --> MainWindow["MainWindow"]
UnoHost["Uno Platform Host"] --> App
MainWindow --> MainPage["MainPage"]
MainPage --> ChatViewModel["ChatViewModel"]
MainPage --> ChatListViewModel["ChatListViewModel"]
ChatViewModel --> ChatMessage["ChatMessage"]
ChatViewModel --> ChatSession["ChatSession"]
ChatViewModel --> LocalizationService["LocalizationService"]
ChatViewModel --> MarkdownHelper["MarkdownHelper"]
SettingsPage["SettingsPage"] --> App
SettingsPage --> MainWindow
LocalizationService --> Resources[".resw Files"]
```

**Diagram sources**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)
- [ChatMessage.cs](file://Agentic.Desktop/ViewModels/Messages/ChatMessage.cs)
- [ChatSession.cs](file://Agentic.Desktop/ViewModels/Messages/ChatSession.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [MarkdownHelper.cs](file://Agentic.Desktop/Services/MarkdownHelper.cs)
- [SettingsPage.xaml.cs](file://Agentic.Desktop/SettingsPage.xaml.cs)

**Section sources**
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [ChatViewModel.cs](file://Agentic.Desktop/ViewModels/ChatViewModel.cs)
- [ChatListViewModel.cs](file://Agentic.Desktop/ViewModels/ChatListViewModel.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [MarkdownHelper.cs](file://Agentic.Desktop/Services/MarkdownHelper.cs)
- [SettingsPage.xaml.cs](file://Agentic.Desktop/SettingsPage.xaml.cs)

## Performance Considerations
- Streaming message updates are batched with a short delay to minimize UI churn during rapid chunk arrivals.
- Auto-scroll is scheduled on the UI thread using DispatcherQueue.TryEnqueue to avoid layout conflicts.
- ItemsRepeater efficiently renders large lists of messages compared to traditional ListView approaches.
- Avoid unnecessary property changes by clearing messages only on disconnect and updating session previews sparingly.
- Resource loading is optimized through Windows' built-in caching mechanism for .resw files.
- **Cross-Platform Optimization**: Uno Platform Host manages rendering backend performance automatically across different platforms.

## Troubleshooting Guide
- Connection status not updating: Ensure UpdateConnectionStatus is called from the UI thread and that MainWindow has initialized the TitleBar content.
- Messages not scrolling: Verify ScrollToBottom is invoked on collection changes and that MessageScroller has completed layout before scrolling.
- Localization missing: Confirm .resw files exist under Strings directory and keys match LocalizationService.Get calls and x:Uid attributes.
- Settings dialog issues: Ensure InitializeWithWindow is called with the correct HWND before showing pickers or dialogs.
- Language switching problems: Verify system locale settings and ensure all required .resw files are included in the build output.
- **Cross-Platform Issues**: Check Uno Platform Host configuration and ensure appropriate rendering backends are enabled for target platform.
- **Window Handle Problems**: Verify conditional compilation symbols and ensure platform-specific code paths are properly handled.

**Section sources**
- [MainWindow.xaml.cs](file://Agentic.Desktop/MainWindow.xaml.cs)
- [MainPage.xaml.cs](file://Agentic.Desktop/MainPage.xaml.cs)
- [SettingsPage.xaml.cs](file://Agentic.Desktop/SettingsPage.xaml.cs)
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [Program.cs](file://Agentic.Desktop/Platforms/Desktop/Program.cs)

## Conclusion
The application leverages WinUI 3 and MVVM to deliver a responsive chat interface with robust navigation, comprehensive internationalization support, and cross-platform compatibility through Uno Platform. MainWindow centralizes navigation and window chrome, while MainPage binds tightly to ViewModels for state-driven UI updates. The extensive use of x:Uid attribute binding ensures all user-facing text is properly localized across multiple languages. Uno Platform Host integration enables deployment across Windows, Linux, and macOS with appropriate rendering backends. Services and converters encapsulate cross-cutting concerns, and integration points with Windows App SDK enable native features like file pickers and permissions. This structure promotes maintainability, scalability, and a clear separation of concerns across the UI layer while providing seamless multilingual support and cross-platform deployment capabilities.