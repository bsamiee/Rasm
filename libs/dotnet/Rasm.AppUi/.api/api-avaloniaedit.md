# [RASM_APPUI_API_AVALONIAEDIT]

`Avalonia.AvaloniaEdit` mints a code-editor control over a rope-backed `TextDocument`: a `TextArea`/`TextView` render stack with colorizing transformers and element generators, undo grouping, code folding, xshd highlighting, `CompletionWindow` IntelliSense, regex search, and snippet and indentation engines. Styled properties across `TextView`, `TextArea`, `TextEditor`, and the margins own every chrome brush, pen, and glyph the render stack paints outside a token run.

## [01]-[PUBLIC_TYPES]

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
|  [13]   | `StringTextSource`                         | class          | in-memory `ITextSource`         |

- `Caret`: `Offset` `Line` `Column` `Location`
- `TextDocument`: `BeginUpdate` `EndUpdate` `RunUpdate() -> IDisposable` `Replace` `Insert` `Remove` `GetText(ISegment)` `GetCharAt(int)` `GetLineByNumber(int)` `GetLineByOffset(int)` `LineCount` `TextLength` `UndoStack` — `RunUpdate` is the `using`-scoped form of the begin/end pair, so a multi-edit fold nests inside one document update
- `StringTextSource`: `StringTextSource(string)` `StringTextSource(string, ITextSourceVersion)` `Empty` `Text` `TextLength` `CreateSnapshot` `CreateReader` `GetText(ISegment)` (`: ITextSource`) — the one concrete plain-text source, so a headless scan runs the same `ISearchStrategy` over raw text that the pane runs over its document
- `DocumentLine`: `Offset` `Length` `LineNumber`
- `UndoStack`: `Undo` `Redo` `SizeLimit` `CanUndo` `CanRedo` `AcceptChanges` `IsOriginalFile` `MarkAsOriginalFile` `DiscardOriginalFileMarker` `Push(IUndoableOperation)` `PushOptional` `ClearRedoStack` `ClearAll` — grouping is `StartUndoGroup()` / `StartUndoGroup(object groupDescriptor)` / `StartContinuedUndoGroup(object? groupDescriptor = null)` / `EndUndoGroup()` with the last descriptor readable off `LastGroupDescriptor`, so a run of same-kind edits continues one group instead of minting a step per keystroke
- `TextViewPosition`: `Line` `Column` `VisualColumn`
- `TextSegment`: `StartOffset` `EndOffset` `Length` (settable; `: ISegment`)
- `TextSegmentCollection<T> where T : TextSegment`: `Add` `Remove` `Clear` `Count` `FirstSegment` `LastSegment` `GetNextSegment` `GetPreviousSegment` `FindFirstSegmentWithStartAfter(int)` `FindSegmentsContaining(int)` `FindOverlappingSegments(ISegment)`; `TextSegmentCollection(TextDocument)` and `UpdateOffsets(DocumentChangeEventArgs)` keep every held span live across edits, `Disconnect` releasing that binding

[FEATURE_TYPES]: folding, highlighting, completion, search, snippets, and indentation

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]           |
| :-----: | :------------------------------ | :------------- | :--------------------- |
|  [01]   | `FoldingManager`                | class          | folding owner          |
|  [02]   | `FoldingSection`                | class          | folded region          |
|  [03]   | `NewFolding`                    | class          | folding input          |
|  [04]   | `XmlFoldingStrategy`            | class          | XML folding            |
|  [05]   | `HighlightingManager`           | class          | definition registry    |
|  [06]   | `IHighlightingDefinition`       | interface      | definition contract    |
|  [07]   | `HighlightingLoader`            | static class   | xshd loader            |
|  [08]   | `DocumentHighlighter`           | class          | highlight engine       |
|  [09]   | `IHighlighter`                  | interface      | highlight contract     |
|  [10]   | `DocumentColorizingTransformer` | abstract class | line colorizer         |
|  [11]   | `IBackgroundRenderer`           | interface      | layer renderer         |
|  [12]   | `CompletionWindow`              | class          | completion popup       |
|  [13]   | `ICompletionData`               | interface      | completion item        |
|  [14]   | `OverloadInsightWindow`         | class          | overload popup         |
|  [15]   | `IOverloadProvider`             | interface      | overload contract      |
|  [16]   | `CompletionAcceptAction`        | enum           | commit gesture         |
|  [17]   | `SearchPanel`                   | class          | search overlay         |
|  [18]   | `ISearchStrategy`               | interface      | search contract        |
|  [19]   | `ISearchResult`                 | interface      | one hit (`: ISegment`) |
|  [20]   | `SearchStrategyFactory`         | static class   | the strategy mint      |
|  [21]   | `SearchMode`                    | enum           | pattern grammar        |
|  [22]   | `Snippet`                       | class          | snippet root           |
|  [23]   | `SnippetTextElement`            | class          | snippet literal        |
|  [24]   | `SnippetReplaceableTextElement` | class          | snippet tab-stop       |
|  [25]   | `SnippetBoundElement`           | class          | snippet bound field    |
|  [26]   | `SnippetCaretElement`           | class          | snippet caret target   |
|  [27]   | `SnippetSelectionElement`       | class          | snippet selection      |
|  [28]   | `IIndentationStrategy`          | interface      | indentation contract   |
|  [29]   | `CSharpIndentationStrategy`     | class          | C# indenter            |

