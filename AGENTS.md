# AGENTS.md

Project-level guidance for AI agents working on this repository.

## Tech Stack

- **UI Framework:** WinUI 3 (Windows App SDK 2.3.1) + Uno Platform (cross-platform)
- **Target Frameworks:** dual TFM — `net10.0-windows10.0.26100` (Windows) + `net10.0-desktop` (Uno Skia desktop)
- **SDK:** `Uno.Sdk`
- **Key Libraries:**
  - `CommunityToolkit.Mvvm` 8.4.2 — MVVM toolkit (`ObservableObject`, `RelayCommand`, `IMessenger`)
  - `Markdig` 1.3.2 — Markdown rendering
  - `ShihaoShen.Agentic.ACPLibrary` — ACP agent communication protocol
- **Language:** C# / .NET 10.0

## Build & Test

| Task | Command |
|------|----------|
| Build & Run | `pwsh ./BuildAndRun.ps1` **or** `winapp run` |
| Run tests | `dotnet test` |
| Restore | `dotnet restore` |

Always verify changes compile before committing.

## Architecture — MVVM

```
Agentic.Desktop/
├── Views/          ← XAML pages & user controls (code-behind only)
├── ViewModels/     ← ObservableObject classes, [RelayCommand], IMessenger messages
├── Services/       ← Business logic, I/O, transport, localization
├── Converters/     ← XAML value converters
├── Mocks/          ← Design-time / test doubles
└── ViewModels/Messages/  ← IMessenger message records
```

### Key Constraints

1. **x:Bind over Binding** — Use `{x:Bind}` (compiled binding) for all data binding.
   Every bound property must be public on the code-behind or ViewModel.
2. **CommunityToolkit.Mvvm** — All ViewModels inherit `ObservableObject`; use
   `[ObservableProperty]` and `[RelayCommand]` source generators. Do not manually
   implement `INotifyPropertyChanged`.
3. **Thread safety** — UI updates must go through `DispatcherQueue.TryEnqueue()`.
   Background work uses `Task.Run`; never block the UI thread.
4. **Uno conditional code** — Use `#if WINDOWS` (mapped from the `WINDOWS` define)
   for Windows App SDK-only APIs. Uno desktop target (`net10.0-desktop`) must
   compile without Windows-specific dependencies.
5. **XAML resource naming** — All user-facing strings go through `Resources.resw`;
   use `x:Uid` for localization. Never hard-code display strings in XAML.

## WinUI 3 & Uno Platform Technical Constraints

This project uses **WinUI 3** (via Windows App SDK) as the primary UI framework and **Uno Platform** as the cross-platform host. This dual-target architecture imposes specific technical constraints that every contributor and agent must follow.

### 1. Dual Target Framework (TFM)

The project targets two frameworks simultaneously:

| TFM | Host | Runtime |
|-----|------|----------|
| `net10.0-windows10.0.26100` | Windows App SDK (WinUI 3) | Native WinUI XAML compiler |
| `net10.0-desktop` | Uno Platform Skia | Skia-based XAML renderer |

**Consequences:**
- Every C# file must compile under **both** TFMs.
- Windows-only APIs (e.g., `Windows.ApplicationModel`, `Windows.Storage`) **must** be wrapped in `#if WINDOWS ... #endif`.
- The `WINDOWS` constant is defined automatically for the Windows TFM only (see `.csproj`).
- Uno desktop target **must not** reference `Microsoft.WindowsAppSDK` or any Win32-only assembly.

### 2. XAML Compiler Differences

| Feature | WinUI 3 (Windows) | Uno Platform (Desktop) |
|---------|-------------------|------------------------|
| `{x:Bind}` | Compiled at build time | Compiled at build time (Uno XAML compiler) |
| `{Binding}` | Runtime reflection | Runtime reflection (same) |
| `x:DataType` | Required for `{x:Bind}` in `DataTemplate` | Required (same) |
| `ThemeResource` | Resolved at runtime from WinUI theme | Resolved at runtime from Uno theme |
| `AcrylicBrush` | `Microsoft.UI.Xaml.Media.AcrylicBrush` | `Microsoft.UI.Xaml.Media.AcrylicBrush` (Uno emulates) |
| `BackdropReceiver` | WinUI 3 compositor | Not supported on Uno Skia — use `#if WINDOWS` |

**Rule:** Always test XAML changes under both TFMs. Visual differences between WinUI 3 and Uno Skia are expected; functional differences must be handled with conditional code.

