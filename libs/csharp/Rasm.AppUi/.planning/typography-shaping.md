# [APPUI_TYPOGRAPHY_SHAPING]

One typographic law serves every AppUi surface: `TypographyRole` is the ten-row vocabulary every product text appearance traces to, `FontChain` rows make font admission deterministic per platform, and one HarfBuzz shaping rail places every Skia-rendered glyph. `MarkdownProjection` folds the Markdig AST into role-keyed rows so document panels ride the same vocabulary, and `TextMetricsPolicy` owns baseline-grid math and measurement. The package spine is Avalonia.Fonts.Inter for the embedded faces, SkiaSharp.HarfBuzz over the centrally pinned HarfBuzz natives for shaping, and Markdig for document structure; retained styles, chart paints, editor fonts, table columns, and shaped labels all consume one resolved `TextStyleRow`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]           | [OWNS]                                                        |
| :-----: | ------------------- | ------------------------------------------------------------- |
|   [1]   | ROLE_AXIS           | Ten role rows; every text appearance literal traces here      |
|   [2]   | FONT_ADMISSION      | Deterministic embedded-Inter admission; ranked per-platform fallback chains |
|   [3]   | SHAPING_RAIL        | One HarfBuzz shaping rail before every Skia glyph draw        |
|   [4]   | MARKDOWN_PROJECTION | Markdig AST folds to role-keyed rows and inline runs          |
|   [5]   | TEXT_METRICS        | Baseline-grid math, measurement, trimming, tabular-numeral proof |

## [2]-[ROLE_AXIS]

- Owner: `TypographyRole`
- Cases: display, headline, title, subtitle, body, body-strong, caption, overline, code, numeric
- Entry: `public static TextStyleRow Resolve(TypographyRole role, FontChain chain)` — pure fold; the resolved row is the only typographic product any consumer reads.
- Auto: one role resolve yields retained styles, chart paints, editor fonts, table columns, and shaped Skia labels alike — per-label font, size, weight, and feature setup call sites are deleted.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new text appearance is one `TypographyRole` row; zero new surface.
- Boundary: every size, weight, tracking, line-height, and OpenType-feature literal in AppUi traces to a role row — a bare font value at a call site is the named defect and the deleted pattern; numeric and temporal text arrives pre-formatted through the `ClockPolicy` NodaTime patterns and the `CompositeFormat` rail, and the numeric row guarantees tabular glyph geometry only; uppercase casing applies at presentation from the row flag; wrap behavior is a row column consumed by the metrics policy; retained-rail feature and letter-spacing application rides its research row.

```csharp signature
public sealed class TypographyKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TypographyKeyPolicy, string>]
[KeyMemberComparer<TypographyKeyPolicy, string>]
public sealed partial class TypographyRole {
    public static readonly TypographyRole Display = new("display", size: 32d, lineHeight: 40d, weight: 600, tracking: -0.02d, mono: false, uppercase: false, wraps: false, features: Seq("calt"));
    public static readonly TypographyRole Headline = new("headline", size: 24d, lineHeight: 32d, weight: 600, tracking: -0.01d, mono: false, uppercase: false, wraps: false, features: Seq("calt"));
    public static readonly TypographyRole Title = new("title", size: 18d, lineHeight: 24d, weight: 600, tracking: 0d, mono: false, uppercase: false, wraps: false, features: Seq("calt"));
    public static readonly TypographyRole Subtitle = new("subtitle", size: 16d, lineHeight: 22d, weight: 500, tracking: 0d, mono: false, uppercase: false, wraps: true, features: Seq("calt"));
    public static readonly TypographyRole Body = new("body", size: 14d, lineHeight: 20d, weight: 400, tracking: 0d, mono: false, uppercase: false, wraps: true, features: Seq("calt"));
    public static readonly TypographyRole BodyStrong = new("body-strong", size: 14d, lineHeight: 20d, weight: 600, tracking: 0d, mono: false, uppercase: false, wraps: true, features: Seq("calt"));
    public static readonly TypographyRole Caption = new("caption", size: 12d, lineHeight: 16d, weight: 400, tracking: 0d, mono: false, uppercase: false, wraps: true, features: Seq("calt"));
    public static readonly TypographyRole Overline = new("overline", size: 11d, lineHeight: 16d, weight: 500, tracking: 0.08d, mono: false, uppercase: true, wraps: false, features: Seq("calt"));
    public static readonly TypographyRole Code = new("code", size: 13d, lineHeight: 20d, weight: 400, tracking: 0d, mono: true, uppercase: false, wraps: false, features: Seq("calt"));
    public static readonly TypographyRole Numeric = new("numeric", size: 14d, lineHeight: 20d, weight: 400, tracking: 0d, mono: false, uppercase: false, wraps: false, features: Seq("tnum", "calt", "ss01"));

    public double Size { get; }

    public double LineHeight { get; }

    public int Weight { get; }

    public double Tracking { get; }

    public bool Mono { get; }

    public bool Uppercase { get; }

    public bool Wraps { get; }

    public Seq<string> Features { get; }
}

public sealed record TextStyleRow(string Family, double Size, int Weight, double Tracking, double LineHeight, Seq<string> Features, bool Uppercase, bool Wraps) {
    public static TextStyleRow Resolve(TypographyRole role, FontChain chain) =>
        new(
            Family: string.Join(", ", role.Mono ? chain.Mono : chain.Sans),
            Size: role.Size,
            Weight: role.Weight,
            Tracking: role.Tracking,
            LineHeight: role.LineHeight,
            Features: role.Features,
            Uppercase: role.Uppercase,
            Wraps: role.Wraps);
}
```

