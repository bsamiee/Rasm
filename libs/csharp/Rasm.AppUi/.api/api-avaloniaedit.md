# [RASM_APPUI_API_AVALONIAEDIT]

`Avalonia.AvaloniaEdit` is a full code-editor control: a rope-backed `TextDocument`, a `TextArea`/`TextView` rendering stack with pluggable colorizing transformers and element generators, undo grouping, code folding, xshd/`IHighlightingDefinition` highlighting, a `CompletionWindow`/`ICompletionData` IntelliSense surface, an overload-insight popup, a regex search/replace panel, a snippet engine, and indentation strategies. `AvaloniaEdit.TextMate` bolts a TextMate tokenizer onto one editor: `InstallTextMate(IRegistryOptions)` registers a `TextMateColoringTransformer` driven by a `TextMateSharp` `Registry`. The critical integration fact this catalog surfaces: the registry, grammar scope names, and themes are owned by the transitively-pulled `TextMateSharp` / `TextMateSharp.Grammars` (`2.0.3`) packages, not by AvaloniaEdit.TextMate — the notebook/inspector design pages stack onto `RegistryOptions(ThemeName.…)` / `GetScopeByExtension` / `LoadTheme` from there, and the `IGrammar.TokenizeLine` / `IToken` token rail + the four-member `IRegistryOptions` contract are documented at first-class depth in `api-textmatesharp.md`. This catalog owns only the AvaloniaEdit adapter (`InstallTextMate`, `Installation.SetGrammar`/`SetTheme`, `TextEditorModel`); the provider surface those forward to is `api-textmatesharp.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.AvaloniaEdit`
- package: `Avalonia.AvaloniaEdit`
- license: `MIT`
- assembly: `AvaloniaEdit` (note: assembly name ≠ package id; compiled-XAML resources embedded)
- build-floor: `net10.0` (consumer-bound; `net8.0` fallback present, not bound)
- namespace: `AvaloniaEdit` (control + routed commands), `.Document`, `.Editing`, `.Folding`, `.Highlighting`(+`.Xshd`), `.CodeCompletion`, `.Search`, `.Snippets`, `.Indentation`(+`.CSharp`), `.Rendering`
- asset: runtime library
- rail: editor

[PACKAGE_SURFACE]: `AvaloniaEdit.TextMate`
- package: `AvaloniaEdit.TextMate`
- license: `MIT`
- assembly: `AvaloniaEdit.TextMate`
- build-floor: `net10.0`
- namespace: `AvaloniaEdit.TextMate`
- asset: runtime library
- rail: editor
- depends: `TextMateSharp` + `TextMateSharp.Grammars` (`2.0.3`, transitive); native regex rides `Onigwrap 1.0.10`.
- provider: `TextMateSharp` owns `IRegistryOptions`/`RegistryOptions`, `IGrammar`, `IRawTheme`/`ThemeName`, `TMModel`, and the bundled 50-grammar / 21-theme corpus catalogued in `api-textmatesharp.md`.
- provider-types: `IRegistryOptions`, scope names such as `"source.cs"`, and `ThemeName` are TextMateSharp types.
- adapter: AvaloniaEdit.TextMate adapts the provider to the editor; `TextEditorModel`/`DocumentSnapshot` implement the `IModelLines`/`IModelTokensChangedListener` bridge over `TextDocument`.

## [02]-[PUBLIC_TYPES]

[EDITOR_TYPES]: control, options, editing surface, and rope document model
- rail: editor

