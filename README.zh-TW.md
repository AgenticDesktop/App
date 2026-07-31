# Agentic Desktop

[English](README.md) | [简体中文](README.zh-CN.md) | 繁體中文 | [日本語](README.ja.md)

一個基於 WinUI 3 的 ACP（Agent Communication Protocol）桌面客戶端，提供與 AI Agent 互動的聊天介面。

## 功能特色

- **聊天介面** — 與 ACP Agent 進行即時串流對話，支援 Markdown 轉譯
- **Agent 連線管理** — 透過 stdio 傳輸層連接任意 ACP 相容的 Agent 執行檔
- **內建 Mock Agent** — 無需真實 Agent 即可體驗完整 UI 流程
- **權限管理** — Agent 請求檔案/終端機權限時彈出互動式確認對話方塊
- **終端機管理** — 支援 Agent 發起的終端機命令執行
- **Fluent Design** — Mica 背景、壓克力材質、自適應主題

## 技術堆疊

| 元件 | 版本 |
|------|------|
| .NET | 10.0 |
| Windows App SDK | 2.3.1 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Markdig | 1.3.2 |
| ShihaoShen.Agentic.ACPLibrary  | 0.1.0-beta.3 |

## 系統需求

- Windows 10 1809 (Build 17763) 及以上
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WinApp CLI](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (`dotnet tool install -g winapp`)
- 開啟 **開發人員模式**（設定 > 系統 > 開發人員選項）

## 快速開始

```powershell
# 複製儲存庫
git clone https://github.com/AgenticDesktop/App.git
cd App
dotnet build -p:Platform=x64
winapp run bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64
```

## 使用說明

1. 啟動應用程式後進入 **設定** 頁面
2. 設定 Agent：
   - **Agent 路徑** — 填寫 ACP Agent 執行檔路徑（留空使用內建 Mock Agent）
   - **Agent 參數** — 可选的啟動參數
   - **工作目錄** — Agent 的工作目錄
3. 點擊 **連線**，等待狀態變為「已連線」
4. 切換到 **聊天** 頁面開始對話

## 專案結構

```
App/
├── ViewModels/          # MVVM 檢視模型
│   ├── ChatViewModel.cs         # 聊天邏輯、串流訊息處理
│   ├── SettingsViewModel.cs     # Agent 連線管理
│   └── Messages/ChatMessage.cs  # 訊息模型
├── Views/               # 對話方塊
│   └── PermissionDialog.xaml    # 權限確認對話方塊
├── Services/            # 基礎服務
│   ├── FileSystemHandler.cs     # 檔案系統權限處理
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

```
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