- `FoldingSection`: `IsFolded` `Title`
- `NewFolding`: `StartOffset` `EndOffset` `Name` `DefaultClosed` `IsDefinition` (settable; `: ISegment`; `NewFolding(int start, int end)` throws on `start > end`)
- `HighlightingManager`: `Instance` `GetDefinition` `RegisterHighlighting`
- `IHighlightingDefinition`: `MainRuleSet` `GetNamedColor` `Properties`
- `HighlightingLoader`: `Load(XmlReader, IHighlightingDefinitionReferenceResolver)`
- `DocumentColorizingTransformer`: `ColorizeLine`
- `IBackgroundRenderer`: `Layer` `Draw(TextView, DrawingContext)`
- `ICompletionData`: `Image` `Text` `Content` `Description` `Priority` `Complete` (every member get-only; `Priority` is `double`, so rank is a float and an `int` tier scale cannot express a tie-break)
- `IOverloadProvider` (over `INotifyPropertyChanged`): settable `SelectedIndex` `int`, get-only `Count` `int`, `CurrentIndexText` `string`, `CurrentHeader` `object`, `CurrentContent` `object` — five members and no caret hook, so re-selection as arguments land is the consumer writing `SelectedIndex`
- `CompletionAcceptAction`: `PointerPressed` `PointerReleased` `DoubleTapped` — `TextEditorOptions.CompletionAcceptAction` picks which pointer gesture commits `CompletionList.SelectedItem`
- `ISearchStrategy`: `FindAll(ITextSource, int, int) : IEnumerable<ISearchResult>` `FindNext(ITextSource, int, int) : ISearchResult` (`: IEquatable<ISearchStrategy>`)
- `ISearchResult`: `ReplaceWith(string) : string` (`: ISegment`, so `StartOffset`/`Length`/`EndOffset` carry the hit span)
- `SearchMode`: `Normal` `RegEx` `Wildcard` — `Normal` regex-escapes the pattern, `Wildcard` lowers `?` to `.` and `*` to `.*` while escaping every other character, and the strategy is a `Regex` engine in every mode
- `SearchPanel`: settable `SearchPattern` `string`, `MatchCase` `bool`, `WholeWords` `bool`, `UseRegex` `bool`, `IsReplaceMode` `bool`, `ReplacePattern` `string`; get-only `TextEditor` `IsOpened` `IsClosed`; `SetSearchResultsBrush(IBrush)`, `RegisterCommands(ICollection<RoutedCommandBinding>)`, and the `SearchOptionsChanged` event — the knob set carries NO `SearchMode` column, so a consumer wanting `Wildcard` on the panel lowers the pattern itself and sets `UseRegex`; each knob write re-runs the panel's own search, so the pattern write lands last
- `SearchStrategyFactory.Create` is the ONLY mint for the `internal` regex strategy, so a `new RegexSearchStrategy(...)` at a call site does not compile

[RENDERING_TYPES]: `TextView` extension surface for custom visuals and left-margin chrome (`AvaloniaEdit.Rendering`, `.Editing`, `.Folding`)

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]        |
| :-----: | :--------------------------- | :------------- | :------------------ |
|  [01]   | `TextView`                   | class          | render host         |
|  [02]   | `VisualLineElementGenerator` | abstract class | inline elements     |
|  [03]   | `LinkElementGenerator`       | class          | hyperlinks          |
|  [04]   | `MailLinkElementGenerator`   | class          | mail links          |
|  [05]   | `IVisualLineTransformer`     | interface      | line transformation |
|  [06]   | `KnownLayer`                 | enum           | layer identity      |
|  [07]   | `LayerInsertionPosition`     | enum           | mount order         |
|  [08]   | `BackgroundGeometryBuilder`  | class          | overlay geometry    |
|  [09]   | `AbstractMargin`             | abstract class | left-margin base    |
|  [10]   | `LineNumberMargin`           | class          | line-number margin  |
|  [11]   | `FoldingMargin`              | class          | fold-marker margin  |
|  [12]   | `DottedLineMargin`           | static class   | margin separator    |

