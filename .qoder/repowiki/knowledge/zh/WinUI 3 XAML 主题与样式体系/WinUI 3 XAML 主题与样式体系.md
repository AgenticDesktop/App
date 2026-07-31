---
kind: frontend_style
name: WinUI 3 XAML 主题与样式体系
category: frontend_style
scope:
    - '**'
source_files:
    - Agentic.Desktop/App.xaml
    - Agentic.Desktop/MainWindow.xaml
    - Agentic.Desktop/MainPage.xaml
    - Agentic.Desktop/SettingsPage.xaml
    - Agentic.Desktop/Views/ChatListPanel.xaml
    - Agentic.Desktop/Views/PermissionDialog.xaml
    - Agentic.Desktop/Converters/BoolToVisibilityConverter.cs
---

## 1. 系统/方法
本项目采用 **WinUI 3 + Windows App SDK** 的 XAML 声明式 UI 框架，通过 Fluent Design System 提供原生 Windows 11 视觉风格。样式完全基于 XAML ResourceDictionary 和 ThemeResource 动态资源，未使用 CSS/SCSS/Tailwind 等 Web 技术栈。

## 2. 核心文件与包
- `App.xaml`：应用级资源字典入口，合并 `Microsoft.UI.Xaml.Controls` 的 `XamlControlsResources`
- `MainWindow.xaml`：主窗口布局，使用 `NavigationView` + `TitleBar` + `MicaBackdrop` 构建桌面主界面
- `MainPage.xaml` / `SettingsPage.xaml`：页面级视图，定义聊天界面与设置页
- `Views/ChatListPanel.xaml`、`Views/PermissionDialog.xaml`：可复用用户控件与对话框
- `Converters/*.cs`：XAML 值转换器（BoolToVisibilityConverter、StatusToColorConverter）
- `Strings/*/Resources.resw`：多语言本地化资源

## 3. 架构与约定
- **Fluent Design 主题资源**：所有颜色、背景、边框均通过 `{ThemeResource ...}` 引用（如 `ApplicationPageBackgroundThemeBrush`、`CardBackgroundFillColorDefaultBrush`、`AccentFillColorDefaultBrush`、`TextFillColorSecondaryBrush`），确保跟随系统深色/浅色模式与 Accent Color。
- **XAML 数据绑定**：MVVM 模式下通过 `x:Bind` 进行编译时强类型绑定，ViewModel 位于 `Agentic.Desktop.ViewModels` 命名空间。
- **组件化布局**：`SplitView` 实现侧边栏+内容区布局；`ItemsRepeater` + `DataTemplate` 渲染消息列表；`ContentDialog` 作为权限审批弹窗。
- **样式复用**：按钮统一使用 `Style="{StaticResource AccentButtonStyle}"`，卡片使用 `Border` + `CornerRadius="8"` + `CardStrokeColorDefaultBrush` 边框。
- **无自定义 CSS**：项目内不存在任何 `.css`/`.scss`/`.less` 文件，样式全部内嵌于 XAML 或通过 ThemeResource 继承。

## 4. 开发者应遵循的规则
- 优先使用 `ThemeResource` 而非硬编码颜色值，以支持系统主题切换。
- 使用 WinUI 3 内置控件（`NavigationView`、`SplitView`、`ItemsRepeater`、`ContentDialog`）保持视觉一致性。
- 复杂可见性逻辑通过 `Converters` 目录下的值转换器处理，避免在 XAML 中写条件逻辑。
- 新增页面应在 `Views/` 下创建 XAML + code-behind 文件，并在 `App.xaml` 的资源字典中注册全局样式。
- 文本与提示文案统一放入 `Strings/<locale>/Resources.resw`，通过 `x:Uid` 或代码访问。
- 图标统一使用 `FontIcon` + Segoe Fluent Icons 字符码（如 `&#xE700;`、`&#xE711;`）。