# Configuration and Deployment

<cite>
**Referenced Files in This Document**
- [Agentic.Desktop.csproj](file://Agentic.Desktop/Agentic.Desktop.csproj)
- [Package.appxmanifest](file://Agentic.Desktop/Package.appxmanifest)
- [app.manifest](file://Agentic.Desktop/app.manifest)
- [Directory.Build.props](file://Directory.Build.props)
- [global.json](file://global.json)
- [launchSettings.json](file://Agentic.Desktop/Properties/launchSettings.json)
- [App.xaml.cs](file://Agentic.Desktop/App.xaml.cs)
- [dependabot.yml](file://.github/dependabot.yml)
- [LocalizationService.cs](file://Agentic.Desktop/Services/LocalizationService.cs)
- [Resources.resw](file://Agentic.Desktop/Strings/en/Resources.resw)
</cite>

## Update Summary
**Changes Made**
- Added GitHub Dependabot configuration for automated NuGet package dependency management
- Enhanced project configuration to support comprehensive localization infrastructure with multiple language resources
- Updated dependency analysis to include new automation and localization capabilities

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
10. [Appendices](#appendices)

## Introduction
This document explains how Agentic.Desktop is configured for build, packaging, and deployment on Windows. It covers the .NET project configuration (target framework, platform targets, and NuGet dependencies), MSIX packaging via Package.appxmanifest, system-level settings in app.manifest, build options, signing procedures, distribution guidance, environment variables, configuration file formats, runtime behavior, troubleshooting steps, and validation checks to ensure a successful installation. The project now includes automated dependency management through GitHub Dependabot and comprehensive localization support for multiple languages.

## Project Structure
At a high level, the project uses:
- A .NET WinUI 3 desktop application with MSIX packaging enabled
- Global SDK and language features controlled by global.json and Directory.Build.props
- Visual Studio launch profiles for packaged and unpackaged debugging
- GitHub Dependabot for automated NuGet package updates
- Comprehensive localization infrastructure with resource files for multiple languages

```mermaid
graph TB
subgraph "Build and Packaging"
CSProj["Agentic.Desktop.csproj"]
BuildProps["Directory.Build.props"]
GlobalJSON["global.json"]
AppXManifest["Package.appxmanifest"]
AppManifest["app.manifest"]
end
subgraph "Automation and Localization"
Dependabot[".github/dependabot.yml"]
LocalizationService["LocalizationService.cs"]
Resources["Resources.resw (en, ja, zh-CN, zh-TW)"]
end
subgraph "Runtime and Launch"
LaunchSettings["launchSettings.json"]
AppCode["App.xaml.cs"]
end
CSProj --> AppXManifest
CSProj --> AppManifest
CSProj --> LocalizationService
BuildProps --> CSProj
GlobalJSON --> CSProj
Dependabot --> CSProj
LaunchSettings --> AppCode
```

**Diagram sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)
- [Directory.Build.props:1-8](file://Directory.Build.props#L1-L8)
- [global.json:1-11](file://global.json#L1-L11)
- [Package.appxmanifest:1-57](file://Agentic.Desktop/Package.appxmanifest#L1-L57)
- [app.manifest:1-19](file://Agentic.Desktop/app.manifest#L1-L19)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)
- [launchSettings.json:1-10](file://Agentic.Desktop/Properties/launchSettings.json#L1-L10)

**Section sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)
- [Directory.Build.props:1-8](file://Directory.Build.props#L1-L8)
- [global.json:1-11](file://global.json#L1-L11)
- [Package.appxmanifest:1-57](file://Agentic.Desktop/Package.appxmanifest#L1-L57)
- [app.manifest:1-19](file://Agentic.Desktop/app.manifest#L1-L19)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)
- [launchSettings.json:1-10](file://Agentic.Desktop/Properties/launchSettings.json#L1-L10)

## Core Components
- .NET project configuration (Agentic.Desktop.csproj):
  - Target frameworks net10.0-windows10.0.26100 and net10.0-desktop with minimum OS version 10.0.17763.0
  - DefaultLanguage set to 'en' for localization support
  - MSIX tooling enabled and WinUI integration
  - NuGet packages for WinAppSDK, logging, MVVM toolkit, Markdown parsing, and agent library
- MSIX manifest (Package.appxmanifest):
  - Identity, display properties, resources, target device families
  - Application entry point and visual elements
  - Capabilities including full trust and system AI models
  - Multi-language resource declarations (en, zh-CN, zh-TW, ja)
- System manifest (app.manifest):
  - Compatibility declarations and DPI awareness
- Automation and localization:
  - GitHub Dependabot for automated NuGet package updates
  - Comprehensive localization service with .resw resource files
  - LocalizationService class for accessing localized strings

**Section sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)
- [Package.appxmanifest:1-57](file://Agentic.Desktop/Package.appxmanifest#L1-L57)
- [app.manifest:1-19](file://Agentic.Desktop/app.manifest#L1-L19)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)

## Architecture Overview
The build and packaging pipeline integrates .NET SDK, WinUI 3, and MSIX tooling to produce an installable package. The application declares capabilities and resources required at runtime. Automated dependency management ensures security updates are applied regularly through GitHub Dependabot.

```mermaid
graph TB
Dev["Developer Machine"]
DotNet["dotnet CLI / MSBuild"]
WinAppSDK["Windows App SDK"]
MSIXTooling["MSIX Packaging Tools"]
Dependabot["GitHub Dependabot"]
AppX["Package.appxmanifest"]
Manifest["app.manifest"]
Localization["Localization Resources"]
Output["MSIX Package / Published Artifacts"]
Dev --> DotNet
DotNet --> WinAppSDK
DotNet --> MSIXTooling
Dependabot --> DotNet
MSIXTooling --> AppX
MSIXTooling --> Manifest
MSIXTooling --> Localization
MSIXTooling --> Output
```

**Diagram sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)
- [Package.appxmanifest:1-57](file://Agentic.Desktop/Package.appxmanifest#L1-L57)
- [app.manifest:1-19](file://Agentic.Desktop/app.manifest#L1-L19)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)

## Detailed Component Analysis

### .NET Project Configuration (Agentic.Desktop.csproj)
Key aspects:
- TargetFrameworks set to net10.0-windows10.0.26100 and net10.0-desktop with TargetPlatformMinVersion 10.0.17763.0
- DefaultLanguage configured as 'en' for localization support
- MSIX tooling enabled; WinUI integration active
- Content assets included for splash screen, icons, and store logo
- NuGet packages:
  - Microsoft.WindowsAppSDK and related build tools
  - CommunityToolkit.Mvvm for MVVM patterns
  - Microsoft.Extensions.Logging.Debug for debug logging
  - Markdig for Markdown processing
  - ShihaoShen.Agentic.ACPLibrary for agent communication
- Publish optimizations:
  - ReadyToRun and trimming enabled in non-Debug configurations

```mermaid
flowchart TD
Start(["Build Start"]) --> Resolve["Resolve NuGet Packages"]
Resolve --> Compile["Compile WinUI App"]
Compile --> Assets["Include Content Assets"]
Assets --> Pack["Enable MSIX Tooling"]
Pack --> Optimize{"Configuration == Debug?"}
Optimize --> |Yes| NoOpt["Disable ReadyToRun and Trimming"]
Optimize --> |No| EnableOpt["Enable ReadyToRun and Trimming"]
NoOpt --> Output["Produce Artifacts"]
EnableOpt --> Output
```

**Diagram sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)

**Section sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)

### MSIX Packaging Configuration (Package.appxmanifest)
Highlights:
- Identity includes Name, Publisher, Version
- Properties define DisplayName, PublisherDisplayName, Logo
- Dependencies target Windows.Universal and Windows.Desktop families with min/max versions
- Resources declare supported languages (en, zh-CN, zh-TW, ja)
- Application entry points and VisualElements define UI branding and splash
- Capabilities:
  - runFullTrust allows full-trust execution
  - systemAIModels enables access to system AI model capabilities

```mermaid
sequenceDiagram
participant User as "User"
participant Installer as "Windows Installer"
participant MSIX as "MSIX Package"
participant App as "Agentic.Desktop.exe"
participant Manifest as "Package.appxmanifest"
participant Localization as "Localization Resources"
User->>Installer : Install MSIX
Installer->>MSIX : Validate Package
MSIX-->>Installer : OK
Installer->>Manifest : Read Capabilities and Resources
Installer->>Localization : Load Language Resources
Installer-->>User : Installation Complete
User->>App : Launch App
App->>Manifest : Verify Permissions
App->>Localization : Get Localized Strings
App-->>User : Run with Full Trust and Localized UI
```

**Diagram sources**
- [Package.appxmanifest:1-57](file://Agentic.Desktop/Package.appxmanifest#L1-L57)

**Section sources**
- [Package.appxmanifest:1-57](file://Agentic.Desktop/Package.appxmanifest#L1-L57)

### Windows Application Manifest (app.manifest)
System-level configuration:
- Assembly identity and compatibility declarations for Windows 10+
- DPI awareness set to PerMonitorV2 for correct scaling across monitors

```mermaid
flowchart TD
Load["Process Start"] --> CheckCompat["Check OS Compatibility"]
CheckCompat --> DPI["Apply DPI Awareness Settings"]
DPI --> Run["Launch Application"]
```

**Diagram sources**
- [app.manifest:1-19](file://Agentic.Desktop/app.manifest#L1-L19)

**Section sources**
- [app.manifest:1-19](file://Agentic.Desktop/app.manifest#L1-L19)

### GitHub Dependabot Configuration (.github/dependabot.yml)
Automated dependency management setup:
- Configured for NuGet package ecosystem
- Targets the Agentic.Desktop directory where package manifests are located
- Daily schedule for automatic dependency updates
- Ensures security patches and updates are applied automatically

```mermaid
flowchart TD
Dependabot["GitHub Dependabot"] --> Scan["Scan NuGet Packages"]
Scan --> Analyze["Analyze Security & Updates"]
Analyze --> CreatePR["Create Pull Request"]
CreatePR --> Review["Developer Review"]
Review --> Merge["Merge Update"]
Merge --> Build["Automated Build"]
Build --> Test["Automated Testing"]
Test --> Deploy["Deploy Updated Package"]
```

**Diagram sources**
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)

**Section sources**
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)

### Localization Infrastructure
Comprehensive localization support:
- LocalizationService class provides centralized access to localized strings
- Resource files (.resw) organized by language (en, ja, zh-CN, zh-TW)
- Supports both simple string retrieval and formatted string operations
- Integrated throughout the application UI and code-behind

```mermaid
classDiagram
class LocalizationService {
+string Get(string key)
+string Format(string key, params object[] args)
-ResourceLoader _loader
}
class Resources {
+NavChat.Content
+StatusText.Text
+SettingsAgentConfig.Text
+PermissionDialog.Title
}
class XAML {
+x : Uid attributes
+Localized strings
}
LocalizationService --> Resources : reads
XAML --> LocalizationService : uses
```

**Diagram sources**
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)
- [Resources.resw:1-223](file://Agentic.Desktop/Strings/en/Resources.resw#L1-L223)

**Section sources**
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)
- [Resources.resw:1-223](file://Agentic.Desktop/Strings/en/Resources.resw#L1-L223)

### Build Configuration Options
- SDK and language features:
  - global.json pins SDK version and roll-forward policy
  - Directory.Build.props sets Nullable, ImplicitUsings, and LangVersion
- Launch settings:
  - launchSettings.json provides profiles for packaged and unpackaged debugging

```mermaid
classDiagram
class BuildConfig {
+string SdkVersion
+string RollForward
+bool Nullable
+bool ImplicitUsings
+string LangVersion
}
class LaunchSettings {
+string CommandName
}
class DependabotConfig {
+string PackageEcosystem
+string Directory
+string Schedule
}
BuildConfig <.. LaunchSettings : "used by"
BuildConfig <.. DependabotConfig : "managed by"
```

**Diagram sources**
- [global.json:1-11](file://global.json#L1-L11)
- [Directory.Build.props:1-8](file://Directory.Build.props#L1-L8)
- [launchSettings.json:1-10](file://Agentic.Desktop/Properties/launchSettings.json#L1-L10)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)

**Section sources**
- [global.json:1-11](file://global.json#L1-L11)
- [Directory.Build.props:1-8](file://Directory.Build.props#L1-L8)
- [launchSettings.json:1-10](file://Agentic.Desktop/Properties/launchSettings.json#L1-L10)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)

### Signing Procedures for Production Deployment
- For MSIX packages, sign using a trusted certificate:
  - Use signtool or Visual Studio packaging options to apply code signing
  - Ensure the certificate chain is valid and installed on target machines if sideloading
- For self-contained published artifacts:
  - Sign executables and dependent DLLs with Authenticode
  - Distribute certificates or use a trusted CA for automatic trust

[No sources needed since this section provides general guidance]

### Distribution Guidelines
- Microsoft Store:
  - Update Package.appxmanifest identity and logos
  - Prepare store submission assets and metadata
  - Use Visual Studio "Package and Publish" workflow
- Sideloading:
  - Export MSIX with proper signing
  - Provide instructions to enable sideloading on enterprise devices
- Self-contained publish:
  - Use publish profiles to output per-architecture binaries
  - Include all required redistributables and dependencies

[No sources needed since this section provides general guidance]

### Environment Variables, Configuration File Formats, and Runtime Settings
- Logging:
  - Application configures a logger factory with debug output and minimum level set during launch
- Runtime configuration:
  - Self-contained publishes bundle the runtime; no external runtime required
- Environment variables:
  - Agent path, arguments, and working directory are managed through the UI and persisted by the application's settings layer
- Configuration files:
  - No explicit JSON/XML config files are referenced in the project; settings are handled in-memory and surfaced via the Settings page
- Localization:
  - String resources are loaded from .resw files based on user's system language preference

```mermaid
sequenceDiagram
participant App as "App.xaml.cs"
participant Logger as "ILoggerFactory"
participant Localization as "LocalizationService"
participant UI as "SettingsViewModel"
participant FS as "FileSystemHandler"
App->>Logger : Create LoggerFactory (Debug, MinLevel=Debug)
App->>Localization : Initialize Resource Loader
App->>App : Initialize MainWindow and Activate
UI->>FS : Read/Write Settings (Agent Path, Arguments, Working Dir)
UI->>Localization : Get Localized Status Messages
FS-->>UI : Persisted Values
Localization-->>UI : Localized Strings
UI-->>App : Connection State Updates
```

**Diagram sources**
- [App.xaml.cs:1-73](file://Agentic.Desktop/App.xaml.cs#L1-L73)
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)

**Section sources**
- [App.xaml.cs:1-73](file://Agentic.Desktop/App.xaml.cs#L1-L73)
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)

## Dependency Analysis
The project depends on:
- Windows App SDK and build tools for WinUI 3 and MSIX packaging
- MVVM toolkit for observable properties and commands
- Logging infrastructure for diagnostics
- Markdown parser for content rendering
- Agent communication library for connecting to external agents
- GitHub Dependabot for automated dependency management
- Windows Resource Management for localization support

```mermaid
graph TB
CSProj["Agentic.Desktop.csproj"]
WinAppSDK["Microsoft.WindowsAppSDK"]
BuildTools["Microsoft.Windows.SDK.BuildTools"]
MVVM["CommunityToolkit.Mvvm"]
Logging["Microsoft.Extensions.Logging.Debug"]
Markdig["Markdig"]
ACPLib["ShihaoShen.Agentic.ACPLibrary"]
Dependabot["GitHub Dependabot"]
Localization["Windows Resources"]
CSProj --> WinAppSDK
CSProj --> BuildTools
CSProj --> MVVM
CSProj --> Logging
CSProj --> Markdig
CSProj --> ACPLib
Dependabot --> CSProj
Localization --> CSProj
```

**Diagram sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)

**Section sources**
- [Agentic.Desktop.csproj:1-83](file://Agentic.Desktop/Agentic.Desktop.csproj#L1-L83)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)

## Performance Considerations
- ReadyToRun compilation improves startup time in non-Debug builds
- Trimming reduces binary size by removing unused code paths
- Self-contained publishing avoids runtime resolution overhead but increases package size
- DPI awareness ensures crisp rendering across displays without extra scaling logic
- Localization resources are loaded on-demand to minimize memory footprint
- Dependabot updates are processed asynchronously to avoid build delays

[No sources needed since this section provides general guidance]

## Troubleshooting Guide
Common issues and resolutions:
- MSIX installation fails due to missing capabilities:
  - Verify Package.appxmanifest includes required capabilities such as runFullTrust and systemAIModels
- App does not start or crashes on launch:
  - Confirm app.manifest compatibility and DPI settings are present
  - Check that the executable entry point matches the manifest
- Publishing errors:
  - Ensure correct Platform and RuntimeIdentifier in publish profiles
  - Validate SelfContained and PublishSingleFile settings align with distribution needs
- Debugging:
  - Use launchSettings.json profiles to test both packaged and unpackaged modes
  - Inspect debug logs from the logger factory initialized at startup
- Localization issues:
  - Verify .resw files exist for all supported languages
  - Check that x:Uid attributes match resource keys in .resw files
  - Ensure DefaultLanguage is properly configured in project file
- Dependabot not updating packages:
  - Verify dependabot.yml configuration is correct
  - Check GitHub Actions permissions for repository
  - Ensure package manifests are in the specified directory

**Section sources**
- [Package.appxmanifest:1-57](file://Agentic.Desktop/Package.appxmanifest#L1-L57)
- [app.manifest:1-19](file://Agentic.Desktop/app.manifest#L1-L19)
- [launchSettings.json:1-10](file://Agentic.Desktop/Properties/launchSettings.json#L1-L10)
- [App.xaml.cs:1-73](file://Agentic.Desktop/App.xaml.cs#L1-L73)
- [dependabot.yml:1-12](file://.github/dependabot.yml#L1-L12)
- [LocalizationService.cs:1-23](file://Agentic.Desktop/Services/LocalizationService.cs#L1-L23)

## Conclusion
Agentic.Desktop is configured as a modern WinUI 3 desktop application targeting multiple Windows architectures with MSIX packaging. The project leverages robust build and publish configurations, clear capability declarations, and system-level manifests to ensure reliable deployment. With the addition of GitHub Dependabot for automated dependency management and comprehensive localization infrastructure supporting multiple languages, the application maintains security and accessibility standards. By following the signing and distribution guidelines, validating configurations, and using the provided troubleshooting steps, you can confidently deliver the application to users via the Microsoft Store or sideloading channels.

## Appendices
- Recommended validation steps:
  - Build in Release mode to enable ReadyToRun and trimming
  - Test both packaged and unpackaged launch profiles
  - Verify MSIX signature and capabilities before distribution
  - Confirm DPI scaling and resource localization across languages
  - Monitor Dependabot pull requests for dependency updates
  - Test localization with different system language settings

[No sources needed since this section provides general guidance]