## [3]-[FONT_ADMISSION]

- Owner: `FontChain`
- Cases: MacOS | Windows | Linux
- Entry: `public static AppBuilder Admit(AppBuilder builder)` — one boot-time admission on the application builder; no second font registration path exists.
- Packages: Avalonia.Fonts.Inter, Avalonia, SkiaSharp, LanguageExt.Core
- Growth: a new platform or script coverage is one `FontChain` row or one ranked family value on an existing row; zero new surface.
- Boundary: the chain row binds once at composition from the resolved profile — ambient OS probing and system-font assumptions are the deleted patterns; embedded Inter resolves first on every surface, host families participate only as declared ranked rows beneath it, and the symbols row terminates every walk; the mono ranks exist for the code role only; the retained fallback-chain knob spelling rides its research row.

```csharp signature
public sealed record FontChain(string Rid, Seq<string> Sans, Seq<string> Mono, string Symbols) {
    public static readonly FontChain MacOS = new("osx", Sans: Seq("Inter", "SF Pro Text"), Mono: Seq("SF Mono", "Menlo"), Symbols: "Apple Color Emoji");
    public static readonly FontChain Windows = new("win", Sans: Seq("Inter", "Segoe UI"), Mono: Seq("Cascadia Mono", "Consolas"), Symbols: "Segoe UI Emoji");
    public static readonly FontChain Linux = new("linux", Sans: Seq("Inter", "Noto Sans"), Mono: Seq("Noto Sans Mono", "DejaVu Sans Mono"), Symbols: "Noto Color Emoji");

    public SKTypeface Face(SKFontManager manager, bool mono) =>
        (mono ? Mono : Sans)
            .Map(manager.CreateTypeface)
            .Filter(static face => face is not null)
            .HeadOrNone()
            .IfNone(() => manager.CreateTypeface(Symbols));
}

public static class FontAdmission {
    public static AppBuilder Admit(AppBuilder builder) => builder.WithInterFont();
}
```

## [4]-[SHAPING_RAIL]

- Owner: `ShapingSurface`
- Entry: `public static Unit DrawLabel(SKCanvas canvas, SKShaper shaper, SKFont font, SKPaint paint, string text, float x, float y)` — boundary write onto a caller-leased canvas; the lease rail lives with the canvas owner.
- Receipt: the first shaped draw on a profile emits the libHarfBuzzSharp load identity — version, path, RID — consumed as typography proof by the evidence stream.
- Packages: SkiaSharp.HarfBuzz, SkiaSharp, HarfBuzzSharp.NativeAssets.macOS, HarfBuzzSharp.NativeAssets.Win32, HarfBuzzSharp.NativeAssets.Linux, LanguageExt.Core
- Growth: a new script or feature requirement is one policy value on the role row riding the same shaping call; zero new surface.
- Boundary: shaping precedes drawing for every Skia-rendered glyph — manual glyph placement, per-script branches, and per-control glyph positioning are the deleted patterns; bidi and complex-script resolution happen inside the shaper; one central HarfBuzz native line serves the retained text stack and the shaped rail on every desktop profile; per-role feature tags enter the shaped rail through the gated feature-array route.