| [INDEX] | [SYMBOL]                                   | [SIGNATURE]                                                   | [RAIL]          |
| :-----: | :----------------------------------------- | :------------------------------------------------------------ | :-------------- |
|  [01]   | `TextEditor`                               | `class TextEditor : TemplatedControl, ITextEditorComponent`   | editor control  |
|  [02]   | `TextEditorOptions`                        | `class` — wrap/indent/whitespace/hyperlink policy             | editor options  |
|  [03]   | `TextArea`                                 | `class : TemplatedControl` — input handlers, selection, caret | editing surface |
|  [04]   | `Caret`                                    | `class` (`Offset`, `Line`, `Column`, `Location`)              | caret state     |
|  [05]   | `Selection`                                | `abstract class` (+ `SimpleSelection`/`RectangleSelection`)   | selection model |
|  [06]   | `TextDocument`                             | `class : IDocument` — rope text, `BeginUpdate`/`Replace`      | document model  |
|  [07]   | `DocumentLine`                             | `class : IDocumentLine` (`Offset`, `Length`, `LineNumber`)    | line model      |
|  [08]   | `TextAnchor`                               | `class : ITextAnchor` — survives edits                        | position anchor |
|  [09]   | `TextSegment` / `TextSegmentCollection<T>` | `class` / red-black segment tree                              | segment + tree  |
|  [10]   | `UndoStack`                                | `class` (`Undo`/`Redo`/`StartUndoGroup`/`SizeLimit`)          | undo history    |
|  [11]   | `TextViewPosition`                         | `struct` (`Line`, `Column`, `VisualColumn`)                   | visual position |

[FEATURE_TYPES]: folding, highlighting, completion, search, snippets, and indentation
- rail: editor

| [INDEX] | [SYMBOL]                        | [KIND]                                   | [ROLE]               |
| :-----: | :------------------------------ | :--------------------------------------- | :------------------- |
|  [01]   | `FoldingManager`                | `class`                                  | folding owner        |
|  [02]   | `FoldingSection`                | `class : TextSegment`                    | folded region        |
|  [03]   | `NewFolding`                    | `class`                                  | folding input        |
|  [04]   | `XmlFoldingStrategy`            | `class`                                  | XML folding          |
|  [05]   | `HighlightingManager`           | `class`                                  | definition registry  |
|  [06]   | `IHighlightingDefinition`       | `interface`                              | definition contract  |
|  [07]   | `HighlightingLoader`            | `static class`                           | xshd loader          |
|  [08]   | `DocumentHighlighter`           | `class`                                  | highlight engine     |
|  [09]   | `IHighlighter`                  | `interface`                              | highlight contract   |
|  [10]   | `DocumentColorizingTransformer` | `abstract class : ColorizingTransformer` | line colorizer       |
|  [11]   | `IBackgroundRenderer`           | `interface`                              | layer renderer       |
|  [12]   | `CompletionWindow`              | `class : CompletionWindowBase`           | completion popup     |
|  [13]   | `ICompletionData`               | `interface`                              | completion item      |
|  [14]   | `OverloadInsightWindow`         | `class`                                  | overload popup       |
|  [15]   | `IOverloadProvider`             | `interface`                              | overload contract    |
|  [16]   | `SearchPanel`                   | `class`                                  | search overlay       |
|  [17]   | `ISearchStrategy`               | `interface`                              | search contract      |
|  [18]   | `RegexSearchStrategy`           | regex implementation                     | search engine        |
|  [19]   | `Snippet`                       | snippet tree                             | snippet engine       |
|  [20]   | `SnippetTextElement`            | snippet tree member                      | snippet engine       |
|  [21]   | `SnippetReplaceableTextElement` | snippet tree member                      | snippet engine       |
|  [22]   | `SnippetBoundElement`           | snippet tree member                      | snippet engine       |
|  [23]   | `SnippetCaretElement`           | snippet tree member                      | snippet engine       |
|  [24]   | `SnippetSelectionElement`       | snippet tree member                      | snippet engine       |
|  [25]   | `IIndentationStrategy`          | `interface`                              | indentation contract |
|  [26]   | `CSharpIndentationStrategy`     | C# implementation                        | indentation          |

[FEATURE_MEMBER_SHAPES]:
- `FoldingManager`: static `Install(TextArea)`
- `FoldingSection`: `IsFolded` and `Title`
- `NewFolding`: `StartOffset`, `EndOffset`, `Name`, and `DefaultClosed`
- `XmlFoldingStrategy`: `UpdateFoldings(FoldingManager, TextDocument)`
- `HighlightingManager`: `Instance`, `GetDefinition`, and `RegisterHighlighting`
- `IHighlightingDefinition`: `MainRuleSet`, `GetNamedColor`, and `Properties`
- `HighlightingLoader`: `Load(XmlReader, IHighlightingDefinitionReferenceResolver)`
- `DocumentHighlighter`/`IHighlighter`: programmatic line highlighting
- `DocumentColorizingTransformer`: `ColorizeLine`
- `IBackgroundRenderer`: `Layer` and `Draw(TextView, DrawingContext)`
- `CompletionWindow`: `CompletionList`
- `ICompletionData`: `Image` (`IImage`), `Text`, `Content`, `Description`, `Priority`, and `Complete`
- `OverloadInsightWindow`: `.ctor(TextArea)` and `Provider`
- `IOverloadProvider`: `SelectedIndex`, `Count`, `CurrentHeader`, and `CurrentContent`
- `SearchPanel`: static `Install(TextEditor)`
- `ISearchStrategy`: `FindAll` and `FindNext`; `RegexSearchStrategy` implements regex search.
- `IIndentationStrategy`: `IndentLine` and `IndentLines`; `CSharpIndentationStrategy` implements C# indentation.

