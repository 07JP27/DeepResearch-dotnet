# Deep Research Web Application

Azure Deep Research は、AI を活用した Web リサーチ・要約・反省・レポート生成を自動で行う Web アプリケーションです。

## 機能

- 研究トピックを入力すると AI が自動的に Web リサーチを実行
- リアルタイムで進行状況を表示
- 検索クエリ生成、Web 検索、要約、反省のサイクルを自動実行
- 最終的な研究レポートを生成・表示
- Blazor Server + SignalR によるリアルタイム通信

## 必要な設定

### 1. Azure OpenAI

`appsettings.Development.json`で以下を設定：

```json
{
  "OpenAI": {
    "Endpoint": "https://your-azure-openai-endpoint.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4"
  }
}
```

### 2. Tavily Search API

`appsettings.Development.json`で以下を設定：

```json
{
  "Tavily": {
    "ApiKey": "your-tavily-api-key"
  }
}
```

## 実行方法

```bash
# プロジェクトのルートディレクトリで
dotnet watch run --project app/DeepResearch.Web
```

または、VS Code のタスクランナーを使用：

```bash
# Ctrl+Shift+P → "Tasks: Run Task" → "watch-web"
```

## アーキテクチャ

- **DeepResearch.Core**: 研究ロジックとプロンプト管理
- **DeepResearch.SearchClient**: Tavily API 等の検索クライアント
- **DeepResearch.Web**: Blazor Server Web アプリケーション
  - SignalR ハブによるリアルタイム通信
  - TailwindCSS によるモダン UI
  - 進行状況のリアルタイム表示

## UI

- レスポンシブデザイン
- リアルタイム進行状況表示
- 各ステップの視覚的フィードバック
- 最終レポートの画像・ソース付き表示

## 技術スタック

- .NET 9.0
- Blazor Server
- SignalR
- Azure OpenAI
- Tavily Search API
- TailwindCSS
