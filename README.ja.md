# Agentic Desktop

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | 日本語

WinUI 3 ベースの ACP（Agent Communication Protocol）デスクトップクライアントで、AI Agent と対話するためのチャットインターフェースを提供します。

## 機能

- **チャットインターフェース** — ACP Agent とリアルタイムストリーミング対話、Markdown レンダリング対応
- **Agent 接続管理** — stdio 転送層経由で任意の ACP 対応 Agent 実行ファイルに接続
- **組み込み Mock Agent** — 実際の Agent なしで完全な UI フローを体験可能
- **権限管理** — Agent がファイル/ターミナルの権限を要求する際、対話型の確認ダイアログを表示
- **ターミナル管理** — Agent が開始するターミナルコマンドの実行をサポート
- **Fluent Design** — Mica 背景、アクリル素材、アダプティブテーマ

## 技術スタック

| コンポーネント | バージョン |
| -------------- | ---------- |
| .NET | 10.0 |
| Windows App SDK | 2.3.1 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Markdig | 1.3.2 |
| ShihaoShen.Agentic.ACPLibrary | 0.1.0-beta.3 |

## システム要件

- Windows 10 1809 (Build 17763) 以降
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WinApp CLI](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (`dotnet tool install -g winapp`)
- **開発者モード** を有効にする（設定 > システム > 開発者オプション）

## クイックスタート

```powershell
# リポジトリをクローン
git clone https://github.com/AgenticDesktop/App.git
cd App
dotnet build -p:Platform=x64
winapp run bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64
```

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
