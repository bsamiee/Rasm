# [APPUI_CODEPANE_EDITING]

Grammar-scoped code editing over AvaloniaEdit and TextMate: `EditorInk` is the one chrome correspondence feeding both the live styled-property bind set and the projected gui-color block, `EditorOptionsRow` the capability-set enablement policy, `RasmRegistry` the one product grammar locator, `LanguagePlan` the compiled behavior projection its `[Mapper]` lands, and `CodePane` the boundary capsule mounting grammar, chrome, folding, behavior, and search as one custody chain — with the table-driven completion and overload projections beside it.

## [01]-[INDEX]

- [02]-[EDITOR_CHROME]: The ink correspondence table, the gui-color decorator, the affordance and whitespace capability sets, and the owned indent-guide renderer.
- [03]-[GRAMMAR_BEHAVIOR]: The product registry over the bundled corpus, the grammar-scope rows, the compiled behavior plan with its generated mapper, and the indentation strategy.
- [04]-[CODE_PANE]: The pane capsule, the session custody chain, the whole-set fold resync, and the overview lane source.
- [05]-[ASSIST]: The completion-family axis, the weight policy, the one `ICompletionData` projection, and the overload provider over the same rows.

## [02]-[EDITOR_CHROME]

- Owner: `EditorInk` `[SmartEnum<string>]` the chrome correspondence table; `TokenRawTheme` the projected gui-color decorator; `EditorAffordance` and `WhitespaceMark` the enablement capability vocabularies; `EditorOptionsRow` the enablement policy; `IndentGuides` the owned background renderer.
- Law: chrome resolves from the ONE token resolve, never from the grammar theme — the bundled themes author their gui-color block partially and inconsistently (`DarkPlus` declares `editor.background` with no selection or line-number key; `Monokai` and `OneDark` spell one gutter pixel under two names), so the direction inverts: `EditorInk` rows project the resolve INTO the gui-color vocabulary through `TokenRawTheme`, and every headless tokenized surface reads the same pixels a pane paints. A chrome key read back through `TryGetThemeColor` on a mounted editor is the deleted form.
- Law: the projected hex is written `#RRGGBBAA` because Avalonia's own `Color.ToString` writes `#AARRGGBB` — eight hex digits behind a hash either way, so the default round-trip is a silently channel-rotated colour no parse can catch.
- Law: enablement is the other half of chrome — `HighlightCurrentLine` and `ShowColumnRulers` default false, so the current-line and ruler inks paint nothing until the options row turns them on; each `EditorInk` row therefore carries its `Lit` gate against the options row, and `Bind` seats only the lit rows. `TextView.CurrentLineBackground`/`CurrentLineBorder` forward to an internal renderer whose light-tuned seed never reaches the styled properties, so a dark pass binds the two properties and never calls `SetDefaultHighlightLineColors()`.
- Law: three chrome members take a PEN where the emission carries brushes and metrics, so each folds its ink and stroke width from two dynamic reads: `TextArea.SelectionBorder` is the concrete `Pen` (an `ImmutablePen` does not assign), `TextView.ColumnRulerPenProperty` registers under the Avalonia name `"ColumnRulerBrush"` while its CLR accessor reads `ColumnRulerPen`, and `TextEditor.SearchResultsBrush` forwards to the installed `SearchPanel` — a write before the panel mounts is DROPPED, which is why `Open` orders `SearchPanel.Install` ahead of the ink set.
- Packages: Avalonia.AvaloniaEdit, AvaloniaEdit.TextMate, TextMateSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one chrome pixel is one `EditorInk` row; one enablement axis is one `EditorAffordance` or `WhitespaceMark` row read at both the options fold and the row gate.
- Boundary: `TokenRawTheme` takes a VALUE theme because the tokenizer compiles its colour trie once — a re-materialization, not a dynamic consumer — and rides `Theme/tokens#CONTROL_THEMES` `Rematerialize.GrammarTheme`, whose rebuild re-emits the block and calls `SetTheme` on every mounted installation; inside a pane the rows bind styled properties through `ThemeGate.Bind`, so a variant flip re-tints with nothing rebuilt. `WhitespaceMark` restores a capability the retired single bool erased: spaces, tabs, and line-ends are three package knobs one flag drove as one. Indent guides ship in no AvaloniaEdit type — the row's own `Attach` mounts the owned renderer on `TextView.BackgroundRenderers` (`InsertLayer` throws for anything but `Above` against `KnownLayer.Background`, and an `Above` layer paints over the text).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EditorAffordance : ICapability<EditorAffordance> {
    public static readonly EditorAffordance CurrentLine = new(key: "current-line");
    public static readonly EditorAffordance Rulers = new(key: "rulers");
    public static readonly EditorAffordance Hyperlinks = new(key: "hyperlinks");
    public static readonly EditorAffordance IndentGuides = new(key: "indent-guides");
    public static readonly EditorAffordance VirtualSpace = new(key: "virtual-space");
    public static readonly EditorAffordance SpacesForTabs = new(key: "spaces-for-tabs");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WhitespaceMark : ICapability<WhitespaceMark> {
    public static readonly WhitespaceMark Spaces = new(key: "spaces");
    public static readonly WhitespaceMark Tabs = new(key: "tabs");
    public static readonly WhitespaceMark LineEnds = new(key: "line-ends");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EditorInk {
    public static readonly EditorInk Surface = new("editor.background", PaintRole.Well, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor, TemplatedControl.BackgroundProperty),
        lit: static _ => true);
    public static readonly EditorInk Ink = new("editor.foreground", PaintRole.Text, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor, TemplatedControl.ForegroundProperty),
        lit: static _ => true);
    public static readonly EditorInk CurrentLine = new("editor.lineHighlightBackground", PaintRole.Surface, rung: 1,
        attach: static (editor, key) => ThemeGate.Bind(editor.TextArea.TextView, TextView.CurrentLineBackgroundProperty),
        lit: static options => options.Affordances.Admits(EditorAffordance.CurrentLine));
    public static readonly EditorInk CurrentLineEdge = new("editor.lineHighlightBorder", PaintRole.Border, rung: 0,
        attach: static (editor, key) => Stroked(editor.TextArea.TextView, TextView.CurrentLineBorderProperty, static pen => (IPen)pen),
        lit: static options => options.Affordances.Admits(EditorAffordance.CurrentLine));
    public static readonly EditorInk Selection = new("editor.selectionBackground", PaintRole.Selection, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor.TextArea, TextArea.SelectionBrushProperty),
        lit: static _ => true);
    public static readonly EditorInk SelectionInk = new("editor.selectionForeground", PaintRole.SelectionText, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor.TextArea, TextArea.SelectionForegroundProperty),
        lit: static _ => true);
    public static readonly EditorInk SelectionEdge = new("editor.selectionHighlightBorder", PaintRole.Focus, rung: 0,
        attach: static (editor, key) => Stroked(editor.TextArea, TextArea.SelectionBorderProperty, static pen => pen),
        lit: static _ => true);
    public static readonly EditorInk Caret = new("editorCursor.foreground", PaintRole.Text, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor.TextArea, TextArea.CaretBrushProperty),
        lit: static _ => true);
    public static readonly EditorInk LineNumbers = new("editorLineNumber.foreground", PaintRole.TextFaint, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor, TextEditor.LineNumbersForegroundProperty),
        lit: static _ => true);
    public static readonly EditorInk Ruler = new("editorRuler.foreground", PaintRole.Separator, rung: 0,
        attach: static (editor, key) => Stroked(editor.TextArea.TextView, TextView.ColumnRulerPenProperty, static pen => (IPen)pen),
        lit: static options => options.Affordances.Admits(EditorAffordance.Rulers));
    public static readonly EditorInk Whitespace = new("editorWhitespace.foreground", PaintRole.TextFaint, rung: 1,
        attach: static (editor, key) => ThemeGate.Bind(editor.TextArea.TextView, TextView.NonPrintableCharacterBrushProperty),
        lit: static options => toSeq(WhitespaceMark.Items).Exists(options.Marks.Admits));
    public static readonly EditorInk Link = new("textLink.foreground", PaintRole.Link, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor.TextArea.TextView, TextView.LinkTextForegroundBrushProperty),
        lit: static options => options.Affordances.Admits(EditorAffordance.Hyperlinks));
    public static readonly EditorInk Match = new("editor.findMatchHighlightBackground", PaintRole.Highlight, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor, TextEditor.SearchResultsBrushProperty),
        lit: static _ => true);
    public static readonly EditorInk FoldMarker = new("editorGutter.foldingControlForeground", PaintRole.TextMuted, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor, FoldingMargin.FoldingMarkerBrushProperty),
        lit: static _ => true);
    public static readonly EditorInk FoldMarkerFill = new("editorGutter.background", PaintRole.Panel, rung: 0,
        attach: static (editor, key) => ThemeGate.Bind(editor, FoldingMargin.FoldingMarkerBackgroundBrushProperty),
        lit: static _ => true);
    public static readonly EditorInk IndentGuide = new("editorIndentGuide.background", PaintRole.Separator, rung: 0,
        attach: static (editor, _) => IndentGuides.Mount(editor.TextArea.TextView),
        lit: static options => options.Affordances.Admits(EditorAffordance.IndentGuides));

    public PaintRole Role { get; }
    public int Rung { get; }

    public TokenKey Token => Role.At(Rung);

    [UseDelegateFromConstructor]
    public partial IDisposable Attach(TextEditor editor, TokenKey token);

    [UseDelegateFromConstructor]
    public partial bool Lit(EditorOptionsRow options);

    public static IDisposable Bind(TextEditor editor, EditorOptionsRow options) =>
        new CompositeDisposable(toSeq(Items).Filter(row => row.Lit(options)).Map(row => row.Attach(editor, row.Token)));

    public static ICollection<KeyValuePair<string, object>> Emit(ResolvedTheme resolved) =>
        toSeq(Items)
            .Choose(row => resolved.Paint(row.Role, row.Rung)
                .Map(colour => new KeyValuePair<string, object>(row.Key, (object)Hex(colour))))
            .ToList();

    static string Hex(Color colour) => $"#{colour.R:X2}{colour.G:X2}{colour.B:X2}{colour.A:X2}";

    static IDisposable Stroked<T>(Control target, StyledProperty<T> property, TokenKey ink, Func<Pen, T> lift) =>
        target.Bind(property, target.GetResourceObservable(ink.Value)
            .CombineLatest(target.GetResourceObservable(MetricFamily.Stroke.At(0).Value),
                (brush, width) => lift(new Pen(brush as IBrush, width is double stroke ? stroke : 1d))));
}