[RENDERING_TYPES]: the `TextView` extension surface for custom visuals (`AvaloniaEdit.Rendering`)
- rail: editor

| [INDEX] | [SYMBOL]                     | [KIND]            | [ROLE]              |
| :-----: | :--------------------------- | :---------------- | :------------------ |
|  [01]   | `TextView`                   | `class : Control` | render host         |
|  [02]   | `VisualLineElementGenerator` | `abstract class`  | inline elements     |
|  [03]   | `LinkElementGenerator`       | element generator | hyperlinks          |
|  [04]   | `MailLinkElementGenerator`   | element generator | mail links          |
|  [05]   | `IVisualLineTransformer`     | `interface`       | line transformation |
|  [06]   | `KnownLayer`                 | `enum`            | layer identity      |
|  [07]   | `BackgroundGeometryBuilder`  | geometry builder  | overlay geometry    |

[RENDERING_MEMBER_SHAPES]:
- `TextView`: `LineTransformers`, `BackgroundRenderers`, `ElementGenerators`, and `Redraw`
- `VisualLineElementGenerator`: `GetFirstInterestedOffset` and `ConstructElement`
- `IVisualLineTransformer`: `Transform(ITextRunConstructionContext, IList<VisualLineElement>)`

[COMMAND_TYPES]: routed-command surface for keybinding/menu wiring (`AvaloniaEdit`)
- rail: editor

| [INDEX] | [SYMBOL]               | [KIND]                  | [ROLE]          |
| :-----: | :--------------------- | :---------------------- | :-------------- |
|  [01]   | `ApplicationCommands`  | `RoutedCommand` statics | clipboard/edit  |
|  [02]   | `EditingCommands`      | `RoutedCommand` statics | caret/selection |
|  [03]   | `AvaloniaEditCommands` | `RoutedCommand` statics | editor-specific |
|  [04]   | `RoutedCommand`        | `class`                 | command         |
|  [05]   | `RoutedCommandBinding` | `class`                 | binding         |
|  [06]   | `SearchCommands`       | `RoutedCommand` statics | search keys     |

[COMMAND_MEMBERS]:
- `ApplicationCommands`: `Copy`, `Cut`, `Paste`, `Delete`, `SelectAll`, `Undo`, `Redo`, `Find`, and `Replace`
- `EditingCommands`: caret and selection movement, indentation, and casing
- `AvaloniaEditCommands`: `ToggleOverstrike`, `DeleteLine`, `ConvertTabsToSpaces`, …
- `RoutedCommandBinding`: `(command, exec, canExec)`
- `SearchCommands`: `FindNext`, `FindPrevious`, `ReplaceNext`, `ReplaceAll`, and `CloseSearchPanel`

[TEXTMATE_TYPES]: TextMate tokenizer adapter (`AvaloniaEdit.TextMate`)
- rail: editor

| [INDEX] | [SYMBOL]                      | [SIGNATURE]                                                                | [RAIL]              |
| :-----: | :---------------------------- | :------------------------------------------------------------------------- | :------------------ |
|  [01]   | `TextMate`                    | `static class` (`InstallTextMate` extension)                               | install extension   |
|  [02]   | `TextMate.Installation`       | `class : IDisposable` — grammar/theme session                              | grammar session     |
|  [03]   | `TextEditorModel`             | `class : AbstractLineList` (`DocumentSnapshot`, `InvalidateViewPortLines`) | tokenizer model     |
|  [04]   | `TextMateColoringTransformer` | `class : GenericLineTransformer` (`SetGrammar`/`SetTheme`/`SetModel`)      | token colorizer     |
|  [05]   | `GenericLineTransformer`      | `abstract class : DocumentColorizingTransformer` (`SetTextStyle(...)`)     | line transform base |
|  [06]   | `DocumentSnapshot`            | `class` — immutable line snapshot for the background tokenizer             | tokenizer snapshot  |

