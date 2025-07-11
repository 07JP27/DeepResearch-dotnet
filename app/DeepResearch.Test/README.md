# DeepResearch.Test

このプロジェクトは`DeepResearch.Core`のための包括的な単体テストプロジェクトです。

## テストフレームワーク

- **xUnit** - 主要なテストフレームワーク
- **FluentAssertions** - 読みやすいアサーション
- **Moq** - モッキング（将来の拡張用）

## テスト構造

### テストファイル

- `DeepResearchOptionsTests.cs` - 設定クラスのテスト
- `ResearchResultTests.cs` - 研究結果モデルのテスト
- `ResearchStateTests.cs` - 内部状態モデルのテスト
- `FormattingTests.cs` - フォーマットユーティリティのテスト
- `ProgressModelsTests.cs` - プログレスモデルのテスト

### ソースファイル

テスト対象のソースファイルは`Source/`ディレクトリにコピー/リンクされています：

- `DeepResearchOptions.cs` - 設定オプション
- `ResearchResult.cs` - 研究結果
- `ResearchState.cs` - 内部状態
- `Formatting.cs` - フォーマットユーティリティ
- `Models/` - プログレスモデル群

## テスト実行

### コマンドライン

```bash
# すべてのテストを実行
dotnet test

# 詳細出力でテストを実行
dotnet test --verbosity normal

# カバレッジ付きでテストを実行
dotnet test --collect "Code Coverage"
```

### VSCode

VSCodeタスクが設定されています：

- `Ctrl+Shift+P` → `Tasks: Run Task` → `Run Tests`
- `Ctrl+Shift+P` → `Tasks: Run Task` → `Run Tests with Coverage`
- `Ctrl+Shift+P` → `Tasks: Run Task` → `Build and Test`

## テスト対象機能

### DeepResearchOptions
- デフォルト値の検証
- プロパティの設定・取得
- 各種数値の境界値テスト

### ResearchResult
- 初期化状態の検証
- プロパティの設定・取得
- コレクションの操作

### ResearchState
- 内部状態の管理
- ワークフロー全体のシミュレーション
- すべてのプロパティの動作

### Formatting
- `DeduplicateAndFormatSources` - 重複排除とフォーマット
- `FormatSources` - 簡単なフォーマット
- `DeduplicateAndCleanSources` - 重複排除とクリーニング
- 日本語文字化けテキストのクリーニング
- エッジケースの処理

### Progress Models
- すべてのプログレスクラスの基本機能
- 継承構造の検証
- プロパティの設定・取得

## テスト統計

合計：93テスト
- `DeepResearchOptionsTests`: 8テスト
- `ResearchResultTests`: 11テスト 
- `ResearchStateTests`: 12テスト
- `FormattingTests`: 45テスト
- `ProgressModelsTests`: 17テスト

## 注意事項

- このテストプロジェクトは.NET 8.0をターゲットにしています
- 元のプロジェクトが.NET 9.0をターゲットにしている場合、互換性を保つためにソースファイルをコピーしています
- `JsonSchemaGenerator`クラスは.NET 9.0固有の機能を使用しているため、このテストプロジェクトには含まれていません