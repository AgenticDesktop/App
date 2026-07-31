---
kind: frontend_style
name: WinUI 3 Fluent Design with XAML Theme Resources
category: frontend_style
scope:
    - '**'
source_files:
    - Agentic.Desktop/App.xaml
    - Agentic.Desktop/MainWindow.xaml
    - Agentic.Desktop/MainPage.xaml
    - Agentic.Desktop/SettingsPage.xaml
    - Agentic.Desktop/Views/ChatListPanel.xaml
    - Agentic.Desktop/Converters/BoolToVisibilityConverter.cs
    - Agentic.Desktop/Converters/StatusToColorConverter.cs
---

The Agentic Desktop application uses **WinUI 3** (Microsoft.UI.Xaml) as its frontend framework, following the **Fluent Design System** for styling and theming. The UI is built entirely in XAML with no CSS/SCSS — styling is handled through XAML Resource Dictionaries and WinUI's built-in theme resources.

### Styling Approach
- **Theme Resources**: All visual styling uses `{ThemeResource ...}` bindings to WinUI's built-in brushes (e.g., `ApplicationPageBackgroundThemeBrush`, `CardBackgroundFillColorDefaultBrush`, `AccentFillColorDefaultBrush`, `TextFillColorSecondaryBrush`, `SystemAccentColor`). This ensures automatic light/dark theme support and Windows system integration.
- **Mica & Acrylic Effects**: The main window applies `<MicaBackdrop />` for the modern Windows 11 frosted glass effect, while input areas use `AcrylicBackgroundFillColorDefaultBrush` for translucent panels.
- **Resource Dictionary Composition**: Global styles are centralized in `App.xaml` via merged dictionaries, currently loading `XamlControlsResources` from `Microsoft.UI.Controls`.

### Component Architecture
- **MVVM Pattern**: Views (`*.xaml`) bind to ViewModels via `x:Bind` with one-way/two-way data binding. No code-behind styling logic exists.
- **Reusable Converters**: Custom `IValueConverter` implementations in `Converters/` handle UI state transformations:
  - `BoolToVisibilityConverter`: Converts booleans to Visibility with optional inversion via `ConverterParameter="Invert"`
  - `StatusToColorConverter`: Maps connection states to SolidColorBrush colors (Green/Orange/Gray)
- **UserControl Components**: `Views/ChatListPanel.xaml` encapsulates the chat sidebar as a reusable UserControl with its own resource scope.

### Layout Conventions
- **Grid-based layouts**: Consistent use of `Grid` with `RowSpacing`/`ColumnSpacing` for spacing instead of margins where possible.
- **Responsive patterns**: `SplitView` for collapsible sidebars, `ItemsRepeater` for performant lists, `ScrollViewer` for overflow content.
- **Consistent padding/margins**: Standardized spacing using `Spacing` properties on StackPanel/Grid rather than ad-hoc margins.

### Data Templates
- Message types use separate `DataTemplate` definitions (`UserMessageTemplate`, `AgentMessageTemplate`) with a `ChatMessageTemplateSelector` to switch between user/agent message styling.
- Chat list items use inline `DataTemplate` with `x:DataType` for compile-time binding safety.

### Accessibility & Internationalization
- `AutomationProperties.AutomationId` attributes on interactive elements for accessibility testing.
- Localized strings via `.resw` files under `Strings/{locale}/Resources.resw` with `x:Uid` bindings.

### What's NOT Used
- No CSS/SCSS files — pure XAML styling
- No third-party UI libraries beyond WinUI 3 controls
- No custom color palettes or design tokens — relies entirely on WinUI theme resources
- No responsive breakpoints — desktop-only layout