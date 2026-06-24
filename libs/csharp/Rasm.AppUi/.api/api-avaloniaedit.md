# [RASM_APPUI_API_AVALONIAEDIT]

`Avalonia.AvaloniaEdit` is a full code-editor control: a rope-backed `TextDocument`, a `TextArea`/`TextView` rendering stack with pluggable colorizing transformers and element generators, undo grouping, code folding, xshd/`IHighlightingDefinition` highlighting, a `CompletionWindow`/`ICompletionData` IntelliSense surface, an overload-insight popup, a regex search/replace panel, a snippet engine, and indentation strategies. `AvaloniaEdit.TextMate` bolts a TextMate tokenizer onto one editor: `InstallTextMate(IRegistryOptions)` registers a `TextMateColoringTransformer` driven by a `TextMateSharp` `Registry`. The critical integration fact this catalog surfaces: the registry, grammar scope names, and themes are owned by the transitively-pulled `TextMateSharp` / `TextMateSharp.Grammars` (`2.0.3`) packages, not by AvaloniaEdit.TextMate — the notebook/inspector design pages stack onto `RegistryOptions(ThemeName.…)` / `GetScopeByExtension` / `LoadTheme` from there, and the `IGrammar.TokenizeLine` / `IToken` token rail + the four-member `IRegistryOptions` contract are documented at first-class depth in `api-textmatesharp.md`. This catalog owns only the AvaloniaEdit adapter (`InstallTextMate`, `Installation.SetGrammar`/`SetTheme`, `TextEditorModel`); the provider surface those forward to is `api-textmatesharp.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.AvaloniaEdit`
- package: `Avalonia.AvaloniaEdit`
- version: `12.0.0`
- license: `MIT`
- assembly: `AvaloniaEdit` (note: assembly name ≠ package id; compiled-XAML resources embedded)
- build-floor: `net10.0` (consumer-bound; `net8.0` fallback present, not bound)
- namespace: `AvaloniaEdit` (control + routed commands), `.Document`, `.Editing`, `.Folding`, `.Highlighting`(+`.Xshd`), `.CodeCompletion`, `.Search`, `.Snippets`, `.Indentation`(+`.CSharp`), `.Rendering`
- asset: runtime library
- rail: editor

