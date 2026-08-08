# Agentic Desktop

[English](README.md) | [简体中文](README.zh-CN.md) | 繁體中文 | [日本語](README.ja.md)

一個基於 WinUI 3 + Uno Platform 的 ACP（Agent Communication Protocol）桌面客戶端。同一份程式碼庫面向兩個框架：原生 **WinUI 3** 建置（MSIX 封裝、Mica 背景）和跨平台 **Uno Desktop / Skia** 建置（直接執行 exe、無需封裝）。

## 功能特色

- **聊天介面** — 與 ACP Agent 進行即時串流對話，支援 Markdown 轉譯
- **Agent 連線管理** — 透過 stdio 傳輸層連接任意 ACP 相容的 Agent 執行檔
- **內建 Mock Agent** — 無需真實 Agent 即可體驗完整 UI 流程
- **權限管理** — Agent 請求檔案/終端機權限時彈出互動式確認對話方塊
- **終端機管理** — 支援 Agent 發起的終端機命令執行
- **Fluent Design** — Mica 背景、壓克力材質、自適應主題
- **雙目標** — 單份 XAML 程式碼庫同時產出原生 WinUI 3 應用程式與 Uno Skia 桌面應用程式

## 技術堆疊

| 元件 | 版本 |
| ------ | ------ |
| .NET | 10.0 |
| Windows App SDK | 2.3.1 |
| Uno.WinUI | 6.6.166 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Markdig | 1.3.2 |
| ShihaoShen.Agentic.ACPLibrary | 0.2.0 |

## 系統需求

- Windows 10 1809 (Build 17763) 及以上
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WinApp CLI](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (`dotnet tool install -g winapp`)
- 開啟 **開發人員模式**（設定 > 系統 > 開發人員選項）— 僅 WinUI / MSIX 目標需要

## 快速開始

> [!WARNING]
> 在非 Windows 平台（Linux、macOS）上建置並不受官方支援，即使是 Uno Desktop 建置也是如此。為了獲得更好的體驗，建議使用 Windows（實體機、VMware、Parallel Desktop 等）進行開發。

> [!IMPORTANT]
> 如果你是在與 `ShihaoShen.Agentic.ACPLibrary` 程式庫一起開發應用，請確保將兩個儲存庫複製到同一個父目錄下，讓 `Agentic.Desktop` 專案能夠引用本機程式庫專案，而不是 NuGet 套件。
>
> 像這樣：
>
> ```plaintext
> AgenticDesktop-DevFolder/
> ├── App/ (目前的儲存庫)
> └── ACPLibrary/ (程式庫儲存庫)
> ```

倉庫附帶兩個便利腳本：

| 腳本 | 目標 | 啟動方式 | 需要開發人員模式 |
| ------ | ------ | ------ | -------------- |
| `winui.ps1` | `net10.0-windows10.0.26100`（WinUI 3，MSIX） | `winapp run` | 是 |
| `uno.ps1` | `net10.0-desktop`（Uno / Skia，直接 exe） | 直接執行 `.exe` | 否 |

```powershell
# WinUI 3 建置（封裝、原生）
.\winui.ps1                  # 建置 + 前景執行
.\winui.ps1 -Detach          # 建置 + 背景啟動
.\winui.ps1 -SkipRun         # 僅建置

# Uno Desktop 建置（Skia，直接 exe）
.\uno.ps1                    # 建置 + 前景執行
.\uno.ps1 -SkipRun           # 僅建置
```

手動建置（不使用腳本）：

```powershell
git clone https://github.com/AgenticDesktop/App.git
cd App

# WinUI 3
dotnet build -p:Platform=x64 -f net10.0-windows10.0.26100 -m:1
winapp run Agentic.Desktop\bin\x64\Debug\net10.0-windows10.0.26100\win-x64

# Uno Desktop
dotnet build -p:Platform=x64 -f net10.0-desktop -m:1
Agentic.Desktop\bin\x64\Debug\net10.0-desktop\Agentic.Desktop.exe
```

> [!NOTE]
> `-m:1`（單進程建置）是必需的，用於規避 .NET 10 preview SDK 上 Uno `EmbeddedResourceInjectorTask` 在多進程 MSBuild 下偶發的 `MSB4018` 錯誤。兩個腳本會自動套用此設定。

## 使用說明

1. 啟動應用程式後進入 **設定** 頁面
2. 設定 Agent：
   - **Agent 路徑** — 填寫 ACP Agent 執行檔路徑（留空使用內建 Mock Agent）
   - **Agent 參數** — 可選的啟動參數
   - **工作目錄** — Agent 的工作目錄
3. 點擊 **連線**，等待狀態變為「已連線」
4. 切換到 **聊天** 頁面開始對話

## 專案結構

```plaintext
App/
├── ViewModels/          # MVVM 檢視模型
│   ├── ChatViewModel.cs         # 聊天邏輯、串流訊息處理
│   ├── ChatListViewModel.cs     # 聊天會話列表管理
│   ├── SettingsViewModel.cs     # Agent 連線管理
│   └── Messages/
│       ├── ChatMessage.cs       # 訊息模型
│       └── ChatSession.cs       # 聊天會話模型
├── Views/               # 對話方塊和面板
│   ├── ChatListPanel.xaml       # 聊天會話列表面板
│   ├── ChatListPanel.xaml.cs
│   ├── PermissionDialog.xaml    # 權限確認對話方塊
│   └── PermissionDialog.xaml.cs
├── Services/            # 核心服務
│   ├── FileSystemHandler.cs     # 檔案系統權限處理
│   ├── LocalizationService.cs   # 本地化 / i18n
│   ├── PermissionHandler.cs     # 權限請求 UI 排程
│   ├── TerminalManager.cs       # 終端機工作階段管理
│   └── MarkdownHelper.cs        # Markdown 轉譯
├── Converters/          # XAML 值轉換子
├── Mocks/               # Mock Agent 傳輸層
├── MainPage.xaml        # 聊天頁面
├── SettingsPage.xaml    # 設定頁面
└── MainWindow.xaml      # 主視窗（導覽框架）
```

## 架構

應用程式採用 MVVM 架構，透過 `IAcpClient` 介面與 Agent 通訊：

```plaintext
┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  WinUI UI   │────▶│   ViewModels     │────▶│   AcpClient     │
│  (XAML)     │◀────│  (CommunityToolkit)│◀────│  (ACP Library)  │
└─────────────┘     └──────────────────┘     └────────┬────────┘
                                                      │
                                             ┌────────▼────────┐
                                             │  IAgentTransport │
                                             │  (stdio / mock)  │
                                             └─────────────────┘
```

## 授權條款

[MIT](LICENSE) © 2026 Shihao Shen