### 3. Uno.Sdk and Package Management

- The project uses `Uno.Sdk` which injects implicit package references for both TFMs.
- **Do not** add `Uno.WinUI` or `Uno.UI` packages manually — the SDK handles this.
- To exclude a package from one TFM, use `<PackageReference Remove="..." Condition="..." />` (see `.csproj` for examples).
- The `.csproj` explicitly removes `Uno.WinUI.Runtime.Skia.Wpf` for the desktop target to avoid `NETSDK1136` conflicts.

### 4. Windows App SDK Version Override

- `Uno.Sdk` ships with a default `Microsoft.WindowsAppSDK` version.
- This project **overrides** it with an explicit `<PackageReference Include="Microsoft.WindowsAppSDK" Version="2.3.1" />` in a Windows-conditional `ItemGroup`.
- **Never** remove this override without verifying compatibility with the Uno.Sdk version.

### 5. Thread Model

- **WinUI 3:** Single UI thread per window. Use `DispatcherQueue.TryEnqueue()` to marshal back.
- **Uno Platform:** Same single-UI-thread model. `DispatcherQueue` API is identical.
- `async`/`await` resumes on the UI thread by default (capture context). Use `ConfigureAwait(false)` **only** in `Services/` layer code that never touches UI.

### 6. Resource System

- Both TFMs use `Resources.resw` files under `Strings/<lang>/`.
- `x:Uid` is the **only** supported localization mechanism in XAML.
- Code-behind localization uses `Windows.ApplicationModel.Resources.ResourceLoader` (Windows) or Uno's equivalent.
- **Never** use WPF-style `DynamicResource` for strings — this is not supported.

### 7. MSIX Packaging (Windows Only)

- The Windows TFM produces an MSIX package via `Package.appxmanifest`.
- The desktop TFM produces a standalone executable (no MSIX).
- Publishing profiles are in `Properties/PublishProfiles/` (`win-x64.pubxml`, `win-arm64.pubxml`, `win-x86.pubxml`).
- Use `/winui-packaging` skill for signing and Store submission workflows.

## Coding Rules

### 1. x:Bind 编译绑定

- **所有数据绑定必须使用 `{x:Bind}`**，禁止使用 `{Binding}`。
- 绑定的属性必须在 code-behind 或 ViewModel 上为 **public**。
- 集合绑定必须搭配 `DataTemplate` 和 `x:DataType`。
- 模式转换（如 `bool` → `Visibility`）使用 `x:Bind` 内置转换或自定义 `IValueConverter`。

### 2. MVVM 架构规范

- ViewModel **必须**继承 `CommunityToolkit.Mvvm.ObservableObject`。
- 使用 `[ObservableProperty]` 生成可观察属性，**禁止**手写 `INotifyPropertyChanged`。
- 使用 `[RelayCommand]` 生成命令，**禁止**手写 `new RelayCommand(...)`。
- View 的 code-behind **只放** UI 事件处理和 `x:Bind` 所需的 public 属性/方法。
- 业务逻辑、I/O、网络请求**必须**放在 `Services/` 层，禁止写在 View 或 ViewModel 中。

### 3. 线程安全

- UI 更新**必须**通过 `DispatcherQueue.TryEnqueue()` 回到 UI 线程。
- 后台工作使用 `Task.Run`，**禁止**阻塞 UI 线程。
- 异步方法命名以 `Async` 结尾，返回 `Task` 或 `Task<T>`。

### 4. Uno Platform 条件编译

- Windows App SDK 专有 API **必须**包裹在 `#if WINDOWS ... #endif` 中。
- `net10.0-desktop` 目标**不得**引用 Windows 专有依赖。
- 跨平台共享代码放在项目根目录，平台特定代码用 `#if` 隔离。

### 5. 本地化与资源

- 所有用户可见字符串**必须**通过 `Resources.resw` + `x:Uid` 加载，**禁止**在 XAML 中硬编码文字。
- 修改 `Strings/en/Resources.resw` 时**必须**同步更新 `zh-CN`、`zh-TW`、`ja` 三个版本。
- 修改 README 时**必须**同步更新四个语言版本（`README.md`、`README.zh-CN.md`、`README.zh-TW.md`、`README.ja.md`）。

### 6. 代码风格

- 使用 **file-scoped namespace**（`namespace X;`）。
- 使用 **primary constructor**（C# 12+ 语法）。
- 属性使用 `get; init;` 或 `get; set;`，避免多余的空格和换行。
- 方法参数超过 3 个时使用 **record** 或 **class** 封装参数。
- 禁止提交 `#region` 折叠区域。