- `TextView`: `LineTransformers` `BackgroundRenderers` `ElementGenerators` `Layers` `VisualLines` `VisualLinesValid` `Redraw` `InvalidateLayer` `EnsureVisualLines` `Document` `Options`; a custom visual measures in two further metrics — `WideSpaceWidth -> double` advances one space in the current face, so a column stop is `IndentationSize * WideSpaceWidth` rather than a character count, and `ScrollOffset -> Vector` carries its own `ScrollOffsetChanged` event
- `VisualLine`: `FirstDocumentLine` `LastDocumentLine` `StartOffset` `VisualTop` `Height` `VisualLength` `Elements` `TextLines` `GetVisualPosition` `GetTextLineVisualYPosition` — `VisualTop` is document space, so a renderer subtracts `TextView.ScrollOffset.Y` to reach view space
- `VisualLineElementGenerator`: `GetFirstInterestedOffset` `ConstructElement`
- `IVisualLineTransformer`: `Transform(ITextRunConstructionContext, IList<VisualLineElement>)`
- `KnownLayer`: `Background` `Selection` `Text` `Caret` — declaration order IS paint order, and `IBackgroundRenderer.Layer` returns one case
- `LayerInsertionPosition`: `Below` `Replace` `Above`
- `BackgroundGeometryBuilder`: `CornerRadius` `AlignToWholePixels` `BorderThickness` `ExtendToFullWidthAtLineEnd` `AddSegment(TextView, ISegment)` `AddRectangle(TextView, Rect)` `AddRectangle(double left, double top, double right, double bottom)` `CloseFigure` `CreateGeometry` — the four-double overload is the hook a non-segment visual (an indent guide, a column band) builds its own rects through
- `AbstractMargin`: `TextView` `Document` (`: Control, ITextViewConnect`; a custom margin derives here and reads `Document` after `TextView` binds)
- `LineNumberMargin`: `MinWidthInDigits` `int` (default `2`)
- `FoldingMargin`: `FoldingManager` and four `AttachedProperty<IBrush>` marker brushes
- `DottedLineMargin`: `Create() -> Control` `IsDottedLineMargin(Control) -> bool` — the separator stroke seating between two `LeftMargins` entries
- NO indent-guide type ships: indent guides land as a consumer `IBackgroundRenderer` on `KnownLayer.Background` walking `TextView.VisualLines` through `BackgroundGeometryBuilder`

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

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                 |
| :-----: | :----------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `TextMate`                     | static class   | `InstallTextMate` extension host             |
|  [02]   | `TextMate.Installation`        | class          | grammar/theme session (`: IDisposable`)      |
|  [03]   | `TextEditorModel`              | class          | tokenizer model (`: AbstractLineList`)       |
|  [04]   | `TextMateColoringTransformer`  | class          | token colorizer (`: GenericLineTransformer`) |
|  [05]   | `GenericLineTransformer`       | abstract class | line-transform base (`SetTextStyle`)         |
|  [06]   | `ForegroundTextTransformation` | class          | one token's paint record                     |
|  [07]   | `DocumentSnapshot`             | class          | immutable line snapshot for the tokenizer    |

- `TextEditorModel`: `DocumentSnapshot` `InvalidateViewPortLines`
- `GenericLineTransformer`: `SetTextStyle(DocumentLine, int start, int length, IBrush fg, IBrush bg, FontStyle, FontWeight, bool underline)` — the whole span-paint contract a derived transformer calls
- `ForegroundTextTransformation`: `ColorMap` `ForegroundColor` `BackgroundColor` `FontStyle` `ExceptionHandler` `Transform(GenericLineTransformer, DocumentLine)` — one record per token, its two color ids resolved through `ColorMap` and its `FontStyle` mask lowered to italic, `FontWeight` 700, and underline
- `TextMateColoringTransformer` paints token spans alone: foreground, background, italic, bold, and underline drawn from the winning `Theme.Match` rule. `FontStyle.Strikethrough` never lowers, and no chrome pixel — control background, current line, selection, caret, line numbers, fold markers — is touched, so chrome alignment is entirely the consumer writing the styled-property set.

## [02]-[ENTRYPOINTS]

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
|  [37]   | `SearchPanel`          | property | search accessor     |
|  [38]   | `ExtentWidth`          | property | content extent      |
|  [39]   | `ExtentHeight`         | property | content extent      |
|  [40]   | `ViewportWidth`        | property | scroll window       |
|  [41]   | `ViewportHeight`       | property | scroll window       |
|  [42]   | `HorizontalOffset`     | property | scroll position     |
|  [43]   | `VerticalOffset`       | property | scroll position     |

