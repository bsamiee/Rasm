# [RASM_APPUI_API_AVALONIAEDIT]

`Avalonia.AvaloniaEdit` and `AvaloniaEdit.TextMate` supply the `TextEditor` control, document model, folding, highlighting, completion, search, and TextMate grammar installation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.AvaloniaEdit`
- package: `Avalonia.AvaloniaEdit`
- assembly: `AvaloniaEdit`
- namespace: `AvaloniaEdit`
- namespace: `AvaloniaEdit.Document`
- namespace: `AvaloniaEdit.Editing`
- namespace: `AvaloniaEdit.Folding`
- namespace: `AvaloniaEdit.Highlighting`
- namespace: `AvaloniaEdit.CodeCompletion`
- namespace: `AvaloniaEdit.Search`
- namespace: `AvaloniaEdit.Rendering`
- asset: runtime library
- rail: editor

[PACKAGE_SURFACE]: `AvaloniaEdit.TextMate`
- package: `AvaloniaEdit.TextMate`
- assembly: `AvaloniaEdit.TextMate`
- namespace: `AvaloniaEdit.TextMate`
- asset: runtime library
- rail: editor

## [2]-[PUBLIC_TYPES]

[EDITOR_TYPES]: editor control and document model
- rail: editor

| [INDEX] | [SYMBOL]            | [RAIL]          |
| :-----: | :------------------ | :-------------- |
|   [1]   | `TextEditor`        | editor control  |
|   [2]   | `TextEditorOptions` | editor options  |
|   [3]   | `TextArea`          | editing surface |
|   [4]   | `Caret`             | caret state     |
|   [5]   | `Selection`         | selection model |
|   [6]   | `TextDocument`      | document model  |
|   [7]   | `DocumentLine`      | line model      |
|   [8]   | `TextAnchor`        | position anchor |
|   [9]   | `TextSegment`       | segment model   |
|  [10]   | `UndoStack`         | undo history    |

[FEATURE_TYPES]: folding, highlighting, completion, and search
- rail: editor

| [INDEX] | [SYMBOL]                        | [RAIL]              |
| :-----: | :------------------------------ | :------------------ |
|   [1]   | `FoldingManager`                | folding owner       |
|   [2]   | `FoldingSection`                | folded region       |
|   [3]   | `NewFolding`                    | folding input       |
|   [4]   | `XmlFoldingStrategy`            | XML folding         |
|   [5]   | `HighlightingManager`           | definition registry |
|   [6]   | `IHighlightingDefinition`       | definition contract |
|   [7]   | `DocumentColorizingTransformer` | line colorizer      |
|   [8]   | `IBackgroundRenderer`           | layer renderer      |
|   [9]   | `CompletionWindow`              | completion popup    |
|  [10]   | `ICompletionData`               | completion item     |
|  [11]   | `OverloadInsightWindow`         | overload insight    |
|  [12]   | `SearchPanel`                   | search overlay      |

[TEXTMATE_TYPES]: TextMate grammar surfaces
- rail: editor

| [INDEX] | [SYMBOL]                      | [RAIL]              |
| :-----: | :---------------------------- | :------------------ |
|   [1]   | `TextMate`                    | install extension   |
|   [2]   | `TextMate.Installation`       | grammar session     |
|   [3]   | `TextEditorModel`             | editor projection   |
|   [4]   | `TextMateColoringTransformer` | token colorizer     |
|   [5]   | `GenericLineTransformer`      | line transform base |
|   [6]   | `DocumentSnapshot`            | tokenizer snapshot  |

## [3]-[ENTRYPOINTS]

[EDITOR_ENTRYPOINTS]: editor control operations
- rail: editor

| [INDEX] | [SURFACE]            | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :------------------- | :------------- | :---------------- |
|   [1]   | `Text`               | `TextEditor`   | text content      |
|   [2]   | `Document`           | `TextEditor`   | document binding  |
|   [3]   | `SyntaxHighlighting` | `TextEditor`   | xshd highlighting |
|   [4]   | `Options`            | `TextEditor`   | behavior options  |
|   [5]   | `WordWrap`           | `TextEditor`   | wrap posture      |
|   [6]   | `IsReadOnly`         | `TextEditor`   | readonly posture  |
|   [7]   | `ShowLineNumbers`    | `TextEditor`   | margin toggle     |
|   [8]   | `Load`               | `TextEditor`   | stream load       |
|   [9]   | `Save`               | `TextEditor`   | stream save       |
|  [10]   | `AppendText`         | `TextEditor`   | content append    |
|  [11]   | `Clear`              | `TextEditor`   | content clear     |
|  [12]   | `ScrollTo`           | `TextEditor`   | caret navigation  |
|  [13]   | `SelectedText`       | `TextEditor`   | selection text    |
|  [14]   | `Copy`               | `TextEditor`   | clipboard copy    |
|  [15]   | `Cut`                | `TextEditor`   | clipboard cut     |
|  [16]   | `Paste`              | `TextEditor`   | clipboard paste   |

[FOLDING_ENTRYPOINTS]: folding lifecycle
- rail: editor

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]   | [RAIL]           |
| :-----: | :---------------------- | :--------------- | :--------------- |
|   [1]   | `Install`               | `FoldingManager` | margin install   |
|   [2]   | `Uninstall`             | `FoldingManager` | margin removal   |
|   [3]   | `UpdateFoldings`        | `FoldingManager` | folding refresh  |
|   [4]   | `CreateFolding`         | `FoldingManager` | manual fold      |
|   [5]   | `GetFoldingsContaining` | `FoldingManager` | fold query       |
|   [6]   | `AllFoldings`           | `FoldingManager` | fold enumeration |

[TEXTMATE_ENTRYPOINTS]: grammar registry and theme application
- rail: editor

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]          | [RAIL]            |
| :-----: | :----------------- | :---------------------- | :---------------- |
|   [1]   | `InstallTextMate`  | `TextMate`              | session install   |
|   [2]   | `RegistryOptions`  | `TextMate.Installation` | grammar registry  |
|   [3]   | `SetGrammar`       | `TextMate.Installation` | scope selection   |
|   [4]   | `SetGrammarFile`   | `TextMate.Installation` | file grammar      |
|   [5]   | `SetTheme`         | `TextMate.Installation` | theme apply       |
|   [6]   | `TryGetThemeColor` | `TextMate.Installation` | theme color query |
|   [7]   | `AppliedTheme`     | `TextMate.Installation` | theme event       |
|   [8]   | `Dispose`          | `TextMate.Installation` | session teardown  |

## [4]-[IMPLEMENTATION_LAW]

[EDITOR_LAW]:
- Package: `Avalonia.AvaloniaEdit`
- Owns: text editing, document model, folding, highlighting, completion, and search surfaces
- Accept: code-view intent maps to `TextEditor` with document-model state
- Reject: TextBox-derived custom code editors

[GRAMMAR_LAW]:
- Package: `AvaloniaEdit.TextMate`
- Owns: TextMate tokenization sessions bound to one editor through `InstallTextMate`
- Accept: grammar and theme state flow through `Installation` against an `IRegistryOptions` registry
- Reject: per-keystroke manual recolorization outside the installation transformers
