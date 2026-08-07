# Agentic Desktop

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | 日本語

WinUI 3 + Uno Platform ベースの ACP（Agent Communication Protocol）デスクトップクライアント。単一のコードベースから 2 つのフレームワークをターゲットします：ネイティブ **WinUI 3** ビルド（MSIX パッケージ、Mica 背景）とクロスプラットフォーム **Uno Desktop / Skia** ビルド（直接 exe 実行、パッケージ不要）。

## 機能

- **チャットインターフェース** — ACP Agent とリアルタイムストリーミング対話、Markdown レンダリング対応
- **Agent 接続管理** — stdio 転送層経由で任意の ACP 対応 Agent 実行ファイルに接続
- **組み込み Mock Agent** — 実際の Agent なしで完全な UI フローを体験可能
- **権限管理** — Agent がファイル/ターミナルの権限を要求する際、対話型の確認ダイアログを表示
- **ターミナル管理** — Agent が開始するターミナルコマンドの実行をサポート
- **Fluent Design** — Mica 背景、アクリル素材、アダプティブテーマ
- **デュアルターゲット** — 単一の XAML コードベースからネイティブ WinUI 3 アプリと Uno Skia デスクトップアプリの両方を生成

## 技術スタック

| コンポーネント | バージョン |
| -------------- | ---------- |
| .NET | 10.0 |
| Windows App SDK | 2.3.1 |
| Uno.WinUI | 6.6.166 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Markdig | 1.3.2 |
| ShihaoShen.Agentic.ACPLibrary | 0.1.0-nightly |

## システム要件

- Windows 10 1809 (Build 17763) 以降
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WinApp CLI](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (`dotnet tool install -g winapp`)
- **開発者モード** を有効にする（設定 > システム > 開発者オプション）— WinUI / MSIX ターゲットのみ必要

## クイックスタート

リポジトリには 2 つの便利スクリプト（および汎用の `BuildAndRun.ps1`）が同梱されています：

| スクリプト | ターゲット | 起動方法 | 開発者モード |
| ---------- | ---------- | -------- | ------------ |
| `winui.ps1` | `net10.0-windows10.0.26100`（WinUI 3、MSIX） | `winapp run` | 必要 |
| `uno.ps1` | `net10.0-desktop`（Uno / Skia、直接 exe） | `.exe` を直接実行 | 不要 |

```powershell
# WinUI 3 ビルド（パッケージ、ネイティブ）
.\winui.ps1                  # ビルド + フォアグラウンド実行
.\winui.ps1 -Detach          # ビルド + バックグラウンド起動
.\winui.ps1 -SkipRun         # ビルドのみ

# Uno Desktop ビルド（Skia、直接 exe）
.\uno.ps1                    # ビルド + フォアグラウンド実行
.\uno.ps1 -Detach            # ビルド + バックグラウンド起動
.\uno.ps1 -SkipRun           # ビルドのみ
```

手動ビルド（スクリプト不使用）：

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
> `-m:1`（シングルプロセスビルド）は必須です。.NET 10 preview SDK において、Uno の `EmbeddedResourceInjectorTask` がマルチプロセス MSBuild 下で断続的に発生させる `MSB4018` エラーを回避するためです。両スクリプトは自動的にこの設定を適用します。

## 使い方

1. アプリ起動後、**設定** ページに移動します
2. Agent を設定します：
   - **Agent パス** — ACP Agent 実行ファイルのパスを入力（空欄にすると組み込み Mock Agent を使用）
   - **Agent 引数** — オプションの起動引数
   - **作業ディレクトリ** — Agent の作業ディレクトリ
3. **接続** をクリックし、ステータスが「接続済み」になるまで待ちます
4. **チャット** ページに切り替えて会話を開始します

## プロジェクト構造

```plaintext
App/
├── ViewModels/          # MVVM ビューモデル
│   ├── ChatViewModel.cs         # チャットロジック、ストリーミングメッセージ処理
│   ├── SettingsViewModel.cs     # Agent 接続管理
│   └── Messages/ChatMessage.cs  # メッセージモデル
├── Views/               # ダイアログ
│   └── PermissionDialog.xaml    # 権限確認ダイアログ
├── Services/            # 基盤サービス
│   ├── FileSystemHandler.cs     # ファイルシステム権限処理
│   ├── PermissionHandler.cs     # 権限リクエスト UI ディスパッチ
│   ├── TerminalManager.cs       # ターミナルセッション管理
│   └── MarkdownHelper.cs        # Markdown レンダリング
├── Converters/          # XAML 値コンバーター
├── Mocks/               # Mock Agent 転送層
├── MainPage.xaml        # チャットページ
├── SettingsPage.xaml    # 設定ページ
└── MainWindow.xaml      # メインウィンドウ（ナビゲーションフレームワーク）
```

## アーキテクチャ

アプリケーションは MVVM アーキテクチャを採用し、`IAcpClient` インターフェースを通じて Agent と通信します：

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

## ライセンス

[MIT](LICENSE) © 2026 Shihao Shen