[PACKAGE_SURFACE]: `AvaloniaEdit.TextMate`
- package: `AvaloniaEdit.TextMate`
- version: `12.0.0`
- license: `MIT`
- assembly: `AvaloniaEdit.TextMate`
- build-floor: `net10.0`
- namespace: `AvaloniaEdit.TextMate`
- asset: runtime library
- rail: editor
- depends: `TextMateSharp` + `TextMateSharp.Grammars` (`2.0.3`, transitive; native regex via `Onigwrap 1.0.10`) — supplies `IRegistryOptions`/`RegistryOptions`, `IGrammar`, `IRawTheme`/`ThemeName`, the `TMModel` background tokenizer, and the bundled 50-grammar / 21-theme corpus, all catalogued in `api-textmatesharp.md`. `IRegistryOptions`, scope names (`"source.cs"`), and `ThemeName` are TextMateSharp types; AvaloniaEdit.TextMate only adapts them to the editor (`TextEditorModel`/`DocumentSnapshot` are an `IModelLines`/`IModelTokensChangedListener` adapter over the editor's `TextDocument`).

## [02]-[PUBLIC_TYPES]

[EDITOR_TYPES]: control, options, editing surface, and rope document model
- rail: editor

| [INDEX] | [SYMBOL]            | [SIGNATURE]                                                  | [RAIL]            |
| :-----: | :------------------ | :---------------------------------------------------------- | :---------------- |
|  [01]   | `TextEditor`        | `class TextEditor : TemplatedControl, ITextEditorComponent` | editor control    |
|  [02]   | `TextEditorOptions` | `class` — wrap/indent/whitespace/hyperlink policy           | editor options    |
|  [03]   | `TextArea`          | `class : TemplatedControl` — input handlers, selection, caret | editing surface |
|  [04]   | `Caret`             | `class` (`Offset`, `Line`, `Column`, `Location`)            | caret state       |
|  [05]   | `Selection`         | `abstract class` (+ `SimpleSelection`/`RectangleSelection`) | selection model   |
|  [06]   | `TextDocument`      | `class : IDocument` — rope text, `BeginUpdate`/`Replace`    | document model    |
|  [07]   | `DocumentLine`      | `class : IDocumentLine` (`Offset`, `Length`, `LineNumber`)  | line model        |
|  [08]   | `TextAnchor`        | `class : ITextAnchor` — survives edits                      | position anchor   |
|  [09]   | `TextSegment` / `TextSegmentCollection<T>` | `class` / red-black segment tree         | segment + tree    |
|  [10]   | `UndoStack`         | `class` (`Undo`/`Redo`/`StartUndoGroup`/`SizeLimit`)        | undo history      |
|  [11]   | `TextViewPosition`  | `struct` (`Line`, `Column`, `VisualColumn`)                 | visual position   |

[FEATURE_TYPES]: folding, highlighting engine, completion, search, snippets, indentation
- rail: editor

| [INDEX] | [SYMBOL]                        | [SIGNATURE]                                                       | [RAIL]              |
| :-----: | :------------------------------ | :--------------------------------------------------------------- | :------------------ |
|  [01]   | `FoldingManager`                | `class` (static `Install(TextArea)`)                             | folding owner       |
|  [02]   | `FoldingSection`                | `class : TextSegment` (`IsFolded`, `Title`)                      | folded region       |
|  [03]   | `NewFolding`                    | `class` (`StartOffset`/`EndOffset`/`Name`/`DefaultClosed`)       | folding input       |
|  [04]   | `XmlFoldingStrategy`            | `class` (`UpdateFoldings(FoldingManager, TextDocument)`)         | XML folding         |
|  [05]   | `HighlightingManager`           | `class` (`Instance`, `GetDefinition`, `RegisterHighlighting`)    | definition registry |
|  [06]   | `IHighlightingDefinition`       | `interface` (`MainRuleSet`, `GetNamedColor`, `Properties`)       | definition contract |
|  [07]   | `HighlightingLoader`            | `static class` (`Load(XmlReader, IHighlightingDefinitionReferenceResolver)`) | xshd loader |
|  [08]   | `DocumentHighlighter` / `IHighlighter` | `class` / `interface` — programmatic line highlight        | highlight engine    |
|  [09]   | `DocumentColorizingTransformer` | `abstract class : ColorizingTransformer` (`ColorizeLine`)        | line colorizer      |
|  [10]   | `IBackgroundRenderer`           | `interface` (`Layer`, `Draw(TextView, DrawingContext)`)         | layer renderer      |
|  [11]   | `CompletionWindow`              | `class : CompletionWindowBase` (`CompletionList`)                | completion popup    |
|  [12]   | `ICompletionData`               | `interface` (`Text`/`Content`/`Description`/`Priority`/`Complete`) | completion item   |
|  [13]   | `OverloadInsightWindow` / `IOverloadProvider` | `class` / `interface` (`SelectedIndex`/`Count`/`CurrentHeader`) | overload insight |
|  [14]   | `SearchPanel`                   | `class` (static `Install(TextEditor)`)                          | search overlay      |
|  [15]   | `ISearchStrategy` / `RegexSearchStrategy` | `interface` (`FindAll`/`FindNext`) / regex impl        | search engine       |
|  [16]   | `Snippet` / `SnippetTextElement` / `SnippetReplaceableTextElement` / `SnippetBoundElement` / `SnippetCaretElement` / `SnippetSelectionElement` | snippet tree + placeholders | snippet engine |
|  [17]   | `IIndentationStrategy` / `CSharpIndentationStrategy` | `interface` (`IndentLine`/`IndentLines`) / C# impl | indentation     |

[RENDERING_TYPES]: the `TextView` extension surface for custom visuals (`AvaloniaEdit.Rendering`)
- rail: editor

| [INDEX] | [SYMBOL]                       | [SIGNATURE]                                                       | [RAIL]              |
| :-----: | :----------------------------- | :--------------------------------------------------------------- | :------------------ |
|  [01]   | `TextView`                     | `class : Control` (`LineTransformers`, `BackgroundRenderers`, `ElementGenerators`, `Redraw`) | render host |
|  [02]   | `VisualLineElementGenerator`   | `abstract class` (`GetFirstInterestedOffset`, `ConstructElement`)| inline element gen  |
|  [03]   | `LinkElementGenerator` / `MailLinkElementGenerator` | hyperlink/mail element generators            | link elements       |
|  [04]   | `IVisualLineTransformer`       | `interface` (`Transform(ITextRunConstructionContext, IList<VisualLineElement>)`) | line transform |
|  [05]   | `KnownLayer` / `BackgroundGeometryBuilder` | `enum` / geometry builder for overlay rendering      | layer placement     |

[COMMAND_TYPES]: routed-command surface for keybinding/menu wiring (`AvaloniaEdit`)
- rail: editor

| [INDEX] | [SYMBOL]              | [SHAPE]                                                                | [RAIL]            |
| :-----: | :-------------------- | :-------------------------------------------------------------------- | :---------------- |
|  [01]   | `ApplicationCommands` | `RoutedCommand` statics: `Copy`/`Cut`/`Paste`/`Delete`/`SelectAll`/`Undo`/`Redo`/`Find`/`Replace` | clipboard/edit |
|  [02]   | `EditingCommands`     | `RoutedCommand` statics: caret/selection movement, indent, casing      | caret/selection   |
|  [03]   | `AvaloniaEditCommands`| `RoutedCommand` statics: `ToggleOverstrike`, `DeleteLine`, `ConvertTabsToSpaces`, … | editor-specific |
|  [04]   | `RoutedCommand` / `RoutedCommandBinding` | `class` — command + `(command, exec, canExec)` binding   | command binding   |
|  [05]   | `SearchCommands`      | `RoutedCommand` statics: `FindNext`/`FindPrevious`/`ReplaceNext`/`ReplaceAll`/`CloseSearchPanel` | search keys |

[TEXTMATE_TYPES]: TextMate tokenizer adapter (`AvaloniaEdit.TextMate`)
- rail: editor

| [INDEX] | [SYMBOL]                      | [SIGNATURE]                                                       | [RAIL]              |
| :-----: | :---------------------------- | :--------------------------------------------------------------- | :------------------ |
|  [01]   | `TextMate`                    | `static class` (`InstallTextMate` extension)                     | install extension   |
|  [02]   | `TextMate.Installation`       | `class : IDisposable` — grammar/theme session                    | grammar session     |
|  [03]   | `TextEditorModel`             | `class : AbstractLineList` (`DocumentSnapshot`, `InvalidateViewPortLines`) | tokenizer model |
|  [04]   | `TextMateColoringTransformer` | `class : GenericLineTransformer` (`SetGrammar`/`SetTheme`/`SetModel`) | token colorizer  |
|  [05]   | `GenericLineTransformer`      | `abstract class : DocumentColorizingTransformer` (`SetTextStyle(...)`) | line transform base |
|  [06]   | `DocumentSnapshot`            | `class` — immutable line snapshot for the background tokenizer    | tokenizer snapshot  |

## [03]-[ENTRYPOINTS]

[EDITOR_ENTRYPOINTS]: `TextEditor` content, state, IO, and change-grouping operations
- rail: editor

| [INDEX] | [SURFACE]            | [SIGNATURE]                                                   | [RAIL]            |
| :-----: | :------------------- | :----------------------------------------------------------- | :---------------- |
|  [01]   | `Text`               | `string Text { get; set; }`                                  | text content      |
|  [02]   | `Document`           | `TextDocument Document { get; set; }`                        | document binding  |
|  [03]   | `SyntaxHighlighting` | `IHighlightingDefinition SyntaxHighlighting { get; set; }`   | xshd highlighting |
|  [04]   | `Options`            | `TextEditorOptions Options { get; set; }`                    | behavior options  |
|  [05]   | `WordWrap` / `IsReadOnly` / `ShowLineNumbers` | `bool` styled props                         | view posture      |
|  [06]   | `IsModified` / `Encoding` | `bool IsModified` / `Encoding Encoding`                  | dirty + encoding  |
|  [07]   | `CaretOffset` / `SelectionStart` / `SelectionLength` / `SelectedText` | `int` / `int` / `int` / `string` | selection state |
|  [08]   | `Load` / `Save`      | `void Load(Stream)` / `Load(string fileName)` ; `void Save(Stream)` / `Save(string)` | stream/file IO |
|  [09]   | `AppendText` / `Clear` / `Delete` / `Select` | `void AppendText(string)` / `Clear()` / `Delete()` / `Select(int start, int length)` | content edit |
|  [10]   | `BeginChange` / `EndChange` / `DeclareChangeBlock` | `void` / `void` / `IDisposable` — undo grouping | atomic edit  |
|  [11]   | `Undo` / `Redo`      | `bool Undo()` / `bool Redo()`                                | undo navigation   |
|  [12]   | `Copy` / `Cut` / `Paste` / `SelectAll` | clipboard + select-all                     | clipboard         |
|  [13]   | `ScrollTo` / `ScrollToLine` / `ScrollToEnd` | `void ScrollTo(int line, int column)` / `ScrollToLine(int)` / `ScrollToEnd()` | navigation |
|  [14]   | `DocumentChanged` / `TextChanged` / `OptionChanged` / `PointerHover` | `event` — editor lifecycle hooks | events  |

Wrap multi-edit refactors in `using (editor.DeclareChangeBlock())` (or `BeginChange()`/`EndChange()`) so the `UndoStack` records one reversible step. `Load(Stream)` auto-detects encoding into `Encoding`; `IsModified` drives the dirty indicator.

[FOLDING_ENTRYPOINTS]: `FoldingManager` lifecycle and query
- rail: editor

| [INDEX] | [SURFACE]               | [SIGNATURE]                                                   | [RAIL]           |
| :-----: | :---------------------- | :----------------------------------------------------------- | :--------------- |
|  [01]   | `Install`               | `static FoldingManager Install(TextArea)`                    | margin install   |
|  [02]   | `Uninstall`             | `static void Uninstall(FoldingManager)`                      | margin removal   |
|  [03]   | `UpdateFoldings`        | `void UpdateFoldings(IEnumerable<NewFolding>, int firstErrorOffset)` | folding refresh |
|  [04]   | `CreateFolding`         | `FoldingSection CreateFolding(int startOffset, int endOffset)` | manual fold    |
|  [05]   | `GetFoldingsContaining` / `GetFoldingsAt` / `GetNextFolding` | `ReadOnlyCollection<FoldingSection>` / `…` / `FoldingSection` | fold query |
|  [06]   | `AllFoldings` / `Clear` / `RemoveFolding` | `IEnumerable<FoldingSection>` / `void` / `void` | fold set ops |

`UpdateFoldings` takes a `firstErrorOffset` (folds past a syntax error are dropped); pass `-1` when the whole range is valid. A custom folding strategy produces `NewFolding` rows and calls `UpdateFoldings` — `XmlFoldingStrategy` is the built-in example.

[COMPLETION_ENTRYPOINTS]: IntelliSense popup and item contract
- rail: editor

| [INDEX] | [SURFACE]            | [SIGNATURE]                                                              | [RAIL]            |
| :-----: | :------------------- | :---------------------------------------------------------------------- | :---------------- |
|  [01]   | `.ctor` + `Show`     | `new CompletionWindow(TextArea)`; populate `CompletionList.CompletionData`; `Show()` | popup open |
|  [02]   | `CompletionData`     | `IList<ICompletionData> CompletionList.CompletionData { get; }`          | item source       |
|  [03]   | `Complete`           | `void ICompletionData.Complete(TextArea, ISegment completionSegment, EventArgs)` | item insertion |
|  [04]   | `CloseAutomatically` / `CloseWhenCaretAtBeginning` | `bool` window dismissal policy             | dismissal         |
|  [05]   | `Provider`           | `IOverloadProvider OverloadInsightWindow.Provider { get; set; }`        | overload list     |

Implement `ICompletionData` per suggestion (an `Image`/`Content`/`Description`/`Priority` row whose `Complete` mutates the `TextArea` over the trigger `ISegment`), add rows to `CompletionList.CompletionData`, then `Show()`. The shell command rail (Compute-receipt-backed suggestions) feeds these rows; insertion runs through `Complete`, not direct document mutation.

[SEARCH_ENTRYPOINTS]: regex search/replace panel and strategy
- rail: editor

| [INDEX] | [SURFACE]            | [SIGNATURE]                                                   | [RAIL]            |
| :-----: | :------------------- | :----------------------------------------------------------- | :---------------- |
|  [01]   | `Install`            | `static SearchPanel Install(TextEditor)`                     | panel install     |
|  [02]   | `Open` / `Close` / `Reactivate` | `void` — panel visibility                         | overlay toggle    |
|  [03]   | `FindNext` / `FindPrevious` | `void FindNext(int startOffset = -1)` / `FindPrevious()` | match navigation |
|  [04]   | `ReplaceNext` / `ReplaceAll` | `void` — single/bulk replace                        | replace           |
|  [05]   | `FindAll`            | `IEnumerable<ISearchResult> ISearchStrategy.FindAll(ITextSource, int offset, int length)` | bulk search |

`SearchCommands.*` (`RoutedCommand` statics with default `KeyGesture`s — Ctrl+F/F3/Ctrl+H) are the keybinding entrypoints the shell command page binds; a programmatic count uses `ISearchStrategy.FindAll` directly.

[SNIPPET_AND_INDENT_ENTRYPOINTS]: template insertion and auto-indent
- rail: editor

| [INDEX] | [SURFACE]            | [SIGNATURE]                                                   | [RAIL]            |
| :-----: | :------------------- | :----------------------------------------------------------- | :---------------- |
|  [01]   | `Insert`             | `InsertionContext Snippet.Insert(TextArea)`                  | snippet expansion |
|  [02]   | `InsertText` / `Link` | `void InsertionContext.InsertText(string)` / `Link(ISegment, ISegment[])` | placeholder wiring |
|  [03]   | `IndentLine` / `IndentLines` | `void IIndentationStrategy.IndentLine(TextDocument, DocumentLine)` / `IndentLines(TextDocument, int begin, int end)` | auto-indent |
|  [04]   | `.ctor`              | `new CSharpIndentationStrategy(TextEditorOptions)`           | C# indenter       |

Build a `Snippet` from `SnippetTextElement` + `SnippetReplaceableTextElement` (tab-stops) + `SnippetCaretElement` parts, then `Insert(textArea)` drives the interactive placeholder session. Assign `TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options)` for newline re-indent.

[TEXTMATE_ENTRYPOINTS]: grammar/theme session bound to one editor
- rail: editor

| [INDEX] | [SURFACE]          | [SIGNATURE]                                                                              | [RAIL]            |
| :-----: | :----------------- | :-------------------------------------------------------------------------------------- | :---------------- |
|  [01]   | `InstallTextMate`  | `Installation InstallTextMate(this TextEditor, IRegistryOptions, bool initCurrentDocument = true, Action<Exception>? exceptionHandler = null)` | session install |
|  [02]   | `SetGrammar`       | `void Installation.SetGrammar(string scopeName)` (e.g. `"source.cs"`)                   | scope selection   |
|  [03]   | `SetGrammarFile`   | `void Installation.SetGrammarFile(string path)`                                         | file grammar      |
|  [04]   | `SetTheme`         | `void Installation.SetTheme(IRawTheme theme)` (`TextMateSharp.Themes`)                   | theme apply       |
|  [05]   | `TryGetThemeColor` | `bool Installation.TryGetThemeColor(string colorKey, out string colorString)`           | theme color query |
|  [06]   | `RegistryOptions` / `EditorModel` | `IRegistryOptions RegistryOptions { get; }` / `TextEditorModel EditorModel { get; }` | session state |
|  [07]   | `AppliedTheme`     | `event EventHandler<Installation> AppliedTheme`                                          | theme event       |
|  [08]   | `Dispose`          | `void Installation.Dispose()`                                                           | session teardown  |

`InstallTextMate` takes an `IRegistryOptions` — construct `new RegistryOptions(ThemeName.DarkPlus)` from `TextMateSharp.Grammars`, then `SetGrammar(registryOptions.GetScopeByExtension(".cs"))`. The `exceptionHandler` captures background-tokenizer faults (the tokenizer runs off the UI thread). `TryGetThemeColor` reads editor-chrome colors (`"editor.background"`) so the theme page aligns the surrounding shell to the grammar theme.

## [04]-[INTEGRATION_LAW]

[EDITOR_RAIL_LAW]:
- Stack: a code surface is `TextEditor` + `Document` (`TextDocument`) wrapped in a `ReactiveUserControl`; `FoldingManager.Install(TextArea)` + a folding strategy own structure; `SearchPanel.Install(editor)` owns find/replace; a `CompletionWindow` populated with `ICompletionData` rows (sourced from Compute receipts) owns IntelliSense; `CSharpIndentationStrategy` owns newline indent. Edits are grouped through `DeclareChangeBlock()` for one undo step.
- Accept: code-view intent maps to `TextEditor` over `TextDocument` state with the feature owners installed onto its `TextArea`/`TextView`.
- Reject: `TextBox`-derived custom code editors; direct document mutation that bypasses `ICompletionData.Complete` / the `UndoStack`.

[GRAMMAR_RAIL_LAW]:
- Stack: tokenization is `editor.InstallTextMate(registryOptions)` -> `installation.SetGrammar(scope)` -> `installation.SetTheme(registryOptions.LoadTheme(ThemeName.…))`, where `registryOptions`, scope strings, and theme handles all come from `TextMateSharp.Grammars` (`api-textmatesharp.md` — `[REGISTRY_ENTRYPOINTS]` owns `RegistryOptions`/`GetScopeByExtension`/`LoadTheme`, and the custom `IRegistryOptions` that registers `source.rasm`/`source.rasm-expression` is its `[LOCATOR_RAIL_LAW]`). The single `Installation` owns the `TextMateColoringTransformer` on `TextView.LineTransformers`; theme changes raise `AppliedTheme` for the surrounding shell to follow (chrome colors read `Theme.GetGuiColorDictionary` via `TryGetThemeColor`).
- Accept: grammar and theme flow through one `Installation` against an `IRegistryOptions`; the editor `SyntaxHighlighting` (xshd) path and the TextMate path are mutually exclusive per editor — TextMate replaces xshd colorization. A non-editor surface that needs the same coloring drives the standalone `Registry`/`IGrammar.TokenizeLine` rail from `api-textmatesharp.md`, never a second `InstallTextMate`.
- Reject: per-keystroke manual recolorization outside the installation transformers; treating `IRegistryOptions`/`ThemeName` as AvaloniaEdit types (they are `TextMateSharp`); leaking the `Installation` (it must be disposed when the editor unloads — wire it through `WhenActivated`'s `CompositeDisposable`).