- `DeclareChangeBlock()`: returns an `IDisposable`; a `using` scope records one reversible `UndoStack` step across multi-edit refactors.
- Rows [38]–[43] are the scroll geometry an overview strip reads: all six are `double`, forwarding to the templated `ScrollViewer`'s `Extent`, `Viewport`, and `Offset`, so a content-and-viewport rectangle comes off the editor rather than from a re-derivation over `TextView`. Each returns `0.0` while that part is null, so every read before the first layout pass answers a degenerate rectangle and a producer seeding once at composition publishes it.
- `Load(Stream)`: auto-detects encoding into `Encoding`; `IsModified` drives the dirty indicator.
- `TextArea`: mounts `FoldingManager.Install`, `CompletionWindow`, and `OverloadInsightWindow`; `SearchPanel.Install` mounts on `TextEditor`.
- `SearchPanel`: `CanSearch` reports whether the ctor installed this panel; every `SearchResultsBrush` write forwards here, so an assignment made before the control mounts is dropped.

[STYLING_ENTRYPOINTS]: chrome brushes, pens, and metrics the render stack paints outside a token run

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `TextView.CurrentLineBackground -> IBrush`                     | property | current-line fill       |
|  [02]   | `TextView.CurrentLineBorder -> IPen`                           | property | current-line outline    |
|  [03]   | `TextView.HighlightedLine -> int`                              | property | current-line target     |
|  [04]   | `TextView.SetDefaultHighlightLineColors()`                     | instance | current-line reset      |
|  [05]   | `TextView.ColumnRulerPen -> IPen`                              | property | ruler stroke            |
|  [06]   | `TextView.NonPrintableCharacterBrush -> IBrush`                | property | whitespace glyph paint  |
|  [07]   | `TextView.LinkTextForegroundBrush -> IBrush`                   | property | hyperlink paint         |
|  [08]   | `TextView.LinkTextBackgroundBrush -> IBrush`                   | property | hyperlink fill          |
|  [09]   | `TextView.LinkTextUnderline -> bool`                           | property | hyperlink decoration    |
|  [10]   | `TextArea.SelectionBrush -> IBrush`                            | property | selection fill          |
|  [11]   | `TextArea.SelectionForeground -> IBrush`                       | property | selected-text paint     |
|  [12]   | `TextArea.SelectionBorder -> Pen`                              | property | selection outline       |
|  [13]   | `TextArea.SelectionCornerRadius -> double`                     | property | selection corner        |
|  [14]   | `TextArea.CaretBrush -> IBrush`                                | property | caret paint             |
|  [15]   | `TextArea.Watermark -> string`                                 | property | empty-document hint     |
|  [16]   | `TextArea.LeftMargins -> ObservableCollection<Control>`        | property | margin stack            |
|  [17]   | `TextEditor.SearchResultsBrush -> IBrush`                      | property | match-marker fill       |
|  [18]   | `TextEditor.LineNumbersForeground -> IBrush`                   | property | line-number paint       |
|  [19]   | `TextEditor.LineNumbersMargin -> Thickness`                    | property | line-number padding     |
|  [20]   | `LineNumberMargin.MinWidthInDigits -> int`                     | property | line-number width floor |
|  [21]   | `FoldingMargin.FoldingMarkerBrush -> IBrush`                   | property | fold-marker stroke      |
|  [22]   | `FoldingMargin.FoldingMarkerBackgroundBrush -> IBrush`         | property | fold-marker fill        |
|  [23]   | `FoldingMargin.SelectedFoldingMarkerBrush -> IBrush`           | property | hovered marker stroke   |
|  [24]   | `FoldingMargin.SelectedFoldingMarkerBackgroundBrush -> IBrush` | property | hovered marker fill     |
|  [25]   | `TextArea.IndentationStrategy -> IIndentationStrategy`         | property | newline re-indent owner |
|  [26]   | `TextArea.TextEntering` / `TextEntered`                        | event    | pre/post input hooks    |
|  [27]   | `TextArea.SelectionChanged` / `TextCopied` / `TextPasted`      | event    | selection and clipboard |

