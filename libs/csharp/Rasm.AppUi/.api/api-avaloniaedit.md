# [RASM_APPUI_API_AVALONIAEDIT]

`Avalonia.AvaloniaEdit` mints a code-editor control over a rope-backed `TextDocument`: a `TextArea`/`TextView` render stack with colorizing transformers and element generators, undo grouping, code folding, xshd highlighting, `CompletionWindow` IntelliSense, regex search, and snippet and indentation engines. `AvaloniaEdit.TextMate` bolts a TextMate tokenizer onto one editor through `InstallTextMate(IRegistryOptions)`, installing a `TextMateColoringTransformer` on its `TextView`. `api-textmatesharp.md` owns the `IRegistryOptions`/`IGrammar`/`ThemeName` provider rail that adapter consumes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.AvaloniaEdit`
- package: `Avalonia.AvaloniaEdit` (`MIT`)
- assembly: `AvaloniaEdit` (name ≠ package id; compiled-XAML resources embedded)
- namespace: `AvaloniaEdit` (control + routed commands), `.Document`, `.Editing`, `.Folding`, `.Highlighting`(+`.Xshd`), `.CodeCompletion`, `.Search`, `.Snippets`, `.Indentation`(+`.CSharp`), `.Rendering`
- asset: runtime library
- rail: editor

[PACKAGE_SURFACE]: `AvaloniaEdit.TextMate`
- package: `AvaloniaEdit.TextMate` (`MIT`)
- assembly: `AvaloniaEdit.TextMate`
- namespace: `AvaloniaEdit.TextMate`
- asset: runtime library
- depends: `TextMateSharp`, `TextMateSharp.Grammars` (transitive) — the provider rail `api-textmatesharp.md` owns
- adapter: `TextEditorModel`/`DocumentSnapshot` bridge `TextDocument` onto the tokenizer's `IModelLines`/`IModelTokensChangedListener`

## [02]-[PUBLIC_TYPES]

[EDITOR_TYPES]: control, options, editing surface, and rope document model

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `TextEditor`                               | class          | editor control                  |
|  [02]   | `TextEditorOptions`                        | class          | wrap/indent/whitespace policy   |
|  [03]   | `TextArea`                                 | class          | input, selection, caret surface |
|  [04]   | `Caret`                                    | class          | caret state                     |
|  [05]   | `Selection`                                | abstract class | selection model                 |
|  [06]   | `SimpleSelection` / `RectangleSelection`   | class          | selection variant               |
|  [07]   | `TextDocument`                             | class          | rope text model (`: IDocument`) |
|  [08]   | `DocumentLine`                             | class          | line model                      |
|  [09]   | `TextAnchor`                               | class          | edit-surviving position         |
|  [10]   | `TextSegment` / `TextSegmentCollection<T>` | class          | red-black segment tree          |
|  [11]   | `UndoStack`                                | class          | undo history                    |
|  [12]   | `TextViewPosition`                         | struct         | visual position                 |

- `Caret`: `Offset` `Line` `Column` `Location`
- `TextDocument`: `BeginUpdate` `Replace`
- `DocumentLine`: `Offset` `Length` `LineNumber`
- `UndoStack`: `Undo` `Redo` `StartUndoGroup` `SizeLimit`
- `TextViewPosition`: `Line` `Column` `VisualColumn`

[FEATURE_TYPES]: folding, highlighting, completion, search, snippets, and indentation

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]         |
| :-----: | :------------------------------ | :------------- | :------------------- |
|  [01]   | `FoldingManager`                | class          | folding owner        |
|  [02]   | `FoldingSection`                | class          | folded region        |
|  [03]   | `NewFolding`                    | class          | folding input        |
|  [04]   | `XmlFoldingStrategy`            | class          | XML folding          |
|  [05]   | `HighlightingManager`           | class          | definition registry  |
|  [06]   | `IHighlightingDefinition`       | interface      | definition contract  |
|  [07]   | `HighlightingLoader`            | static class   | xshd loader          |
|  [08]   | `DocumentHighlighter`           | class          | highlight engine     |
|  [09]   | `IHighlighter`                  | interface      | highlight contract   |
|  [10]   | `DocumentColorizingTransformer` | abstract class | line colorizer       |
|  [11]   | `IBackgroundRenderer`           | interface      | layer renderer       |
|  [12]   | `CompletionWindow`              | class          | completion popup     |
|  [13]   | `ICompletionData`               | interface      | completion item      |
|  [14]   | `OverloadInsightWindow`         | class          | overload popup       |
|  [15]   | `IOverloadProvider`             | interface      | overload contract    |
|  [16]   | `SearchPanel`                   | class          | search overlay       |
|  [17]   | `ISearchStrategy`               | interface      | search contract      |
|  [18]   | `RegexSearchStrategy`           | class          | regex search engine  |
|  [19]   | `Snippet`                       | class          | snippet root         |
|  [20]   | `SnippetTextElement`            | class          | snippet literal      |
|  [21]   | `SnippetReplaceableTextElement` | class          | snippet tab-stop     |
|  [22]   | `SnippetBoundElement`           | class          | snippet bound field  |
|  [23]   | `SnippetCaretElement`           | class          | snippet caret target |
|  [24]   | `SnippetSelectionElement`       | class          | snippet selection    |
|  [25]   | `IIndentationStrategy`          | interface      | indentation contract |
|  [26]   | `CSharpIndentationStrategy`     | class          | C# indenter          |

- `FoldingSection`: `IsFolded` `Title`
- `NewFolding`: `StartOffset` `EndOffset` `Name` `DefaultClosed`
- `HighlightingManager`: `Instance` `GetDefinition` `RegisterHighlighting`
- `IHighlightingDefinition`: `MainRuleSet` `GetNamedColor` `Properties`
- `HighlightingLoader`: `Load(XmlReader, IHighlightingDefinitionReferenceResolver)`
- `DocumentColorizingTransformer`: `ColorizeLine`
- `IBackgroundRenderer`: `Layer` `Draw(TextView, DrawingContext)`
- `ICompletionData`: `Image` `Text` `Content` `Description` `Priority` `Complete`
- `IOverloadProvider`: `SelectedIndex` `Count` `CurrentHeader` `CurrentContent`

[RENDERING_TYPES]: the `TextView` extension surface for custom visuals (`AvaloniaEdit.Rendering`)

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]        |
| :-----: | :--------------------------- | :------------- | :------------------ |
|  [01]   | `TextView`                   | class          | render host         |
|  [02]   | `VisualLineElementGenerator` | abstract class | inline elements     |
|  [03]   | `LinkElementGenerator`       | class          | hyperlinks          |
|  [04]   | `MailLinkElementGenerator`   | class          | mail links          |
|  [05]   | `IVisualLineTransformer`     | interface      | line transformation |
|  [06]   | `KnownLayer`                 | enum           | layer identity      |
|  [07]   | `BackgroundGeometryBuilder`  | class          | overlay geometry    |

- `TextView`: `LineTransformers` `BackgroundRenderers` `ElementGenerators` `Redraw`
- `VisualLineElementGenerator`: `GetFirstInterestedOffset` `ConstructElement`
- `IVisualLineTransformer`: `Transform(ITextRunConstructionContext, IList<VisualLineElement>)`

[COMMAND_TYPES]: routed-command surface for keybinding and menu wiring (`AvaloniaEdit`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]           | [CAPABILITY]    |
| :-----: | :--------------------- | :---------------------- | :-------------- |
|  [01]   | `ApplicationCommands`  | `RoutedCommand` statics | clipboard/edit  |
|  [02]   | `EditingCommands`      | `RoutedCommand` statics | caret/selection |
|  [03]   | `AvaloniaEditCommands` | `RoutedCommand` statics | editor-specific |
|  [04]   | `SearchCommands`       | `RoutedCommand` statics | search keys     |
|  [05]   | `RoutedCommand`        | class                   | command         |
|  [06]   | `RoutedCommandBinding` | class                   | binding         |

- `ApplicationCommands`: `Copy` `Cut` `Paste` `Delete` `SelectAll` `Undo` `Redo` `Find` `Replace`
- `EditingCommands`: caret and selection movement, indentation, casing
- `AvaloniaEditCommands`: `ToggleOverstrike` `DeleteLine` `ConvertTabsToSpaces`
- `SearchCommands`: `FindNext` `FindPrevious` `ReplaceNext` `ReplaceAll` `CloseSearchPanel`
- `RoutedCommandBinding`: `(command, exec, canExec)`

[TEXTMATE_TYPES]: TextMate tokenizer adapter (`AvaloniaEdit.TextMate`)

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                 |
| :-----: | :---------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `TextMate`                    | static class   | `InstallTextMate` extension host             |
|  [02]   | `TextMate.Installation`       | class          | grammar/theme session (`: IDisposable`)      |
|  [03]   | `TextEditorModel`             | class          | tokenizer model (`: AbstractLineList`)       |
|  [04]   | `TextMateColoringTransformer` | class          | token colorizer (`: GenericLineTransformer`) |
|  [05]   | `GenericLineTransformer`      | abstract class | line-transform base (`SetTextStyle`)         |
|  [06]   | `DocumentSnapshot`            | class          | immutable line snapshot for the tokenizer    |

- `TextEditorModel`: `DocumentSnapshot` `InvalidateViewPortLines`

## [03]-[ENTRYPOINTS]

[EDITOR_ENTRYPOINTS]: `TextEditor` content, state, IO, and change-grouping operations

| [INDEX] | [SURFACE]              | [SHAPE]  | [CAPABILITY]        |
| :-----: | :--------------------- | :------- | :------------------ |
|  [01]   | `Text`                 | property | text content        |
|  [02]   | `Document`             | property | document binding    |
|  [03]   | `SyntaxHighlighting`   | property | xshd highlighting   |
|  [04]   | `Options`              | property | behavior options    |
|  [05]   | `WordWrap`             | property | view posture        |
|  [06]   | `IsReadOnly`           | property | edit posture        |
|  [07]   | `ShowLineNumbers`      | property | line-number view    |
|  [08]   | `IsModified`           | property | dirty state         |
|  [09]   | `Encoding`             | property | text encoding       |
|  [10]   | `CaretOffset`          | property | caret state         |
|  [11]   | `SelectionStart`       | property | selection state     |
|  [12]   | `SelectionLength`      | property | selection span      |
|  [13]   | `SelectedText`         | property | selected content    |
|  [14]   | `Load(Stream)`         | instance | stream or file load |
|  [15]   | `Save(Stream)`         | instance | stream or file save |
|  [16]   | `AppendText(string)`   | instance | content edit        |
|  [17]   | `Clear()`              | instance | content edit        |
|  [18]   | `Delete()`             | instance | content edit        |
|  [19]   | `Select(int, int)`     | instance | selection edit      |
|  [20]   | `BeginChange()`        | instance | undo grouping       |
|  [21]   | `EndChange()`          | instance | undo grouping       |
|  [22]   | `DeclareChangeBlock()` | instance | undo group scope    |
|  [23]   | `Undo()`               | instance | undo navigation     |
|  [24]   | `Redo()`               | instance | undo navigation     |
|  [25]   | `Copy()`               | instance | clipboard           |
|  [26]   | `Cut()`                | instance | clipboard           |
|  [27]   | `Paste()`              | instance | clipboard           |
|  [28]   | `SelectAll()`          | instance | selection           |
|  [29]   | `ScrollTo(int, int)`   | instance | navigation          |
|  [30]   | `ScrollToLine(int)`    | instance | navigation          |
|  [31]   | `ScrollToEnd()`        | instance | navigation          |
|  [32]   | `DocumentChanged`      | event    | lifecycle hook      |
|  [33]   | `TextChanged`          | event    | lifecycle hook      |
|  [34]   | `OptionChanged`        | event    | lifecycle hook      |
|  [35]   | `PointerHover`         | event    | lifecycle hook      |
|  [36]   | `TextArea`             | property | editing accessor    |

- `DeclareChangeBlock()`: returns an `IDisposable`; a `using` scope records one reversible `UndoStack` step across multi-edit refactors.
- `Load(Stream)`: auto-detects encoding into `Encoding`; `IsModified` drives the dirty indicator.
- `TextArea`: mounts `FoldingManager.Install`, `CompletionWindow`, and `OverloadInsightWindow`; `SearchPanel.Install` mounts on `TextEditor`.

[FOLDING_ENTRYPOINTS]: `FoldingManager` lifecycle and query

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]    |
| :-----: | :------------------------------------ | :------- | :-------------- |
|  [01]   | `Install(TextArea) -> FoldingManager` | static   | margin install  |
|  [02]   | `Uninstall(FoldingManager)`           | static   | margin removal  |
|  [03]   | `UpdateFoldings(NewFolding[], int)`   | instance | folding refresh |
|  [04]   | `CreateFolding(int, int)`             | instance | manual fold     |
|  [05]   | `GetFoldingsContaining(int)`          | instance | fold query      |
|  [06]   | `GetFoldingsAt(int)`                  | instance | fold query      |
|  [07]   | `GetNextFolding(int)`                 | instance | fold query      |
|  [08]   | `AllFoldings`                         | property | fold set        |
|  [09]   | `Clear()`                             | instance | fold set        |
|  [10]   | `RemoveFolding(FoldingSection)`       | instance | fold set        |

- `UpdateFoldings`: its `int firstErrorOffset` drops folds past a syntax error; pass `-1` when the whole range is valid. A custom strategy produces `NewFolding` rows and calls it — `XmlFoldingStrategy` is the built-in example.

[COMPLETION_ENTRYPOINTS]: IntelliSense popup and item contract

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]   |
| :-----: | :-------------------------------- | :------- | :------------- |
|  [01]   | `CompletionWindow(TextArea)`      | ctor     | popup creation |
|  [02]   | `CompletionWindow.Show()`         | instance | popup open     |
|  [03]   | `CompletionList.CompletionData`   | property | item source    |
|  [04]   | `ICompletionData.Complete(...)`   | instance | item insertion |
|  [05]   | `CloseAutomatically`              | property | dismissal      |
|  [06]   | `CloseWhenCaretAtBeginning`       | property | dismissal      |
|  [07]   | `OverloadInsightWindow(TextArea)` | ctor     | popup creation |
|  [08]   | `Provider`                        | property | overload list  |
|  [09]   | `OverloadInsightWindow.Show()`    | instance | popup open     |

- `ICompletionData.Complete(TextArea, ISegment, EventArgs)`: mutates the `TextArea` over the trigger `ISegment` — insertion runs here, never direct document mutation. Implement one per suggestion, add rows to `CompletionList.CompletionData`, then `Show()`; the shell command rail feeds the rows.
- `OverloadInsightWindow`: construct over `TextArea`, set `Provider`, then `Show()` for multi-signature insight.

[SEARCH_ENTRYPOINTS]: regex search/replace panel and strategy

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]     |
| :-----: | :----------------------------------------------- | :------- | :--------------- |
|  [01]   | `Install(TextEditor) -> SearchPanel`             | static   | panel install    |
|  [02]   | `Open()`                                         | instance | overlay toggle   |
|  [03]   | `Close()`                                        | instance | overlay toggle   |
|  [04]   | `Reactivate()`                                   | instance | overlay toggle   |
|  [05]   | `FindNext(int)`                                  | instance | match navigation |
|  [06]   | `FindPrevious()`                                 | instance | match navigation |
|  [07]   | `ReplaceNext()`                                  | instance | replace          |
|  [08]   | `ReplaceAll()`                                   | instance | replace          |
|  [09]   | `ISearchStrategy.FindAll(ITextSource, int, int)` | instance | bulk search      |

- `SearchCommands.*`: `RoutedCommand` statics carry default `KeyGesture`s (`Ctrl+F`/`F3`/`Ctrl+H`) the shell command page binds; a programmatic count drives `ISearchStrategy.FindAll` directly.

[SNIPPET_AND_INDENT_ENTRYPOINTS]: template insertion and auto-indent

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]       |
| :-----: | :------------------------------------------------------------ | :------- | :----------------- |
|  [01]   | `Snippet.Insert(TextArea) -> InsertionContext`                | instance | snippet expansion  |
|  [02]   | `InsertionContext.InsertText(string)`                         | instance | placeholder wiring |
|  [03]   | `InsertionContext.Link(ISegment, ISegment[])`                 | instance | placeholder wiring |
|  [04]   | `IIndentationStrategy.IndentLine(TextDocument, DocumentLine)` | instance | auto-indent        |
|  [05]   | `IndentLines(TextDocument, int, int)`                         | instance | auto-indent        |
|  [06]   | `CSharpIndentationStrategy(TextEditorOptions)`                | ctor     | C# indenter        |

- `Snippet`: build from `SnippetTextElement` + `SnippetReplaceableTextElement` (tab-stops) + `SnippetCaretElement` parts, then `Insert(textArea)` drives the interactive placeholder session. Assign `TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options)` for newline re-indent.

[TEXTMATE_ENTRYPOINTS]: grammar/theme session bound to one editor

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]      |
| :-----: | :-------------------------------------------------- | :------- | :---------------- |
|  [01]   | `InstallTextMate(IRegistryOptions) -> Installation` | static   | session install   |
|  [02]   | `Installation.SetGrammar(string)`                   | instance | scope selection   |
|  [03]   | `Installation.SetGrammarFile(string)`               | instance | file grammar      |
|  [04]   | `Installation.SetTheme(IRawTheme)`                  | instance | theme apply       |
|  [05]   | `Installation.TryGetThemeColor(string, out string)` | instance | theme color query |
|  [06]   | `Installation.RegistryOptions`                      | property | session state     |
|  [07]   | `Installation.EditorModel`                          | property | tokenizer state   |
|  [08]   | `Installation.AppliedTheme`                         | event    | theme event       |
|  [09]   | `Installation.Dispose()`                            | instance | session teardown  |

- `InstallTextMate`: full form is `InstallTextMate(this TextEditor, IRegistryOptions, bool initCurrentDocument = true, Action<Exception>? exceptionHandler = null)`; the `exceptionHandler` captures off-UI-thread background-tokenizer faults.
- `SetGrammar(scope)`: takes a scope string such as `"source.cs"`, from `IRegistryOptions.GetScopeByExtension`.
- `TryGetThemeColor`: reads editor-chrome colors (`"editor.background"`) so the surrounding shell aligns to the grammar theme.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every editor surface is one `TextEditor` over a `TextDocument`; feature owners (`FoldingManager`, `SearchPanel`, `CompletionWindow`, `CSharpIndentationStrategy`) mount onto its `TextArea`/`TextView`, and each multi-edit folds through one `DeclareChangeBlock` undo step.

[STACKING]:
- `TextMateSharp`(`.api/api-textmatesharp.md`): `InstallTextMate` consumes an `IRegistryOptions`, `SetGrammar` a scope string, and `SetTheme` an `IRawTheme` that `LoadTheme(ThemeName)` returns; the provider owns the `IGrammar`/`ThemeName`/theme corpus and this adapter only forwards those handles.
- AppUi code pane: `TextEditor` + `TextDocument` wrap in a `ReactiveUserControl`; `FoldingManager.Install`/`SearchPanel.Install`/`CompletionWindow`/`CSharpIndentationStrategy` own structure, find, IntelliSense, and indent; the single `Installation` rides `TextView.LineTransformers` and disposes through `WhenActivated`'s `CompositeDisposable` when the editor unloads.

[LOCAL_ADMISSION]:
- Code-view intent admits as a `TextEditor` over `TextDocument` state with feature owners installed on its `TextArea`; a `TextBox`-derived custom editor is the deleted form.

[RAIL_LAW]:
- Package: `Avalonia.AvaloniaEdit`
- Owns: the code-editor control, rope document model, and the TextMate adapter binding one editor to a `TextMateSharp` provider.
- Accept: edits grouped through `DeclareChangeBlock`; completion insertion through `ICompletionData.Complete`; one `Installation` per editor against one `IRegistryOptions`, where TextMate colorization replaces xshd per editor and raises `AppliedTheme` for chrome to follow.
- Reject: direct document mutation bypassing the `UndoStack` or `ICompletionData.Complete`; a second `InstallTextMate` where the standalone `Registry`/`IGrammar.TokenizeLine` rail serves a non-editor surface; treating `IRegistryOptions`/`ThemeName` as AvaloniaEdit types.