## [03]-[ENTRYPOINTS]

[EDITOR_ENTRYPOINTS]: `TextEditor` content, state, IO, and change-grouping operations
- rail: editor

| [INDEX] | [SURFACE]            | [SIGNATURE]                                                | [ROLE]             |
| :-----: | :------------------- | :--------------------------------------------------------- | :----------------- |
|  [01]   | `Text`               | `string Text { get; set; }`                                | text content       |
|  [02]   | `Document`           | `TextDocument Document { get; set; }`                      | document binding   |
|  [03]   | `SyntaxHighlighting` | `IHighlightingDefinition SyntaxHighlighting { get; set; }` | xshd highlighting  |
|  [04]   | `Options`            | `TextEditorOptions Options { get; set; }`                  | behavior options   |
|  [05]   | `WordWrap`           | `bool` styled property                                     | view posture       |
|  [06]   | `IsReadOnly`         | `bool` styled property                                     | edit posture       |
|  [07]   | `ShowLineNumbers`    | `bool` styled property                                     | line-number view   |
|  [08]   | `IsModified`         | `bool IsModified`                                          | dirty state        |
|  [09]   | `Encoding`           | `Encoding Encoding`                                        | text encoding      |
|  [10]   | `CaretOffset`        | `int`                                                      | caret state        |
|  [11]   | `SelectionStart`     | `int`                                                      | selection state    |
|  [12]   | `SelectionLength`    | `int`                                                      | selection span     |
|  [13]   | `SelectedText`       | `string`                                                   | selected content   |
|  [14]   | `Load`               | `void Load(Stream)` / `Load(string fileName)`              | stream/file input  |
|  [15]   | `Save`               | `void Save(Stream)` / `Save(string)`                       | stream/file output |
|  [16]   | `AppendText`         | `void AppendText(string)`                                  | content edit       |
|  [17]   | `Clear`              | `void Clear()`                                             | content edit       |
|  [18]   | `Delete`             | `void Delete()`                                            | content edit       |
|  [19]   | `Select`             | `void Select(int start, int length)`                       | selection edit     |
|  [20]   | `BeginChange`        | `void`                                                     | undo grouping      |
|  [21]   | `EndChange`          | `void`                                                     | undo grouping      |
|  [22]   | `DeclareChangeBlock` | `IDisposable`                                              | undo grouping      |
|  [23]   | `Undo`               | `bool Undo()`                                              | undo navigation    |
|  [24]   | `Redo`               | `bool Redo()`                                              | undo navigation    |
|  [25]   | `Copy`               | clipboard command                                          | clipboard          |
|  [26]   | `Cut`                | clipboard command                                          | clipboard          |
|  [27]   | `Paste`              | clipboard command                                          | clipboard          |
|  [28]   | `SelectAll`          | selection command                                          | selection          |
|  [29]   | `ScrollTo`           | `void ScrollTo(int line, int column)`                      | navigation         |
|  [30]   | `ScrollToLine`       | `void ScrollToLine(int)`                                   | navigation         |
|  [31]   | `ScrollToEnd`        | `void ScrollToEnd()`                                       | navigation         |
|  [32]   | `DocumentChanged`    | `event`                                                    | lifecycle hook     |
|  [33]   | `TextChanged`        | `event`                                                    | lifecycle hook     |
|  [34]   | `OptionChanged`      | `event`                                                    | lifecycle hook     |
|  [35]   | `PointerHover`       | `event`                                                    | lifecycle hook     |
|  [36]   | `TextArea`           | `TextArea TextArea { get; }`                               | editing accessor   |

`TextArea` mounts `FoldingManager.Install(TextArea)`, `CompletionWindow`, and `OverloadInsightWindow`; `SearchPanel.Install` mounts on `TextEditor`.