- `TextView.CurrentLineBackground`/`CurrentLineBorder`: both default `null` and forward every write to the internal current-line renderer, which `TextEditorOptions.HighlightCurrentLine` gates. Its own light-tuned seed — `#1614DCE0` fill over a 1px `#3400FF6E` border — never reaches the styled properties, and `SetDefaultHighlightLineColors()` stamps that seed straight onto the renderer behind them; a dark pass assigns the two properties and never calls it.
- `TextView.ColumnRulerPen`: `ColumnRulerPenProperty` registers under the Avalonia property name `"ColumnRulerBrush"` while its CLR accessor reads `ColumnRulerPen`, so the two spellings name one value; default is a frozen 1px `#5A808080` pen, and `TextEditorOptions.ShowColumnRulers` gates the draw.
- `TextArea.SelectionBorder`: declared `Pen`, not `IPen`, so an `ImmutablePen` does not assign; `SelectionCornerRadius` defaults `3.0` and the three selection brushes default `null`.
- `TextArea.CaretBrush`: `DirectProperty` proxying `Caret.CaretBrush`, so the caret reads it live; `LeftMargins` is a getter-only `DirectProperty` and margins mutate the collection in place.
- `TextArea.TextEntering` fires before the input reaches the document and `TextEntered` after, both carrying `TextInputEventArgs`, so an auto-close pair inserts on the second and a suppression cancels on the first; `IndentationStrategy` is a plain `StyledProperty`, assigned once per pane.
- `TextEditor.SearchResultsBrush`: defaults `#515C6A`; `LineNumbersForeground` defaults `Brushes.Gray` and `LineNumbersMargin` `Thickness(2, 0, 2, 0)`.
- `FoldingMargin` declares its four marker brushes as `AttachedProperty<IBrush>`, so one setter on an ancestor styles every fold marker beneath it.

[OPTION_ENTRYPOINTS]: `TextEditorOptions` policy knobs the render stack, element generators, and input handlers read

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `AcceptsTab -> bool`                               | property | tab key indents, default `true`              |
|  [02]   | `IndentationSize -> int`                           | property | indent width in columns, default `4`         |
|  [03]   | `ConvertTabsToSpaces -> bool`                      | property | indent unit is spaces, default `false`       |
|  [04]   | `IndentationString -> string`                      | property | one indent unit, get-only                    |
|  [05]   | `GetIndentationString(int) -> string`              | instance | indent string reaching a column              |
|  [06]   | `ShowSpaces -> bool`                               | property | space glyphs, default `false`                |
|  [07]   | `ShowSpacesGlyph -> string`                        | property | space mark, default `"·"`                    |
|  [08]   | `ShowTabs -> bool`                                 | property | tab glyphs, default `false`                  |
|  [09]   | `ShowTabsGlyph -> string`                          | property | tab mark, default `"→"`                      |
|  [10]   | `ShowEndOfLine -> bool`                            | property | line-end glyphs, default `false`             |
|  [11]   | `EndOfLineCRLFGlyph -> string`                     | property | CRLF mark, default `"¶"`                     |
|  [12]   | `EndOfLineCRGlyph -> string`                       | property | CR mark, default `"\r"`                      |
|  [13]   | `EndOfLineLFGlyph -> string`                       | property | LF mark, default `"\n"`                      |
|  [14]   | `ShowBoxForControlCharacters -> bool`              | property | control-char box, default `true`             |
|  [15]   | `EnableHyperlinks -> bool`                         | property | URL link elements, default `true`            |
|  [16]   | `EnableEmailHyperlinks -> bool`                    | property | mail link elements, default `true`           |
|  [17]   | `RequireControlModifierForHyperlinkClick -> bool`  | property | link activation gate, default `true`         |
|  [18]   | `CutCopyWholeLine -> bool`                         | property | empty-selection line copy, default `true`    |
|  [19]   | `AllowScrollBelowDocument -> bool`                 | property | scroll past the last line, default `true`    |
|  [20]   | `WordWrapIndentation -> double`                    | property | wrapped-line indent, default `0`             |
|  [21]   | `InheritWordWrapIndentation -> bool`               | property | wrap indent follows the line, default `true` |
|  [22]   | `EnableRectangularSelection -> bool`               | property | box selection, default `true`                |
|  [23]   | `EnableTextDragDrop -> bool`                       | property | drag-move selection, default `true`          |
|  [24]   | `EnableVirtualSpace -> bool`                       | property | caret past line end, default `false`         |
|  [25]   | `EnableImeSupport -> bool`                         | property | IME composition, default `true`              |
|  [26]   | `ShowColumnRulers -> bool`                         | property | ruler draw, default `false`                  |
|  [27]   | `ColumnRulerPositions -> IEnumerable<int>`         | property | ruler columns, default `[80]`                |
|  [28]   | `HighlightCurrentLine -> bool`                     | property | current-line draw, default `false`           |
|  [29]   | `HideCursorWhileTyping -> bool`                    | property | pointer hides on input, default `true`       |
|  [30]   | `AllowToggleOverstrikeMode -> bool`                | property | Insert toggles overstrike, default `false`   |
|  [31]   | `ExtendSelectionOnMouseUp -> bool`                 | property | drag extends past release, default `true`    |
|  [32]   | `CompletionAcceptAction -> CompletionAcceptAction` | property | commit gesture, default `PointerPressed`     |
|  [33]   | `LineHeightFactor -> double`                       | property | line-height multiplier, default `1.16`       |
|  [34]   | `PropertyChanged`                                  | event    | knob-change feed                             |
|  [35]   | `TextEditorOptions(TextEditorOptions)`             | ctor     | clone every knob                             |