```csharp signature
public static class ShapingSurface {
    public static SKShaper Shaper(SKTypeface face) => new(face);

    public static Unit DrawLabel(SKCanvas canvas, SKShaper shaper, SKFont font, SKPaint paint, string text, float x, float y) =>
        fun(() => canvas.DrawShapedText(shaper, text, x, y, font, paint))();
}
```

## [5]-[MARKDOWN_PROJECTION]

- Owner: `MarkdownProjection`
- Cases: Heading | Paragraph | Quote | ListRows | Grid | CodeFence | Rule
- Entry: `public static Seq<MarkdownRow> Project(string markdown)` — pure fold from document text to role-keyed rows; presentation consumes rows, never the AST.
- Packages: Markdig, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new document construct is one `MarkdownRow` case plus one dispatch arm on the same fold; the pipe-table arm will land on the declared `Grid` case once its block family spelling is proven; zero new surface.
- Boundary: the pipeline is built once with in-package extensions only; `CodeFence` payloads hand off to the code-editor surface with their language tag — the projection never highlights or renders code; `HtmlBlock` and `HtmlInline` payloads degrade to empty runs so raw HTML never enters the retained tree; document headings cap at `Headline` — `Display` is reserved for shell hero text; Markdown.Avalonia and any parallel Markdown node model are the deleted patterns; retained materialization of `InlineRun` sequences rides its research row.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MarkdownRow {
    private MarkdownRow() { }

    public sealed record Heading(TypographyRole Role, Seq<InlineRun> Runs) : MarkdownRow;

    public sealed record Paragraph(Seq<InlineRun> Runs) : MarkdownRow;

    public sealed record Quote(Seq<MarkdownRow> Children) : MarkdownRow;

    public sealed record ListRows(bool Ordered, Seq<Seq<MarkdownRow>> Items) : MarkdownRow;

    public sealed record Grid(Seq<Seq<InlineRun>> Cells) : MarkdownRow;

    public sealed record CodeFence(string Language, string Source) : MarkdownRow;

    public sealed record Rule : MarkdownRow;
}

public readonly record struct InlineRun(string Text, bool Strong, bool Emphasis, bool Code, Option<string> Link);

public static class MarkdownProjection {
    public static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static Seq<MarkdownRow> Project(string markdown) =>
        toSeq<Block>(Markdown.Parse(markdown, Pipeline)).Map(Row);

    public static TypographyRole HeadingRole(int level) =>
        level switch { 1 => TypographyRole.Headline, 2 => TypographyRole.Title, 3 => TypographyRole.Subtitle, _ => TypographyRole.BodyStrong };

    private static MarkdownRow Row(Block block) =>
        block switch {
            HeadingBlock heading => new MarkdownRow.Heading(HeadingRole(heading.Level), Runs(heading)),
            FencedCodeBlock fence => new MarkdownRow.CodeFence(fence.Info ?? "", fence.Lines.ToString()),
            CodeBlock code => new MarkdownRow.CodeFence("", code.Lines.ToString()),
            QuoteBlock quote => new MarkdownRow.Quote(toSeq<Block>(quote).Map(Row)),
            ListBlock list => new MarkdownRow.ListRows(list.IsOrdered, toSeq<Block>(list).Map(static item => toSeq<Block>((ListItemBlock)item).Map(Row))),
            ThematicBreakBlock => new MarkdownRow.Rule(),
            ParagraphBlock paragraph => new MarkdownRow.Paragraph(Runs(paragraph)),
            LeafBlock leaf => new MarkdownRow.Paragraph(Runs(leaf)),
            ContainerBlock container => new MarkdownRow.Quote(toSeq<Block>(container).Map(Row)),
            _ => new MarkdownRow.Rule(),
        };

    private static Seq<InlineRun> Runs(LeafBlock leaf) =>
        Optional(leaf.Inline)
            .Map(static inline => toSeq(inline.Descendants<LeafInline>()).Map(Flatten))
            .IfNone(Seq<InlineRun>());

    private static InlineRun Flatten(LeafInline node) =>
        node switch {
            CodeInline code => new InlineRun(code.Content, Strong: false, Emphasis: false, Code: true, Link: None),
            LiteralInline literal => new InlineRun(
                Text: literal.Content.ToString(),
                Strong: Ancestry(literal).Exists(static a => a is EmphasisInline { DelimiterCount: >= 2 }),
                Emphasis: Ancestry(literal).Exists(static a => a is EmphasisInline { DelimiterCount: 1 }),
                Code: false,
                Link: Ancestry(literal).Filter(static a => a is LinkInline).Map(static a => ((LinkInline)a).Url ?? "").HeadOrNone()),
            LineBreakInline => new InlineRun(" ", Strong: false, Emphasis: false, Code: false, Link: None),
            _ => new InlineRun("", Strong: false, Emphasis: false, Code: false, Link: None),
        };