Wrap multi-edit refactors in `using (editor.DeclareChangeBlock())` (or `BeginChange()`/`EndChange()`) so the `UndoStack` records one reversible step. `Load(Stream)` auto-detects encoding into `Encoding`; `IsModified` drives the dirty indicator.

[FOLDING_ENTRYPOINTS]: `FoldingManager` lifecycle and query
- rail: editor

| [INDEX] | [SURFACE]               | [SIGNATURE]                                                          | [ROLE]          |
| :-----: | :---------------------- | :------------------------------------------------------------------- | :-------------- |
|  [01]   | `Install`               | `static FoldingManager Install(TextArea)`                            | margin install  |
|  [02]   | `Uninstall`             | `static void Uninstall(FoldingManager)`                              | margin removal  |
|  [03]   | `UpdateFoldings`        | `void UpdateFoldings(IEnumerable<NewFolding>, int firstErrorOffset)` | folding refresh |
|  [04]   | `CreateFolding`         | `FoldingSection CreateFolding(int startOffset, int endOffset)`       | manual fold     |
|  [05]   | `GetFoldingsContaining` | `ReadOnlyCollection<FoldingSection>`                                 | fold query      |
|  [06]   | `GetFoldingsAt`         | `ReadOnlyCollection<FoldingSection>`                                 | fold query      |
|  [07]   | `GetNextFolding`        | `FoldingSection`                                                     | fold query      |
|  [08]   | `AllFoldings`           | `IEnumerable<FoldingSection>`                                        | fold set        |
|  [09]   | `Clear`                 | `void`                                                               | fold set        |
|  [10]   | `RemoveFolding`         | `void`                                                               | fold set        |

`UpdateFoldings` takes a `firstErrorOffset` (folds past a syntax error are dropped); pass `-1` when the whole range is valid. A custom folding strategy produces `NewFolding` rows and calls `UpdateFoldings` — `XmlFoldingStrategy` is the built-in example.

[COMPLETION_ENTRYPOINTS]: IntelliSense popup and item contract
- rail: editor

| [INDEX] | [SURFACE]                    | [SHAPE]                  | [ROLE]         |
| :-----: | :--------------------------- | :----------------------- | :------------- |
|  [01]   | `CompletionWindow`           | `.ctor(TextArea)`        | popup creation |
|  [02]   | `CompletionWindow.Show`      | `void`                   | popup open     |
|  [03]   | `CompletionData`             | `IList<ICompletionData>` | item source    |
|  [04]   | `ICompletionData.Complete`   | `void`                   | item insertion |
|  [05]   | `CloseAutomatically`         | `bool`                   | dismissal      |
|  [06]   | `CloseWhenCaretAtBeginning`  | `bool`                   | dismissal      |
|  [07]   | `OverloadInsightWindow`      | `.ctor(TextArea)`        | popup creation |
|  [08]   | `Provider`                   | `IOverloadProvider`      | overload list  |
|  [09]   | `OverloadInsightWindow.Show` | `void`                   | popup open     |

[COMPLETION_MEMBER_SHAPES]:
- `CompletionData`: `IList<ICompletionData> CompletionList.CompletionData { get; }`
- `Complete`: `void ICompletionData.Complete(TextArea, ISegment completionSegment, EventArgs)`
- `Provider`: `IOverloadProvider OverloadInsightWindow.Provider { get; set; }`
- `CompletionWindow`: construct over `TextArea`, populate `CompletionList.CompletionData`, then call `Show()`.
- `OverloadInsightWindow`: construct over `TextArea`, set `Provider`, then call `Show()` for multi-signature insight.

Implement `ICompletionData` per suggestion (an `Image`/`Content`/`Description`/`Priority` row whose `Complete` mutates the `TextArea` over the trigger `ISegment`), add rows to `CompletionList.CompletionData`, then `Show()`. The shell command rail (Compute-receipt-backed suggestions) feeds these rows; insertion runs through `Complete`, not direct document mutation.

[SEARCH_ENTRYPOINTS]: regex search/replace panel and strategy
- rail: editor