- Every knob write raises `PropertyChanged`, and `TextView` answers it by re-seeding the column ruler from `ShowColumnRulers`/`ColumnRulerPositions`, re-fetching the built-in element generators, and running a full `Redraw()`; `LineHeightFactor` extends that pass by invalidating the cached default line height and baseline.
- Each glyph knob is a free string, so a font lacking `·`, `→`, or `¶` swaps the mark rather than losing the whitespace view; `EndOfLineCRGlyph`/`EndOfLineLFGlyph` default to the two-character literals `\r` and `\n`, never the control characters.
- `TextEditorOptions(TextEditorOptions)` copy-constructs the whole knob set, so a per-pane variant forks a base instance instead of sharing one across editors.

[LAYER_ENTRYPOINTS]: custom-visual mounting on the `TextView` render stack

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :------------------ |
|  [01]   | `TextView.InsertLayer(Control, KnownLayer, LayerInsertionPosition)`                   | instance | layer mount         |
|  [02]   | `TextView.Layers -> LayerCollection`                                                  | property | mounted layers      |
|  [03]   | `TextView.InvalidateLayer(KnownLayer)`                                                | instance | layer repaint       |
|  [04]   | `TextView.BackgroundRenderers -> IList<IBackgroundRenderer>`                          | property | background visuals  |
|  [05]   | `TextView.LineTransformers -> IList<IVisualLineTransformer>`                          | property | per-line paint      |
|  [06]   | `TextView.ElementGenerators -> IList<VisualLineElementGenerator>`                     | property | inline elements     |
|  [07]   | `BackgroundGeometryBuilder.AddSegment(TextView, ISegment)`                            | instance | span geometry       |
|  [08]   | `BackgroundGeometryBuilder.CreateGeometry() -> Geometry`                              | instance | geometry mint       |
|  [09]   | `BackgroundGeometryBuilder.GetRectsForSegment(TextView, ISegment, bool)`              | static   | span rects          |
|  [10]   | `BackgroundGeometryBuilder.GetRectsFromVisualSegment(TextView, VisualLine, int, int)` | static   | visual-column rects |

- `InsertLayer` is the ordered mount: it stamps an internal layer position and splices by `KnownLayer` then `LayerInsertionPosition`, throwing `ArgumentOutOfRangeException` on an undefined enum value and `InvalidOperationException` for anything but `Above` against `KnownLayer.Background`. Adding straight to `Layers` skips that ordering, so a visual seats at the collection tail.
- AvaloniaEdit keeps its current-line, column-ruler, and search-result renderers `internal`, reachable only through their styled properties, so a consumer's own background visual is an `IBackgroundRenderer` added to `BackgroundRenderers` and repainted through `InvalidateLayer` on its own `Layer`.

[FOLDING_ENTRYPOINTS]: `FoldingManager` lifecycle and query

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]    |
| :-----: | :--------------------------------------------- | :------- | :-------------- |
|  [01]   | `Install(TextArea) -> FoldingManager`          | static   | margin install  |
|  [02]   | `Uninstall(FoldingManager)`                    | static   | margin removal  |
|  [03]   | `UpdateFoldings(IEnumerable<NewFolding>, int)` | instance | folding refresh |
|  [04]   | `CreateFolding(int, int)`                      | instance | manual fold     |
|  [05]   | `GetFoldingsContaining(int)`                   | instance | fold query      |
|  [06]   | `GetFoldingsAt(int)`                           | instance | fold query      |
|  [07]   | `GetNextFolding(int)`                          | instance | fold query      |
|  [08]   | `AllFoldings`                                  | property | fold set        |
|  [09]   | `Clear()`                                      | instance | fold set        |
|  [10]   | `RemoveFolding(FoldingSection)`                | instance | fold set        |

