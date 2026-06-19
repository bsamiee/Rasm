# [RASM_APPUI_API_AVALONIAEDIT]

`Avalonia.AvaloniaEdit` and `AvaloniaEdit.TextMate` supply the `TextEditor` control, document model, folding, highlighting, completion, search, and TextMate grammar installation.

## [01]-[PACKAGE_SURFACE]

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

## [02]-[PUBLIC_TYPES]

[EDITOR_TYPES]: editor control and document model
- rail: editor

| [INDEX] | [SYMBOL]            | [RAIL]          |
| :-----: | :------------------ | :-------------- |
|  [01]   | `TextEditor`        | editor control  |
|  [02]   | `TextEditorOptions` | editor options  |
|  [03]   | `TextArea`          | editing surface |
|  [04]   | `Caret`             | caret state     |
|  [05]   | `Selection`         | selection model |
|  [06]   | `TextDocument`      | document model  |
|  [07]   | `DocumentLine`      | line model      |
|  [08]   | `TextAnchor`        | position anchor |
|  [09]   | `TextSegment`       | segment model   |
|  [10]   | `UndoStack`         | undo history    |

[FEATURE_TYPES]: folding, highlighting, completion, and search
- rail: editor

| [INDEX] | [SYMBOL]                        | [RAIL]              |
| :-----: | :------------------------------ | :------------------ |
|  [01]   | `FoldingManager`                | folding owner       |
|  [02]   | `FoldingSection`                | folded region       |
|  [03]   | `NewFolding`                    | folding input       |
|  [04]   | `XmlFoldingStrategy`            | XML folding         |
|  [05]   | `HighlightingManager`           | definition registry |
|  [06]   | `IHighlightingDefinition`       | definition contract |
|  [07]   | `DocumentColorizingTransformer` | line colorizer      |
|  [08]   | `IBackgroundRenderer`           | layer renderer      |
|  [09]   | `CompletionWindow`              | completion popup    |
|  [10]   | `ICompletionData`               | completion item     |
|  [11]   | `OverloadInsightWindow`         | overload insight    |
|  [12]   | `SearchPanel`                   | search overlay      |

[TEXTMATE_TYPES]: TextMate grammar surfaces
- rail: editor

| [INDEX] | [SYMBOL]                      | [RAIL]              |
| :-----: | :---------------------------- | :------------------ |
|  [01]   | `TextMate`                    | install extension   |
|  [02]   | `TextMate.Installation`       | grammar session     |
|  [03]   | `TextEditorModel`             | editor projection   |
|  [04]   | `TextMateColoringTransformer` | token colorizer     |
|  [05]   | `GenericLineTransformer`      | line transform base |
|  [06]   | `DocumentSnapshot`            | tokenizer snapshot  |

## [03]-[ENTRYPOINTS]

[EDITOR_ENTRYPOINTS]: editor control operations
- rail: editor

| [INDEX] | [SURFACE]            | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :------------------- | :------------- | :---------------- |
|  [01]   | `Text`               | `TextEditor`   | text content      |
|  [02]   | `Document`           | `TextEditor`   | document binding  |
|  [03]   | `SyntaxHighlighting` | `TextEditor`   | xshd highlighting |
|  [04]   | `Options`            | `TextEditor`   | behavior options  |
|  [05]   | `WordWrap`           | `TextEditor`   | wrap posture      |
|  [06]   | `IsReadOnly`         | `TextEditor`   | readonly posture  |
|  [07]   | `ShowLineNumbers`    | `TextEditor`   | margin toggle     |
|  [08]   | `Load`               | `TextEditor`   | stream load       |
|  [09]   | `Save`               | `TextEditor`   | stream save       |
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
|  [01]   | `Install`               | `FoldingManager` | margin install   |
|  [02]   | `Uninstall`             | `FoldingManager` | margin removal   |
|  [03]   | `UpdateFoldings`        | `FoldingManager` | folding refresh  |
|  [04]   | `CreateFolding`         | `FoldingManager` | manual fold      |
|  [05]   | `GetFoldingsContaining` | `FoldingManager` | fold query       |
|  [06]   | `AllFoldings`           | `FoldingManager` | fold enumeration |

[TEXTMATE_ENTRYPOINTS]: grammar registry and theme application
- rail: editor

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]          | [RAIL]            |
| :-----: | :----------------- | :---------------------- | :---------------- |
|  [01]   | `InstallTextMate`  | `TextMate`              | session install   |
|  [02]   | `RegistryOptions`  | `TextMate.Installation` | grammar registry  |
|  [03]   | `SetGrammar`       | `TextMate.Installation` | scope selection   |
|  [04]   | `SetGrammarFile`   | `TextMate.Installation` | file grammar      |
|  [05]   | `SetTheme`         | `TextMate.Installation` | theme apply       |
|  [06]   | `TryGetThemeColor` | `TextMate.Installation` | theme color query |
|  [07]   | `AppliedTheme`     | `TextMate.Installation` | theme event       |
|  [08]   | `Dispose`          | `TextMate.Installation` | session teardown  |

## [04]-[IMPLEMENTATION_LAW]

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