| [INDEX] | [SURFACE]      | [SIGNATURE]                                                                               | [ROLE]           |
| :-----: | :------------- | :---------------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `Install`      | `static SearchPanel Install(TextEditor)`                                                  | panel install    |
|  [02]   | `Open`         | `void`                                                                                    | overlay toggle   |
|  [03]   | `Close`        | `void`                                                                                    | overlay toggle   |
|  [04]   | `Reactivate`   | `void`                                                                                    | overlay toggle   |
|  [05]   | `FindNext`     | `void FindNext(int startOffset = -1)`                                                     | match navigation |
|  [06]   | `FindPrevious` | `void FindPrevious()`                                                                     | match navigation |
|  [07]   | `ReplaceNext`  | `void`                                                                                    | replace          |
|  [08]   | `ReplaceAll`   | `void`                                                                                    | replace          |
|  [09]   | `FindAll`      | `IEnumerable<ISearchResult> ISearchStrategy.FindAll(ITextSource, int offset, int length)` | bulk search      |

`SearchCommands.*` (`RoutedCommand` statics with default `KeyGesture`s — Ctrl+F/F3/Ctrl+H) are the keybinding entrypoints the shell command page binds; a programmatic count uses `ISearchStrategy.FindAll` directly.

[SNIPPET_AND_INDENT_ENTRYPOINTS]: template insertion and auto-indent
- rail: editor

| [INDEX] | [SURFACE]     | [SIGNATURE]                                                        | [ROLE]             |
| :-----: | :------------ | :----------------------------------------------------------------- | :----------------- |
|  [01]   | `Insert`      | `InsertionContext Snippet.Insert(TextArea)`                        | snippet expansion  |
|  [02]   | `InsertText`  | `void InsertionContext.InsertText(string)`                         | placeholder wiring |
|  [03]   | `Link`        | `void Link(ISegment, ISegment[])`                                  | placeholder wiring |
|  [04]   | `IndentLine`  | `void IIndentationStrategy.IndentLine(TextDocument, DocumentLine)` | auto-indent        |
|  [05]   | `IndentLines` | `void IndentLines(TextDocument, int begin, int end)`               | auto-indent        |
|  [06]   | `.ctor`       | `new CSharpIndentationStrategy(TextEditorOptions)`                 | C# indenter        |

Build a `Snippet` from `SnippetTextElement` + `SnippetReplaceableTextElement` (tab-stops) + `SnippetCaretElement` parts, then `Insert(textArea)` drives the interactive placeholder session. Assign `TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options)` for newline re-indent.

[TEXTMATE_ENTRYPOINTS]: grammar/theme session bound to one editor
- rail: editor

| [INDEX] | [SURFACE]          | [SHAPE]                | [ROLE]            |
| :-----: | :----------------- | :--------------------- | :---------------- |
|  [01]   | `InstallTextMate`  | returns `Installation` | session install   |
|  [02]   | `SetGrammar`       | `void`                 | scope selection   |
|  [03]   | `SetGrammarFile`   | `void`                 | file grammar      |
|  [04]   | `SetTheme`         | `void`                 | theme apply       |
|  [05]   | `TryGetThemeColor` | `bool`                 | theme color query |
|  [06]   | `RegistryOptions`  | `IRegistryOptions`     | session state     |
|  [07]   | `EditorModel`      | `TextEditorModel`      | tokenizer state   |
|  [08]   | `AppliedTheme`     | `event`                | theme event       |
|  [09]   | `Dispose`          | `void`                 | session teardown  |

[TEXTMATE_MEMBER_SHAPES]:
- `InstallTextMate`: `Installation InstallTextMate(this TextEditor, IRegistryOptions, bool initCurrentDocument = true, Action<Exception>? exceptionHandler = null)`
- `SetGrammar`: `void Installation.SetGrammar(string scopeName)`, such as `"source.cs"`
- `SetGrammarFile`: `void Installation.SetGrammarFile(string path)`
- `SetTheme`: `void Installation.SetTheme(IRawTheme theme)` from `TextMateSharp.Themes`
- `TryGetThemeColor`: `bool Installation.TryGetThemeColor(string colorKey, out string colorString)`
- `RegistryOptions`: `IRegistryOptions RegistryOptions { get; }`
- `EditorModel`: `TextEditorModel EditorModel { get; }`
- `AppliedTheme`: `event EventHandler<Installation> AppliedTheme`
- `Dispose`: `void Installation.Dispose()`

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