- `UpdateFoldings` is the whole-set resync, not an append: the manager walks `AllFoldings` in start order, reuses the live `FoldingSection` whose `StartOffset` the pass repeats — assigning `Length` then `Title`, so user `IsFolded` state survives — removes the sections the pass skipped, and `CreateFolding`s only new starts. It refuses a sequence out of ascending `StartOffset` order with `ArgumentException`, skips a zero-length row silently, and binds `DefaultClosed` only on the manager's first update, so a later pass cannot force a region closed. Its `int firstErrorOffset` drops folds past a syntax error and normalizes a negative to `int.MaxValue`; pass `-1` when the whole range is valid. `XmlFoldingStrategy` is the built-in producer; a custom strategy emits `NewFolding` rows and calls it.
- `CreateFolding` is the manual-fold entry with no dedup and no removal, so a per-region re-parse loop through it doubles the margin and orphans fold state where the resync converges; it throws on `startOffset >= endOffset` and on `endOffset > Document.TextLength`.

[COMPLETION_ENTRYPOINTS]: IntelliSense popup and item contract

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]     |
| :-----: | :------------------------------------------------ | :------- | :--------------- |
|  [01]   | `CompletionWindow(TextArea)`                      | ctor     | popup creation   |
|  [02]   | `CompletionWindowBase.StartOffset / EndOffset`    | property | insertion span   |
|  [03]   | `CompletionWindowBase.ExpectInsertionBeforeStart` | property | span anchoring   |
|  [04]   | `CompletionWindowBase.Show() / Hide()`            | instance | popup lifetime   |
|  [05]   | `CompletionWindow.CloseAutomatically`             | property | dismissal        |
|  [06]   | `CompletionWindow.CloseWhenCaretAtBeginning`      | property | dismissal        |
|  [07]   | `CompletionWindow.CompletionList`                 | property | list accessor    |
|  [08]   | `CompletionList.CompletionData`                   | property | item source      |
|  [09]   | `CompletionList.SelectedItem`                     | property | selection state  |
|  [10]   | `CompletionList.IsFiltering`                      | property | prefix narrowing |
|  [11]   | `ICompletionData.Complete(...)`                   | instance | item insertion   |
|  [12]   | `OverloadInsightWindow(TextArea)`                 | ctor     | popup creation   |
|  [13]   | `OverloadInsightWindow.Provider`                  | property | overload list    |
|  [14]   | `OverloadInsightWindow.Show()`                    | instance | popup open       |

- `StartOffset` IS the insertion contract: the window synthesizes the segment itself, calling `CompletionList.SelectedItem?.Complete(TextArea, new AnchorSegment(TextArea.Document, StartOffset, EndOffset - StartOffset), e)` on an insertion request, after `Hide()`. `CompletionWindowBase`'s ctor seeds both offsets from `TextArea.Caret.Offset`, so a popup mounted after the trigger prefix is typed replaces nothing unless the mount assigns `StartOffset` back to the trigger start; document edits then move `StartOffset` `BeforeInsertion` and `EndOffset` `AfterInsertion`, and `ExpectInsertionBeforeStart` flips one pending caret-position insert to `AfterInsertion` so a committed prefix keystroke widens the span instead of preceding it.
- `IsFiltering` defaults `true` and is the built-in prefix narrowing over the mounted rows — a camel-case and substring match quality fold that re-selects the best row per keystroke — so per-keystroke re-population of `CompletionData` is the deleted form; `false` degrades it to starts-with selection.
- `ICompletionData.Complete(TextArea, ISegment, EventArgs)`: mutates the `TextArea` over the synthesized trigger `ISegment` — insertion runs here, never direct document mutation. Implement one per suggestion, add rows to `CompletionList.CompletionData` (a mutable `IList<ICompletionData>`), then `Show()`; the shell command deck feeds the rows.
- `OverloadInsightWindow`: construct over `TextArea`, set `Provider`, then `Show()` for multi-signature insight. Its own `OnKeyDown` handles Up and Down through an internal `ChangeIndex(±1)` and only while `Provider != null && Provider.Count > 1`; every other index move is the consumer assigning `Provider.SelectedIndex`, because no member tracks the caret.