    private static Seq<Inline> Ancestry(Inline node) =>
        Optional(node.Parent)
            .Map(static parent => ((Inline)parent).Cons(Ancestry(parent)))
            .IfNone(Seq<Inline>());
}
```

```mermaid
flowchart LR
    MarkdownProjection --> MarkdownRow
    MarkdownRow --> InlineRun
    MarkdownRow --> TypographyRole
    TypographyRole --> TextStyleRow
    FontChain --> TextStyleRow
    TextStyleRow --> ShapingSurface
    TextStyleRow --> TextMetricsPolicy
```

## [6]-[TEXT_METRICS]

- Owner: `TextMetricsPolicy`
- Entry: `public double LineBox(SKFontMetrics metrics)` — pure value; the snapped line box every text container sizes against.
- Packages: SkiaSharp, BCL inbox
- Growth: a new metric rule is one policy value on `TextMetricsPolicy`; zero new surface.
- Boundary: measurement uses `MeasureText` and the shaped rail only — hand-rolled width estimation is the deleted pattern; the baseline unit snaps every text box so mixed-role layouts share one vertical rhythm; non-wrapping roles trim with character ellipsis at the retained layer per the role row's wrap column; tabular advance constancy for the numeric role is proven by equal `Advance` results over digit permutations in headless evidence.

```csharp signature
public sealed record TextMetricsPolicy(double BaselineUnit) {
    public static readonly TextMetricsPolicy Grid = new(BaselineUnit: 4d);

    public double Snap(double height) => Math.Ceiling(height / BaselineUnit) * BaselineUnit;

    public double LineBox(SKFontMetrics metrics) => Snap(metrics.Descent - metrics.Ascent + metrics.Leading);

    public double CapCenter(SKFontMetrics metrics, double box) => (box + metrics.CapHeight) / 2d;

    public static float Advance(SKFont font, string text) => font.MeasureText(text);
}
```

## [7]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                  | [PROOF]                                                                              | [GATE]              |
| :-----: | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- | ------------------- |
|   [1]   | Retained-rail OpenType feature and letter-spacing property spelling for role-row application | uv run python -m tools.assay api query avalonia Avalonia.Media.FontFeature             | ROLE_AXIS           |
|   [2]   | ConfigureFonts payload and FontManagerOptions fallback-chain knob spelling                 | uv run python -m tools.assay api query avalonia Avalonia.Media.FontManagerOptions      | FONT_ADMISSION      |
|   [3]   | SKFontManager.CreateTypeface lookup arity for the chain-walk fold                          | uv run python -m tools.assay api query skiasharp SkiaSharp.SKFontManager.CreateTypeface | FONT_ADMISSION      |
|   [4]   | DrawShapedText overload arity and SKShaper.Result member spelling                          | uv run python -m tools.assay api query skiasharp.harfbuzz SkiaSharp.HarfBuzz.CanvasExtensions | SHAPING_RAIL        |
|   [5]   | HarfBuzz feature-array shaping route carrying per-role feature tags                        | uv run python -m tools.assay api query harfbuzzsharp HarfBuzzSharp.Font.Shape           | SHAPING_RAIL        |
|   [6]   | Markdig block payload member spellings and container enumeration for the dispatch arms     | uv run python -m tools.assay api query markdig Markdig.Syntax                           | MARKDOWN_PROJECTION |
|   [7]   | Markdig inline payload member spellings for the run flatten                                | uv run python -m tools.assay api query markdig Markdig.Syntax.Inlines                   | MARKDOWN_PROJECTION |
|   [8]   | Markdig pipe-table extension block family spelling for the Grid dispatch arm               | uv run python -m tools.assay api query markdig Markdig.Extensions.Tables.Table          | MARKDOWN_PROJECTION |
|   [9]   | Avalonia inline document element family spelling for retained InlineRun materialization    | uv run python -m tools.assay api query avalonia Avalonia.Controls.Documents.Run         | MARKDOWN_PROJECTION |
|  [10]   | SKFontMetrics member spelling for cap-height and baseline-box math                         | uv run python -m tools.assay api query skiasharp SkiaSharp.SKFontMetrics                | TEXT_METRICS        |