public sealed class TokenRawTheme(IRawTheme inner, ResolvedTheme resolved) : IRawTheme {
    public string GetName() => inner.GetName();

    public string GetInclude() => inner.GetInclude();

    public ICollection<IRawThemeSetting> GetSettings() => inner.GetSettings();

    public ICollection<IRawThemeSetting> GetTokenColors() => inner.GetTokenColors();

    public ICollection<KeyValuePair<string, object>> GetGuiColors() => EditorInk.Emit(resolved);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record EditorOptionsRow(
    CapabilitySet<EditorAffordance> Affordances,
    Seq<int> RulerColumns,
    CapabilitySet<WhitespaceMark> Marks,
    int IndentSize,
    bool SpacesForTabs,
    double LineHeight) {
    public static readonly EditorOptionsRow Default = new(
        Affordances: CapabilitySet<EditorAffordance>.Of(
            EditorAffordance.CurrentLine, EditorAffordance.Rulers, EditorAffordance.Hyperlinks, EditorAffordance.IndentGuides),
        RulerColumns: Seq(100),
        Marks: CapabilitySet<WhitespaceMark>.Of(),
        IndentSize: 4,
        SpacesForTabs: true,
        LineHeight: 1.4d);

    public TextEditorOptions Apply(TextEditorOptions basis) => new(basis) {
        HighlightCurrentLine = Affordances.Admits(EditorAffordance.CurrentLine),
        ShowColumnRulers = Affordances.Admits(EditorAffordance.Rulers),
        ColumnRulerPositions = RulerColumns,
        ShowSpaces = Marks.Admits(WhitespaceMark.Spaces),
        ShowTabs = Marks.Admits(WhitespaceMark.Tabs),
        ShowEndOfLine = Marks.Admits(WhitespaceMark.LineEnds),
        EnableHyperlinks = Affordances.Admits(EditorAffordance.Hyperlinks),
        EnableEmailHyperlinks = Affordances.Admits(EditorAffordance.Hyperlinks),
        EnableVirtualSpace = Affordances.Admits(EditorAffordance.VirtualSpace),
        IndentationSize = IndentSize,
        ConvertTabsToSpaces = SpacesForTabs,
        LineHeightFactor = LineHeight,
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class IndentGuides : IBackgroundRenderer, IDisposable {
    readonly IDisposable subscription;

    IBrush? ink;

    public IndentGuides(TextView view) =>
        subscription = view.GetResourceObservable(EditorInk.IndentGuide.Token.Value).Subscribe(value => {
            ink = value as IBrush;
            view.InvalidateLayer(KnownLayer.Background);
        });

    public KnownLayer Layer => KnownLayer.Background;

    public static IDisposable Mount(TextView view) {
        IndentGuides guides = new(view);
        view.BackgroundRenderers.Add(guides);
        return Disposable.Create(() => {
            ignore(view.BackgroundRenderers.Remove(guides));
            guides.Dispose();
        });
    }

    public void Draw(TextView textView, DrawingContext drawingContext) {
        double step = textView.Options.IndentationSize * textView.WideSpaceWidth;
        if (ink is not IBrush brush || step <= 0d) { return; }
        BackgroundGeometryBuilder builder = new() { AlignToWholePixels = true };
        toSeq(textView.VisualLines).Iter(line => Stops(textView, line, step)
            .Iter(x => builder.AddRectangle(x, line.VisualTop - textView.ScrollOffset.Y, x + 1d,
                line.VisualTop - textView.ScrollOffset.Y + line.Height)));
        Optional(builder.CreateGeometry()).Iter(geometry => drawingContext.DrawGeometry(brush, null, geometry));
    }

    static Seq<double> Stops(TextView textView, VisualLine line, double step) =>
        toSeq(Enumerable.Range(1, (int)(Leading(textView, line.FirstDocumentLine) / step)))
            .Map(level => level * step - textView.ScrollOffset.X);

    static double Leading(TextView textView, DocumentLine document) =>
        textView.Document.GetText(document).TakeWhile(char.IsWhiteSpace).Count() * textView.WideSpaceWidth;

    public void Dispose() => subscription.Dispose();
}
```

## [03]-[GRAMMAR_BEHAVIOR]

- Owner: `RasmRegistry` the one product `IRegistryOptions`; `CodeGrammar` the product grammar-scope rows; `LanguagePlan` the compiled behavior projection; `LanguageMap` its generated mapper; `EnterPlan`/`IndentPlan` the compiled rule rows; `PlanIndentation` the indentation strategy.
- Cases: `CodeGrammar` = source.rasm | source.rasm-expression | source.json — registered through `RasmRegistry.GetGrammar` while every other language resolves through the corpus's extension and language-id lookup.
- Entry: `RasmRegistry.Scope(string languageOrExtension)` — product rows, then `GetScopeByLanguageId`, then `GetScopeByExtension`, refusing by NAME rather than returning a scope no grammar answers; `RasmRegistry.Configuration(string)` — the parsed `LanguageConfiguration` the corpus already hangs off each language row; `LanguageMap.ToPlan(LanguageConfiguration)` — the generated projection compiling every behaviour pattern ONCE, so no `Regex` mints per keystroke; `Install(directory)`/`Install(grammarName, packageJson)` — the file-backed growth path.
- Law: `GetInjections` is the injection hook and embedded languages declare IN the grammar JSON, so a DSL inside a string or a fenced language inside markdown tokenizes without a second installation; the standalone path reaches the same capability through `Registry.GrammarForScopeName(scope, initialLanguage, embeddedLanguages)`.
- Law: behaviour comes from `LanguageConfiguration` — comment markers, bracket pairs, auto-closing pairs with `NotIn` exclusions, surrounding pairs, fold markers, indentation patterns, on-enter rules; re-parsing that JSON at the pane is the deleted form, and so is a pattern compiled at match time.
- Packages: Avalonia.AvaloniaEdit, AvaloniaEdit.TextMate, TextMateSharp, TextMateSharp.Grammars, Riok.Mapperly, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one grammar scope row on `CodeGrammar` or one file-backed extension on the registry; a new behaviour column is one `LanguagePlan` member and its `LanguageMap` row.
- Boundary: `LanguageMap` is the `LanguageConfiguration → LanguagePlan` mapper at the Mapperly nested-path rung — `[MapProperty]` nested paths carry the direct columns and per-TYPE `[UserMapping]` converters the `Option`/`Seq` lifts; the three members needing whole-source readers (`AutoPairs` merges two source rosters; `FoldMarkers` and `Indent` compile from nested optionals) ride `[MapPropertyFromSource]` with the RMG020 cost declared HERE: source-side completeness on this mapping proves only by the target roster, which `RequiredMappingStrategy.Target` enforces. The retired `Unindent` pattern column was projected and read by nothing — a knob deleted, its pattern re-admittable as one `IndentPlan` column when a consumer lands.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CodeGrammar {
    public static readonly CodeGrammar Rasm = new("source.rasm");
    public static readonly CodeGrammar Expression = new("source.rasm-expression");
    public static readonly CodeGrammar Json = new("source.json");
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record EnterPlan(Regex Before, string Action, string Append);

public sealed record IndentPlan(Regex Increase, Regex Decrease);

public sealed record LanguagePlan(
    Option<string> LineComment,
    Option<(string Open, string Close)> BlockComment,
    Seq<(string Open, string Close)> Brackets,
    Seq<AutoPair> AutoPairs,
    Seq<(char Open, char Close)> Surrounds,
    Option<(Regex Start, Regex End)> FoldMarkers,
    Option<IndentPlan> Indent,
    Seq<EnterPlan> Enters,
    string AutoCloseBefore) {
    public static readonly LanguagePlan Empty = new(
        None, None, Seq<(string, string)>(), Seq<AutoPair>(), Seq<(char, char)>(), None, None, Seq<EnterPlan>(), string.Empty);

    public Unit Toggle(TextArea area) {
        LineComment.Iter(marker => {
            using IDisposable scope = area.Document.RunUpdate();
            Selected(area).Iter(line => Commented(area.Document, line, marker));
        });
        if (LineComment.IsNone) {
            BlockComment.Iter(pair => area.Document.Replace(area.Selection.SurroundingSegment,
                $"{pair.Open}{area.Selection.GetText()}{pair.Close}"));
        }
        return unit;
    }

    public Seq<(int First, int Last)> MarkerRegions(TextDocument document) =>
        FoldMarkers.Match(
            Some: markers => Regions(document, markers.Start, markers.End),
            None: () => Seq<(int, int)>());

    static Seq<(int First, int Last)> Regions(TextDocument document, Regex opens, Regex closes) =>
        toSeq(Enumerable.Range(1, document.LineCount)).Map(document.GetLineByNumber)
            .Fold((Open: Seq<int>(), Closed: Seq<(int First, int Last)>()), (state, line) =>
                document.GetText(line) switch {
                    var text when opens.IsMatch(text) => (Open: state.Open.Add(line.LineNumber), Closed: state.Closed),
                    var text when closes.IsMatch(text) => state.Open.Last.Match(
                        Some: open => (Open: state.Open.Init, Closed: state.Closed.Add((open, line.LineNumber))),
                        None: () => state),
                    _ => state,
                }).Closed;

    static Seq<DocumentLine> Selected(TextArea area) =>
        (First: area.Document.GetLineByOffset(area.Selection.SurroundingSegment.Offset).LineNumber,
         Last: area.Document.GetLineByOffset(area.Selection.SurroundingSegment.EndOffset).LineNumber) switch {
            var span => toSeq(Enumerable.Range(span.First, span.Last - span.First + 1)).Map(area.Document.GetLineByNumber),
        };

    static Unit Commented(TextDocument document, DocumentLine line, string marker) {
        if (document.GetText(line).TrimStart().StartsWith(marker, StringComparison.Ordinal)) {
            document.Replace(line, document.GetText(line).Replace(marker, string.Empty, StringComparison.Ordinal));
        }
        else { document.Insert(line.Offset, marker); }
        return unit;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class LanguageMap {
    [MapProperty("Comments.LineComment", nameof(LanguagePlan.LineComment))]
    [MapProperty("Comments.BlockComment", nameof(LanguagePlan.BlockComment))]
    [MapProperty(nameof(LanguageConfiguration.Brackets), nameof(LanguagePlan.Brackets))]
    [MapPropertyFromSource(nameof(LanguagePlan.AutoPairs), Use = nameof(Pairs))]
    [MapProperty(nameof(LanguageConfiguration.SurroundingPairs), nameof(LanguagePlan.Surrounds))]
    [MapPropertyFromSource(nameof(LanguagePlan.FoldMarkers), Use = nameof(Markers))]
    [MapPropertyFromSource(nameof(LanguagePlan.Indent), Use = nameof(Indentation))]
    [MapProperty("EnterRules.Rules", nameof(LanguagePlan.Enters))]
    [MapProperty(nameof(LanguageConfiguration.AutoCloseBefore), nameof(LanguagePlan.AutoCloseBefore))]
    public static partial LanguagePlan ToPlan(LanguageConfiguration configuration);

    [UserMapping] private static Option<string> Marker(string? marker) =>
        Optional(marker).Filter(static text => !string.IsNullOrEmpty(text));

    [UserMapping] private static Option<(string Open, string Close)> StringPair(IList<string>? pair) =>
        Optional(pair).Filter(static held => held.Count >= 2).Map(static held => (held[0], held[1]));

    [UserMapping] private static Seq<(string Open, string Close)> StringPairs(IList<IList<string>>? pairs) =>
        toSeq(pairs ?? []).Choose(StringPair);

    [UserMapping] private static Seq<(char Open, char Close)> CharPairs(IList<IList<char>>? pairs) =>
        toSeq(pairs ?? []).Filter(static pair => pair.Count >= 2).Map(static pair => (pair[0], pair[1]));

    [UserMapping] private static EnterPlan Enter(EnterRule rule) =>
        new(Compiled(rule.BeforeText), rule.ActionIndent ?? string.Empty, rule.AppendText ?? string.Empty);

    [UserMapping] private static string Tail(string? text) => text ?? string.Empty;

    private static Seq<AutoPair> Pairs(LanguageConfiguration configuration) =>
        toSeq(configuration.AutoClosingPairs?.AutoPairs ?? [])
            + toSeq(configuration.AutoClosingPairs?.CharPairs ?? []).Filter(static pair => pair.Count >= 2)
                .Map(static pair => new AutoPair { Open = pair[0].ToString(), Close = pair[1].ToString() });

    private static Option<(Regex Start, Regex End)> Markers(LanguageConfiguration configuration) =>
        Optional(configuration.Folding?.Markers)
            .Filter(static markers => !string.IsNullOrEmpty(markers.Start) && !string.IsNullOrEmpty(markers.End))
            .Map(static markers => (Compiled(markers.Start), Compiled(markers.End)));

    private static Option<IndentPlan> Indentation(LanguageConfiguration configuration) =>
        Optional(configuration.IndentationRules).Filter(static rules => !rules.IsEmpty)
            .Map(static rules => new IndentPlan(Compiled(rules.Increase), Compiled(rules.Decrease)));

    private static Regex Compiled(string pattern) => new(pattern, RegexOptions.Compiled);
}

public sealed class RasmRegistry(RegistryOptions corpus, HashMap<string, IRawGrammar> owned) : IRegistryOptions {
    public IRawTheme GetTheme(string scopeName) => corpus.GetTheme(scopeName);

    public IRawTheme GetDefaultTheme() => corpus.GetDefaultTheme();

    public ICollection<string> GetInjections(string scopeName) => corpus.GetInjections(scopeName);

    public IRawGrammar GetGrammar(string scopeName) =>
        owned.Find(scopeName).IfNone(() => corpus.GetGrammar(scopeName));

    public Fin<string> Scope(string languageOrExtension) =>
        CodeGrammar.TryGet(languageOrExtension, out CodeGrammar? product) && product is not null
            ? Fin.Succ(product.Key)
            : Optional(corpus.GetScopeByLanguageId(languageOrExtension))
                .Bind(scope => string.IsNullOrEmpty(scope) ? Option<string>.None : Some(scope))
                | Optional(corpus.GetScopeByExtension(Dotted(languageOrExtension)))
                    .Bind(scope => string.IsNullOrEmpty(scope) ? Option<string>.None : Some(scope))
            switch {
                { IsSome: true, Case: string scope } => Fin.Succ(scope),
                _ => Fin.Fail<string>(new EditFault.UnmatchedShape($"grammar scope for '{languageOrExtension}'")),
            };

    static string Dotted(string token) => token.StartsWith('.') ? token : $".{token}";

    public Option<LanguageConfiguration> Configuration(string languageOrExtension) =>
        Optional(corpus.GetLanguageByExtension(Dotted(languageOrExtension)))
            | toSeq(corpus.GetAvailableLanguages())
                .Find(row => string.Equals(row.Id, languageOrExtension, StringComparison.OrdinalIgnoreCase))
        switch {
            { IsSome: true, Case: Language language } => Optional(language.Configuration),
            _ => None,
        };

    public Unit Install(string directory) {
        corpus.LoadFromLocalDir(directory, overwrite: true);
        return unit;
    }

    public Unit Install(string grammarName, FileInfo packageJson) {
        corpus.LoadFromLocalFile(grammarName, packageJson, overwrite: true);
        return unit;
    }
}

public sealed class PlanIndentation(LanguagePlan plan, TextEditorOptions options) : IIndentationStrategy {
    public void IndentLine(TextDocument document, DocumentLine line) =>
        Optional(line.PreviousLine).Iter(previous => document.Replace(
            line.Offset, Leading(document.GetText(line)).Length, Indented(document.GetText(previous))));

    public void IndentLines(TextDocument document, int beginLine, int endLine) =>
        toSeq(Enumerable.Range(beginLine, endLine - beginLine + 1))
            .Map(document.GetLineByNumber)
            .Iter(line => IndentLine(document, line));

    string Indented(string previous) =>
        plan.Enters.Find(rule => rule.Before.IsMatch(previous)).Match(
            Some: rule => Leading(previous) + Shifted(rule.Action) + rule.Append,
            None: () => plan.Indent.Match(
                Some: rules => rules.Increase.IsMatch(previous) ? Leading(previous) + options.IndentationString
                    : rules.Decrease.IsMatch(previous) ? Trimmed(Leading(previous))
                    : Leading(previous),
                None: () => Leading(previous)));

    string Shifted(string action) => action switch {
        "indent" => options.IndentationString,
        "indentOutdent" => options.IndentationString,
        _ => string.Empty,
    };

    string Trimmed(string leading) =>
        leading.Length >= options.IndentationString.Length ? leading[options.IndentationString.Length..] : string.Empty;

    static string Leading(string text) => new(text.TakeWhile(char.IsWhiteSpace).ToArray());
}
```

## [04]-[CODE_PANE]

- Owner: `LaneSource` the one per-lane mark lookup delegate (the conflict mount and every pane producer publish it; the overview strip consumes it); `PaneAffordance` the pane capability vocabulary; `CodeSession` the session value holding the custody chain; `CodePane` the boundary capsule; `FoldRegion` with `CodePane.Fold` the whole-set resync.
- Entry: `Open(TextEditor editor, RasmRegistry registry, string language, ResolvedTheme resolved, LaneSource segments)` — `Fin<CodeSession>`: scope admission, then grammar, search, folding, behavior, and chrome acquired as ONE chain whose failure arm releases LIFO through `Custody.Rollback` and whose success value owns every handle through the session's one composite; `Frames(CodeSession session, IObservable<Unit> ticks)` publishes the overview strip's content-space feed under `SourceKey`; `Fold(FoldingManager manager, TextDocument document, Seq<FoldRegion> regions, Option<int> firstError = default)` — the whole-set resync over regions the caller already computed.
- Law: UNDO ownership splits by PLANE and neither stack wraps the other — the editor's `UndoStack` owns in-pane text history (a keystroke is a rope edit with no revertible-op payload), the `Editing/history` recorder owns property-cell history, and a pane whose text is a durable document routes its COMMITTED text through the `EditGate` entry at commit; every multi-edit pane operation folds through one `DeclareChangeBlock` scope so it undoes as one step.
- Law: `UpdateFoldings` is the diff — it reuses the section whose `StartOffset` repeats, resizing and re-titling in place so `IsFolded` survives a re-parse; `CreateFolding` appends unconditionally, so a per-region mint doubles the margin and orphans every region the user opened. Regions arrive sorted ascending (the manager throws otherwise), degenerate spans drop before the call, and `firstError` bounds the trusted range — `None` collapses to the package's whole-document `-1` at the one call; `DefaultClosed` binds only on the manager's first update.
- Result: the pane publishes its overview frame under `SourceKey`; the strip's own intent is one `ControlIntent.Overview(StripKey, OverviewAxis.Vertical, SourceKey, JumpVerb, binding)` the HOSTING surface constructs — `Editing/history.md` `TimelineSurface.Body` is the witness form, so a pane-side intent factory nothing called is gone and the keys stay the contract.
- Packages: Avalonia.AvaloniaEdit, AvaloniaEdit.TextMate, System.Reactive, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one pane capability is one `PaneAffordance` row; a fifth overview lane reaches the session through the one `LaneSource` column with no signature change.
- Boundary: `Open` is the editor boundary capsule — one TextMate installation per editor, released with the session; mount order is the preconditions' own: scope admits before any owner exists, the options row enables the renderers the chrome colours, `SearchPanel.Install` precedes the ink set because its brush write drops otherwise, and the behavior plane binds last over a live document. Read-only panes (the evidence and conflict viewers) are the same capsule under a grant-less `Editable`. Markdown never renders here — the typography projection owns it and the code pane owns only fenced code.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

public delegate Seq<TextSegment> LaneSource(OverviewLane lane);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaneAffordance : ICapability<PaneAffordance> {
    public static readonly PaneAffordance Editable = new(key: "editable");
    public static readonly PaneAffordance LineNumbers = new(key: "line-numbers");
    public static readonly PaneAffordance Folding = new(key: "folding");
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct FoldRegion(int First, int Last, string Title, bool Closed);

public sealed record CodeSession(
    TextMate.Installation Grammar,
    Option<FoldingManager> Folding,
    SearchPanel Search,
    LanguagePlan Plan,
    TextEditor Editor,
    LaneSource Segments,
    IDisposable Custody) : IDisposable {
    public Rect Content() => new(0d, 0d, Editor.ExtentWidth, Editor.ExtentHeight);

    public Rect Viewport() => new(Editor.HorizontalOffset, Editor.VerticalOffset, Editor.ViewportWidth, Editor.ViewportHeight);

    public Seq<Rect> Marks(OverviewLane lane) =>
        Segments(lane).Bind(segment => toSeq(BackgroundGeometryBuilder
            .GetRectsForSegment(Editor.TextArea.TextView, segment))
            .Map(rect => rect.Translate(new Vector(Editor.HorizontalOffset, Editor.VerticalOffset))));

    public void Dispose() => Custody.Dispose();
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed record CodePane(
    CapabilitySet<PaneAffordance> Affordances,
    EditorOptionsRow Options,
    CompletionPolicy Completion) {
    public const string SourceKey = "inspector.code.overview";
    public const string StripKey = $"{SourceKey}.strip";
    public const string JumpVerb = $"{SourceKey}.jump";

    public IObservable<OverviewFrame> Frames(CodeSession session, IObservable<Unit> ticks) =>
        ticks.Merge(Laid(session.Editor))
            .StartWith(unit)
            .Select(_ => new OverviewFrame(
                session.Content(),
                session.Viewport(),
                Seq(new OverviewBand(OverviewLane.Change, session.Marks(OverviewLane.Change)),
                    new OverviewBand(OverviewLane.Search, session.Marks(OverviewLane.Search)),
                    new OverviewBand(OverviewLane.Error, session.Marks(OverviewLane.Error)),
                    new OverviewBand(OverviewLane.Selection, session.Marks(OverviewLane.Selection)))))
            .DistinctUntilChanged()
            .Replay(1)
            .RefCount();

    static IObservable<Unit> Laid(TextEditor editor) =>
        Observable.FromEventPattern(
            handler => editor.LayoutUpdated += handler,
            handler => editor.LayoutUpdated -= handler)
            .Select(static _ => unit);

    public Fin<CodeSession> Open(
        TextEditor editor, RasmRegistry registry, string language, ResolvedTheme resolved, LaneSource segments) =>
        registry.Scope(language).Bind(scope => {
            TextMate.Installation? grammar = null;
            IDisposable? searchScope = null;
            IDisposable? foldingScope = null;
            IDisposable? inks = null;
            IDisposable? behaviors = null;
            Fin<CodeSession> mounted = Try.lift(() => {
                editor.IsReadOnly = !Affordances.Admits(PaneAffordance.Editable);
                editor.ShowLineNumbers = Affordances.Admits(PaneAffordance.LineNumbers);
                editor.WordWrap = false;
                editor.Options = Options.Apply(editor.Options);
                grammar = editor.InstallTextMate(registry);
                grammar.SetGrammar(scope);
                grammar.SetTheme(new TokenRawTheme(registry.GetDefaultTheme(), resolved));
                SearchPanel search = SearchPanel.Install(editor);
                searchScope = Disposable.Create(search.Uninstall);
                Option<FoldingManager> folding = Affordances.Admits(PaneAffordance.Folding)
                    ? Some(FoldingManager.Install(editor.TextArea))
                    : Option<FoldingManager>.None;
                foldingScope = folding.Match(
                    Some: manager => Disposable.Create(() => FoldingManager.Uninstall(manager)),
                    None: () => Disposable.Empty);
                LanguagePlan plan = Planned(registry, language);
                editor.TextArea.IndentationStrategy = new PlanIndentation(plan, editor.Options);
                inks = EditorInk.Bind(editor, Options);
                EventHandler<TextInputEventArgs> closing = (_, args) => ignore(AutoClose(editor.TextArea, plan, args.Text));
                editor.TextArea.TextEntered += closing;
                behaviors = Disposable.Create(() => editor.TextArea.TextEntered -= closing);
                return Fin.Succ(new CodeSession(grammar, folding, search, plan, editor, segments,
                    new CompositeDisposable(behaviors, inks, foldingScope, searchScope, grammar)));
            }).Run().Bind(static inner => inner);
            return mounted.Rollback(behaviors, inks, foldingScope, searchScope, grammar);
        });

    static Unit AutoClose(TextArea area, LanguagePlan plan, string? entered) {
        Option<char> following = area.Caret.Offset < area.Document.TextLength
            ? Some(area.Document.GetCharAt(area.Caret.Offset))
            : None;
        bool admits = plan.AutoCloseBefore.Length is 0 || following.ForAll(plan.AutoCloseBefore.Contains);
        plan.AutoPairs.Find(pair => pair.Open == entered).Filter(_ => admits).Iter(pair => {
            using IDisposable scope = area.Document.RunUpdate();
            area.Document.Insert(area.Caret.Offset, pair.Close);
            area.Caret.Offset -= pair.Close.Length;
        });
        return unit;
    }

    static LanguagePlan Planned(RasmRegistry registry, string language) =>
        registry.Configuration(language).Map(LanguageMap.ToPlan).IfNone(LanguagePlan.Empty);

    public static Unit Fold(FoldingManager manager, TextDocument document, Seq<FoldRegion> regions, Option<int> firstError = default) {
        manager.UpdateFoldings(
            regions.Choose(region => Admitted(document, region))
                .OrderBy(static region => region.StartOffset)
                .ToArray(),
            firstError.IfNone(-1));
        return unit;
    }

    static Option<NewFolding> Admitted(TextDocument document, FoldRegion region) =>
        region is { First: >= 1 } && region.Last > region.First && region.Last <= document.LineCount
            ? Some(new NewFolding(document.GetLineByNumber(region.First).EndOffset, document.GetLineByNumber(region.Last).EndOffset) {
                Name = region.Title,
                DefaultClosed = region.Closed,
            })
            : Option<NewFolding>.None;

    public static CompletionWindow Assist(TextEditor editor, Seq<CompletionRow> rows, int triggerStart) {
        CompletionWindow window = new(editor.TextArea) {
            StartOffset = triggerStart,
            CloseAutomatically = true,
            CloseWhenCaretAtBeginning = true,
        };
        rows.Iter(row => window.CompletionList.CompletionData.Add(row));
        window.Show();
        return window;
    }

    public static Fin<OverloadInsightWindow> Overloads(TextEditor editor, Seq<CompletionRow> rows, int selected) {
        if (rows.IsEmpty) { return Fin.Fail<OverloadInsightWindow>(new EditFault.UnmatchedShape("overload insight over an empty signature set")); }
        OverloadInsightWindow window = new(editor.TextArea) { Provider = new OverloadRows(rows) { SelectedIndex = selected } };
        window.Show();
        return Fin.Succ(window);
    }
}
```

## [05]-[ASSIST]

- Owner: `CompletionKind` the completion-family axis with `CompletionPolicy` its weight rows; `CompletionRow` the one `ICompletionData` projection; `OverloadRows` the one `IOverloadProvider` over the same row set.
- Cases: `CompletionKind` = section | member | quantity | intent | snippet — explicit `Rank` carries family precedence, `Insert` the row delegate column; the plain arm replaces the trigger span, the snippet arm removes it first because `Snippet.Insert` drives its own placeholder session.
- Law: rank is STRUCTURAL (the family ordering) and the numeric weight is a POLICY value the projection applies, so a re-weighting is one policy edit and never fourteen literals; `Priority` is a `double` on the package contract, so an `int` tier scale cannot express a tie-break.
- Packages: Avalonia.AvaloniaEdit, bodong.PropertyModels, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one completion family is one `CompletionKind` row carrying key, rank, and insertion column; the families are the page's own symbol vocabulary — options section keys, nameof-derived policy member names, `Quantity.Infos` unit abbreviations, resolution intent keys.
- Boundary: `IOverloadProvider` carries five members and NO caret hook — the window wires Up and Down through its own `ChangeIndex` only while `Count > 1`, so re-selecting a signature as arguments land is the consumer assigning `SelectedIndex` off the same projection the list mounts; `OverloadRows` extends `PropertyModels.ComponentModel.ReactiveObject` (named in full because ReactiveUI publishes a same-named screen base) so `[DependsOnProperty]` raises the three derived members off the one index write; insertion runs only on the `ICompletionData.Complete` arm — a pane-side document mutation is the deleted form.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CompletionKind {
    public static readonly CompletionKind Section = new("section", rank: 0, insert: Replace);
    public static readonly CompletionKind Member = new("member", rank: 1, insert: Replace);
    public static readonly CompletionKind Quantity = new("quantity", rank: 2, insert: Replace);
    public static readonly CompletionKind Intent = new("intent", rank: 3, insert: Replace);
    public static readonly CompletionKind Snippet = new("snippet", rank: 4, insert: Expand);

    public int Rank { get; }

    [UseDelegateFromConstructor]
    public partial Unit Insert(TextArea area, ISegment trigger, CompletionRow row);

    private static Unit Replace(TextArea area, ISegment trigger, CompletionRow row) {
        area.Document.Replace(trigger, row.Body);
        return unit;
    }

    private static Unit Expand(TextArea area, ISegment trigger, CompletionRow row) =>
        row.Template.Match(
            Some: template => {
                area.Document.Remove(trigger);
                template.Insert(area);
                return unit;
            },
            None: () => Replace(area, trigger, row));
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record CompletionPolicy(double Head, double Step) {
    public static readonly CompletionPolicy Default = new(Head: 50d, Step: 10d);

    public double Weight(CompletionKind kind) => Head - kind.Rank * Step;
}

public sealed record CompletionRow(
    CompletionKind Kind,
    string Key,
    string Detail,
    string Body,
    double Priority,
    Option<Snippet> Template,
    Option<IImage> Glyph) : ICompletionData {
    public string Text => Body;
    public object Content => Key;
    public object Description => Detail;

    IImage ICompletionData.Image => Glyph.ValueUnsafe()!;

    public void Complete(TextArea area, ISegment trigger, EventArgs request) => ignore(Kind.Insert(area, trigger, this));

    public static Seq<CompletionRow> Project(
        Seq<(CompletionKind Kind, string Key, string Detail, string Body, Option<Snippet> Template)> symbols,
        CompletionPolicy policy,
        Func<CompletionKind, Option<IImage>> glyph) =>
        toSeq(symbols
            .Map(row => new CompletionRow(row.Kind, row.Key, row.Detail, row.Body, policy.Weight(row.Kind), row.Template, glyph(row.Kind)))
            .OrderByDescending(static row => row.Priority)
            .ThenBy(static row => row.Key, ComparerAccessors.StringOrdinalIgnoreCase.Comparer));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed class OverloadRows(Seq<CompletionRow> rows) : PropertyModels.ComponentModel.ReactiveObject, IOverloadProvider {
    private int selected;

    public int SelectedIndex {
        get => selected;
        set => ignore(SetProperty(ref selected, int.Clamp(value, 0, rows.Count - 1)));
    }

    public int Count => rows.Count;

    [DependsOnProperty(nameof(SelectedIndex))]
    public string CurrentIndexText => $"{SelectedIndex + 1}/{Count}";

    [DependsOnProperty(nameof(SelectedIndex))]
    public object CurrentHeader => rows[SelectedIndex].Key;

    [DependsOnProperty(nameof(SelectedIndex))]
    public object CurrentContent => rows[SelectedIndex].Detail;
}
```