### 7. 构建与测试

- 每次修改后**必须**运行 `pwsh ./BuildAndRun.ps1` 或 `dotnet build` 确认编译通过。
- 新增功能**必须**附带对应的单元测试，运行 `dotnet test` 验证。
- 测试项目位于 `Agentic.Desktop.Tests/`，测试类命名以 `Tests` 结尾。

## Agent Skills (winui-*)

This project uses a set of `winui-*` agent skills for WinUI 3 development.
Invoke them via their slash commands when the matching task arises:

| Skill | Slash Command | When to Use |
|-------|---------------|-------------|
| **winui-setup** | `/winui-setup` | First-time machine setup — install .NET SDK 10, WinApp CLI, WinUI 3 templates, Developer Mode |
| **winui-dev-workflow** | `/winui-dev-workflow` | Build, run, and diagnose the app (`BuildAndRun.ps1`, `winapp run`, error fixing) |
| **winui-design** | `/winui-design` | Design or review XAML UI — layout, control choice, Fluent Design, theming, accessibility |
| **winui-code-review** | `/winui-code-review` | Pre-commit code quality review — MVVM compliance, x:Bind, accessibility, security, performance |
| **winui-ui-testing** | `/winui-ui-testing` | Automated UI tests via `winapp ui` — element assertions, interactions, input, screenshots |
| **winui-packaging** | `/winui-packaging` | MSIX packaging, code signing, CI/CD, Microsoft Store submission |
| **winui-wpf-migration** | `/winui-wpf-migration` | Migrate WPF code/namespaces/controls to WinUI 3 |
| **winui-session-report** | `/winui-session-report` | Analyze a coding-agent session and produce a diagnostic report |

**Rule:** Before writing new XAML, run `/winui-design`. Before committing UI changes, run `/winui-code-review`. After a build failure, run `/winui-dev-workflow`.

### Skill Invocation Rules

The following rules are **mandatory** for all agents working on this project:

1. **Before writing any new XAML** — invoke `/winui-design` to plan layout, control choice, and Fluent Design alignment.
2. **Before committing UI changes** — invoke `/winui-code-review` to catch MVVM violations, x:Bind issues, accessibility gaps, and security problems.
3. **After a build failure** — invoke `/winui-dev-workflow` to diagnose and fix the error using the correct toolchain.
4. **Before running the app for the first time on a new machine** — invoke `/winui-setup` to verify all prerequisites (.NET 10 SDK, WinApp CLI, templates, Developer Mode).
5. **Before packaging or releasing** — invoke `/winui-packaging` for MSIX signing, certificate management, and Store submission.
6. **When migrating from WPF** — invoke `/winui-wpf-migration` for namespace replacement, control mapping, and MVVM conversion.
7. **After a long agent session** — invoke `/winui-session-report` to analyze what happened and identify improvement areas.
8. **When writing UI tests** — invoke `/winui-ui-testing` to generate and run automated tests via `winapp ui`.

**Never skip steps 1–3.** They are the minimum quality gate for every change.

## Documentation

### Keep all language versions in sync

This project ships README in four languages:

- `README.md` — English (source of truth)
- `README.zh-CN.md` — 简体中文
- `README.zh-TW.md` — 繁體中文
- `README.ja.md` — 日本語

When updating any README content (features, tech stack, commands, structure,
license, etc.), **update all four files in the same change**. Do not leave one
language stale.

Other multilingual docs under `Strings/` (app resources) follow the same rule:
`en`, `zh-CN`, `zh-TW`, `ja` must be updated together.

## Verification

Before considering any task complete, **all** of the following must pass:

1. **Build** — `pwsh ./BuildAndRun.ps1` or `dotnet build` exits with zero errors.
2. **Tests** — `dotnet test` passes with zero failures.
3. **x:Bind check** — Every `{x:Bind}` expression resolves at compile time; no runtime binding errors in the Output window.
4. **Localization sync** — If any `Resources.resw` or README was modified, all four language variants are updated.
5. **Uno dual-TFM** — Both `net10.0-windows10.0.26100` and `net10.0-desktop` targets compile without errors.
6. **No UI-thread violations** — All UI updates go through `DispatcherQueue.TryEnqueue()`; no cross-thread exceptions.

If any step fails, fix the root cause before marking the task as done.