[SEARCH_ENTRYPOINTS]: regex search/replace panel and strategy

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `SearchPanel.Install(TextEditor) -> SearchPanel`                                  | static   | panel install         |
|  [02]   | `SearchPanel.Uninstall()`                                                         | instance | panel teardown        |
|  [03]   | `Open()` / `Close()` / `Reactivate()`                                             | instance | overlay toggle        |
|  [04]   | `FindNext(int startOffset = -1)` / `FindPrevious()`                               | instance | match navigation      |
|  [05]   | `ReplaceNext()` / `ReplaceAll()`                                                  | instance | replace               |
|  [06]   | `SearchPattern` / `MatchCase` / `WholeWords` / `UseRegex`                         | property | styled search knob    |
|  [07]   | `IsReplaceMode` / `ReplacePattern` / `IsOpened` / `IsClosed` / `TextEditor`       | property | panel state           |
|  [08]   | `SetSearchResultsBrush(IBrush)` / `RegisterCommands(ICollection<…>)`              | instance | marker and keys       |
|  [09]   | `SearchOptionsChanged`                                                            | event    | knob-change feed      |
|  [10]   | `SearchStrategyFactory.Create(string, bool, bool, SearchMode) -> ISearchStrategy` | static   | strategy mint         |
|  [11]   | `ISearchStrategy.FindAll(ITextSource, int, int)`                                  | instance | bulk search           |
|  [12]   | `ISearchStrategy.FindNext(ITextSource, int, int)`                                 | instance | first hit at or after |

- `SearchCommands.*`: `RoutedCommand` statics carry default `KeyGesture`s (`Ctrl+F`/`F3`/`Ctrl+H`) the shell command page binds; a programmatic count drives `ISearchStrategy.FindAll` directly.
- `SearchStrategyFactory.Create(searchPattern, ignoreCase, matchWholeWords, mode)` is the strategy hook a programmatic search takes: the panel builds the identical value from its own knobs (`Create(SearchPattern, !MatchCase, WholeWords, UseRegex ? SearchMode.RegEx : SearchMode.Normal)`), so panel-driven and headless search share one engine and an invalid pattern throws `SearchPatternException` at the mint rather than at the scan.

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
- `TryGetThemeColor(colorKey, out colorString)`: returns the hex string the applied theme's own `colors` block declared under that key, refreshed on every `SetTheme` and cleared on `Dispose` — a call after disposal throws `ObjectDisposedException`. Key coverage is theme-authored, so a `false` return is the normal path and the consumer's fallback owns that pixel.
- `AppliedTheme`: `EventHandler<Installation>` raised after each `SetTheme` swaps the color map; the handler re-reads every chrome key and rewrites the styled-property set.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every editor surface is one `TextEditor` over a `TextDocument`; feature owners (`FoldingManager`, `SearchPanel`, `CompletionWindow`, `CSharpIndentationStrategy`) mount onto its `TextArea`/`TextView`, and each multi-edit folds through one `DeclareChangeBlock` undo step.
- Token color arrives on a `LineTransformers` entry; every other pixel — control background, current line, selection, caret, line numbers, fold markers, column rulers, whitespace glyphs — resolves from a styled property on `TextView`, `TextArea`, `TextEditor`, or a margin, so a theme swap rewrites that property set and leaves the transformer untouched.

[STACKING]:
- `TextMateSharp`(`.api/api-textmatesharp.md`): `InstallTextMate` consumes an `IRegistryOptions`, `SetGrammar` a scope string, and `SetTheme` an `IRawTheme` that `LoadTheme(ThemeName)` returns; the provider owns the `IGrammar`/`ThemeName`/theme corpus and this adapter only forwards those handles.
- `TextMateSharp`(`.api/api-textmatesharp.md`): `Installation.TryGetThemeColor(key, out hex)` reads exactly the `Theme.GetGuiColorDictionary()` map the applied theme carried, so a hex string parses to a brush and lands on the styled-property set — `"editor.background"` onto the control, `"editor.selectionBackground"` onto `TextArea.SelectionBrush`, `"editor.lineHighlightBackground"` onto `TextView.CurrentLineBackground`. Key coverage is theme-authored, so every read branches on the `false` return.
- AppUi code pane: `TextEditor` + `TextDocument` wrap in a `ReactiveUserControl`; `FoldingManager.Install`/`SearchPanel.Install`/`CompletionWindow`/`CSharpIndentationStrategy` own structure, find, IntelliSense, and indent; the single `Installation` rides `TextView.LineTransformers`, its `AppliedTheme` handler rewrites the chrome property set, and disposal runs through `WhenActivated`'s `CompositeDisposable` when the editor unloads.

[LOCAL_ADMISSION]:
- Code-view intent admits as a `TextEditor` over `TextDocument` state with feature owners installed on its `TextArea`; a `TextBox`-derived custom editor is the deleted form.
- Unshipped visuals — indent guides, a diff gutter, an inline squiggle — admit as an `IBackgroundRenderer`, `IVisualLineTransformer`, or `VisualLineElementGenerator` on the owning `TextView`, and a gutter as an `AbstractMargin` in `TextArea.LeftMargins`; a control floated over the editor is the deleted form.
