# [APPUI_RICH_CONTENT_MEDIA]

A rich-content-and-media owner renders markdown to live Avalonia visuals and plays image/svg/video/audio through one `MediaSurface` over codec rows, so documentation cells, help, and embedded media become first-class content surfaces beside the code editor. `MarkdownRenderer` walks the `Theme/typography` `MarkdownRow`/`InlineRun` projection into theme-token-styled blocks — every one of the eleven arms materializing, with fences on a recessed mono surface under the registry-backed grammar lookup, grids projected onto the real `Editing/tables` column rows, callouts tinted by their own kind row, and heading anchors retained as the document outline — and `MediaSurface` is the `[Union]` over image/svg/video/audio codec rows whose materialized control crosses to its host through the one `Shell/hosts.md` `Surfaces.Mount` entry. `HanumanInstitute.LibMpv.Avalonia` drives video/audio, the admitted `AsyncImageLoader` the raster row, and the `Theme/assets` `SvgPipeline` the vector row. `PlaybackTransport` binds the observed playback state to the settled `Render/animation` `TransportVerb` grammar and drains a raised command channel in order, `CaptionTrack` streams a media source into a sidecar subtitle the player itself times, `GallerySurface` composes filmstrip, lightbox, and zoom from owners that already exist, and `DiffSeats` mounts the `Collab/sync` compare session's structured diff into as many panes as its layout row declares. The page owns the markdown retained materialization, the media codec-row union, the playback transport with its chrome, the caption capture seat, the gallery, and the diff seat; it mints no second markdown model, no second image cache, no second transport grammar, no second pan-zoom engine, and no second differ. The spine is `Theme/typography` `MarkdownProjection`, `Avalonia.Controls.Documents`, `AsyncImageLoader.Avalonia`, `Theme/assets` `SvgPipeline`, `HanumanInstitute.LibMpv`/`HanumanInstitute.LibMpv.Avalonia` (`.api/api-libmpv.md`), `Whisper.net` (`.api/api-whisper-net.md`), `PanAndZoom`, the `Shell/hosts.md` mount entry, the kernel fault, lease, custody, capability, and timeline owners, Riok.Mapperly, Thinktecture.Runtime.Extensions, and LanguageExt result types.

## [01]-[INDEX]

- [02]-[MARKDOWN_BLOCKS]: The eleven-arm retained materialization, the slot-table styling context, the registry-backed fence, the grid projection, and the outline.
- [03]-[MEDIA_SURFACE]: The `MediaSurface` `[Union]` codec rows materialized onto one kernel-leased mount for the one `Surfaces.Mount` crossing.
- [04]-[PLAYBACK_TRANSPORT]: One transport path over the libmpv `MpvContext` consuming the settled `TransportVerb` grammar, draining a raised command channel and observing state event-driven.
- [05]-[TRANSPORT_CHROME]: The transport bar, the caption band, the playlist and frame-grab verbs, and the clock-subordination law the role row carries.
- [06]-[CAPTION_TRACK]: The sidecar caption route — VAD-gated streaming transcription under the locale caption policy, timed by the player's own subtitle properties.
- [07]-[GALLERY_SURFACE]: Filmstrip, lightbox, zoom, and honest load state over the thumbnail variants and the overlay canvas.
- [08]-[DIFF_SEAT]: The structured property-and-text diff seat mounting the compare session's surface into layout-declared panes under one custody chain.

## [02]-[MARKDOWN_BLOCKS]

- Owner: `MarkdownRenderer` the `MarkdownRow`-to-Avalonia-visual materialization; `MarkdownStyling` the one resolved context — font chain, math faces, ink, the `SkinSlot` brush table, the `SkinMetric` scale table, the per-kind `CalloutTint` table, the grammar registry, and the code-pane policy; `CalloutPaint` the kind-to-paint correspondence; `MarkdownRendered` the monoidal render product carrying blocks, the link-hit table, the outline, the minted media rows, the refusals, and the fence sessions it owns; `MarkdownGrid` the retained-table projection onto the `Editing/tables` column rows with its span verdict; `MediaCodecRow` the extension-to-codec admission; `RunDecoration` the inline decoration capability; `MathStyle`/`MathBox`/`MathFaces`/`MathTypeset`/`MathRun`/`MathInlineVisual` the TeX-subset typesetting owner; `ContentFault` the direct generated `[Union]` with one `[FaultCase]` leaf per content failure.
- Cases: `ContentFault` = UnresolvedRole | CodecAbsent | DecodeFailed | GrammarAbsent; `MathStyle` = Inline | Display; `SkinSlot` = text · muted · link · surface · border · code-surface · quote-bar · rule-ink; `SkinMetric` = radius · gutter · gap; `RunDecoration` = strike · underline; `MediaCodecRow` = raster · vector · video · audio.
- Entry: `public static MarkdownRendered Render(MarkdownDocumentRows rows, MarkdownStyling styling)` — materializes every one of the eleven `MarkdownRow` arms into one block sequence plus the span-keyed `LinkHit` table, the `MarkdownAnchor` outline, the `MediaSurface` rows the document's media links minted, and the fence sessions the render owns; `public static Seq<MarkdownAnchor> Anchors(MarkdownDocumentRows rows)` — the outline-only projection that materializes nothing and mounts nothing; `public static Fin<string> Scope(RasmRegistry grammar, string language)` — the registry-backed fence grammar lookup; `public static MarkdownGrid Project(MarkdownRow.Grid grid)` — the retained-table projection; `public static Fin<MarkdownStyling> Of(...)` on `MarkdownStyling` — the one accumulating resolve.
- Auto: the markdown AST projection is owned by `Theme/typography` (`MarkdownProjection`, the closed eleven-arm fold to `MarkdownRow`/`InlineRun`) — this renderer consumes those rows and never re-parses. Each `InlineRun` materializes the landed content vocabulary: `InlineContent` = Text | Code | Math | Break | Task | Opaque dispatches through the generated total `Switch`, the `CapabilitySet<InlineStyle>` grants fold to decorations and wrappers, and `LinkTarget` discriminates the hit-table hyperlink from the inline image. Block arms materialize against the SLOT TABLES rather than against an authored literal: a callout resolves its `CalloutPaint` row from the projection's own `CalloutKind` and paints tint, edge, ink, and icon from that row's `PaintRole`s; a quote paints its bar in the separator ink over the panel surface; a list prints the marker its own `ListGrammar` case carries; a rule is a one-metric separator; a definition list pairs a strong term with an indented body; a fence recesses onto the well surface and mounts the code pane under the scope the registry answered, falling back to a plain mono `SelectableTextBlock` reporting the absent grammar by name; a grid projects onto `Editing/tables#GRID_SUBSTRATE` `TableColumnRow` values and BINDS them. Mathematics typesets through `MathTypeset` — one painter serves the measure and the draw, so a run typesets once, `MathStyle` selects script sizing, box anchor, and the alignment the aligned draw centres on, and the engine's `Result`-shaped parse path lands a malformed source as `ContentFault.DecodeFailed` carrying the engine's own message. The engine shapes on its own vendored glyph engine, so `MathFaces` reads each `FontChain` family once into a `Typography.OpenFont.Typeface` and the admitted set rides `Painter.LocalTypefaces`. The round-trip `SourceSpan` maps each retained block and run to its source range; each `Heading.Anchor` retains as a `MarkdownAnchor` whose depth IS the role's own `Heading` level, so the outline is the heading tree and an anchor jump is a settled `Document/search#HIGHLIGHT_NAV` `SearchOpen.ProsePane` request rather than a second link grammar.
- Packages: Markdig, Avalonia, Avalonia.AvaloniaEdit, Avalonia.Skia, AsyncImageLoader.Avalonia, CSharpMath.SkiaSharp, SkiaSharp, Rasm (project — `FaultBand`, `[FaultCase]`, `Fault`, `CapabilitySet`, `ColumnTrait`, `Cell`, `Op`, `PerceptualColor` and its gamut egress), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new `InlineContent` case is one content arm the generated dispatch breaks at compile time; a new `InlineStyle` grant is one `RunDecoration` row; a new `MarkdownRow` case breaks BOTH the row dispatch and the anchor walk at compile time; a new callout kind is one `CalloutKind` row at the projection owner and one `CalloutPaint` row here, and a kind carrying neither refuses the whole resolve by name; a new painted slot is one `SkinSlot` row; a new embeddable media type is one `MediaCodecRow` extension row; a new math modality is one `MathStyle` row and no typesetting surface.
- Boundary: the renderer materializes all eleven `MarkdownRow` cases and an empty projection is a defect rather than a routing verdict — `Opaque` alone renders as its retained node evidence under the muted ink, because raw HTML has no admitted materialization and rendering nothing would hide that a document carried it. THE STYLING RESOLVE ACCUMULATES: eight brush slots, three metrics, and every callout tint are INDEPENDENT columns, so the resolve traverses each table into `Validation` and reports every missing role at once — a `from`-chained resolve names the first defect and hides the rest, which is a theme audit that has to run eleven times. FENCE grammar is a REGISTRY LOOKUP, never a three-scope closure: `RasmRegistry.Scope` consults the product rows, then the corpus language id, then the corpus extension, so a fenced language the corpus carries highlights and one it does not reports `ContentFault.GrammarAbsent` by name on a still-readable mono block; a fence arm rendering nothing and a page-local grammar table are the deleted forms. GRIDS project onto the settled column rows AND BIND THEM — `AutoGenerateColumns` is false, so a projection whose column rows never reached the `DataGrid` rendered a table with no columns at all — and the SPAN VERDICT is stated rather than silent: `DataGrid` exposes no cell-span surface, so a `GridCell` whose `ColumnSpan` or `RowSpan` exceeds one renders its runs in its origin column while the covered columns render empty, and the projection carries a `SpanVerdict` counting exactly those cells so a merged-cell document reads as flattened rather than as correct. MEDIA LINKS mint through the ONE admitted-extension table — an image link whose extension no `MediaCodecRow` claims is `ContentFault.CodecAbsent` rather than a broken control, and a raster link materializes through the SAME shared `ImageLoader.AsyncImageLoader` cache the `[03]` image codec row rides. ANCHORS are consumed, not dropped: the outline is the retained anchor tree, its depth is `TypographyRole.Heading` — the inverse of the projection's own `ForHeading` map, so a second depth ladder cannot disagree with it — and cross-document anchor navigation rides `SearchOpen.ProsePane`, so this page mints no second deep-link vocabulary. A consumer wanting ONLY that tree takes `Anchors`, because a render materializes a control per block and opens a live `CodeSession` per fence — a cost an outline pays for nothing and a lifetime an outline caller has nowhere to release. A RENDER therefore OWNS what it mounted: the fence sessions ride the produced value as TYPED sessions and a surface disposes the previous render before seating the next, so a theme swap, a locale flip, or one keystroke of an edited document cannot leak a grammar installation per fence per pass. Math draws through the settled in-tree vehicle — one `ICustomDrawOperation` folding `ISkiaSharpApiLeaseFeature.Lease()` to `DrawSource.Borrowed` — so an equation composites into the host's in-flight frame and mints no `SKSurface`; a per-equation offscreen surface, a private `SKPaint`/`SKFont` math path, a hand-rolled TeX box model, a `try`/`catch` around the source assignment, and a literal font size are the deleted forms. Alignment is the engine's own centring axis and BOTH trailing floats of the aligned draw are offsets, so the display arm centres through `MathStyle.Alignment` while the retained bounds ride the offsets. `SKTypeface`/`SKFontManager` reach the engine only through `MathFaces`. A `Markdig` re-parse, a silent catch-all, and a retired flat-column `InlineRun` read are rejected.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MathStyle {
    public static readonly MathStyle Inline = new("inline", line: LineStyle.Text, role: TypographyRole.Body, alignment: TextAlignment.Left);
    public static readonly MathStyle Display = new("display", line: LineStyle.Display, role: TypographyRole.Body, alignment: TextAlignment.Center);

    public LineStyle Line { get; }

    public TypographyRole Role { get; }

    public TextAlignment Alignment { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SkinSlot {
    public static readonly SkinSlot Text = new("text", PaintRole.Text, rung: 0);
    public static readonly SkinSlot Muted = new("muted", PaintRole.TextMuted, rung: 0);
    public static readonly SkinSlot Link = new("link", PaintRole.Link, rung: 0);
    public static readonly SkinSlot Surface = new("surface", PaintRole.Panel, rung: 0);
    public static readonly SkinSlot Border = new("border", PaintRole.Border, rung: 0);
    public static readonly SkinSlot CodeSurface = new("code-surface", PaintRole.Well, rung: 0);
    public static readonly SkinSlot QuoteBar = new("quote-bar", PaintRole.Separator, rung: 0);
    public static readonly SkinSlot RuleInk = new("rule-ink", PaintRole.Separator, rung: 0);

    public PaintRole Role { get; }

    public int Rung { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SkinMetric {
    public static readonly SkinMetric Radius = new("radius", MetricFamily.Radius, step: 2);
    public static readonly SkinMetric Gutter = new("gutter", MetricFamily.Space, step: 3);
    public static readonly SkinMetric Gap = new("gap", MetricFamily.Space, step: 2);

    public MetricFamily Family { get; }

    public int Step { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CalloutPaint {
    public static readonly CalloutPaint Note = new(CalloutKind.Note.Key, PaintRole.Info, AssetKey.Create("Note"));
    public static readonly CalloutPaint Tip = new(CalloutKind.Tip.Key, PaintRole.Success, AssetKey.Create("Tip"));
    public static readonly CalloutPaint Important = new(CalloutKind.Important.Key, PaintRole.Accent, AssetKey.Create("Important"));
    public static readonly CalloutPaint Warning = new(CalloutKind.Warning.Key, PaintRole.Warning, AssetKey.Create("Warning"));
    public static readonly CalloutPaint Caution = new(CalloutKind.Caution.Key, PaintRole.Error, AssetKey.Create("Caution"));

    public PaintRole Status { get; }

    public AssetKey Icon { get; }

    public static Fin<CalloutPaint> Of(CalloutKind kind) =>
        TryGet(kind.Key, out CalloutPaint? row)
            ? Fin.Succ(row!)
            : Fin.Fail<CalloutPaint>(new ContentFault.UnresolvedRole($"markdown/callout: {kind.Key} carries no paint row"));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RunDecoration : ICapability<RunDecoration> {
    public static readonly RunDecoration Strike = new("strike", static () => TextDecorations.Strikethrough);
    public static readonly RunDecoration Underline = new("underline", static () => TextDecorations.Underline);

    [UseDelegateFromConstructor]
    public partial TextDecorationCollection Decoration();

    public static CapabilitySet<RunDecoration> Of(CapabilitySet<InlineStyle> styles, Option<LinkTarget> link) =>
        Seq((styles.Admits(InlineStyle.Strike), Strike),
            (link.Exists(static target => target is LinkTarget.Hyperlink), Underline))
            .Fold(CapabilitySet<RunDecoration>.None,
                static (held, row) => row.Item1 ? held.With(row.Item2) : held);

    public static TextDecorationCollection? Fold(CapabilitySet<RunDecoration> held) =>
        held.Held.Count is 0
            ? null
            : toSeq(Items).Filter(held.Admits).Fold(new TextDecorationCollection(), static (collection, row) => {
                collection.AddRange(row.Decoration());
                return collection;
            });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MediaCodecRow {
    static readonly Op Admitting = Op.Of(name: "appui.media.admit");

    public static readonly MediaCodecRow Raster = new(
        "image", visual: true, timed: false,
        Seq(".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp"),
        static (key, source) => Fin.Succ<MediaSurface>(new MediaSurface.Image(key, source, Stretch.Uniform)));
    public static readonly MediaCodecRow Vector = new(
        "svg", visual: true, timed: false,
        Seq(".svg"),
        static (key, source) => Admitting.AcceptValidated<AssetKey, ValidationError>(source)
            .Map<MediaSurface>(asset => new MediaSurface.Svg(key, source, asset))
            .MapFail(_ => (Error)new ContentFault.CodecAbsent($"media/vector: {source} is not an admitted asset")));
    public static readonly MediaCodecRow Video = new(
        "video", visual: true, timed: true,
        Seq(".mp4", ".mkv", ".webm", ".mov", ".m4v"),
        static (key, source) => Fin.Succ<MediaSurface>(new MediaSurface.Video(key, source, PlaybackPolicy.Embedded)));
    public static readonly MediaCodecRow Audio = new(
        "audio", visual: false, timed: true,
        Seq(".mp3", ".wav", ".flac", ".m4a", ".ogg", ".opus"),
        static (key, source) => Fin.Succ<MediaSurface>(new MediaSurface.Audio(key, source, PlaybackPolicy.Embedded)));

    public bool Visual { get; }

    public bool Timed { get; }

    public Seq<string> Extensions { get; }

    [UseDelegateFromConstructor]
    public partial Fin<MediaSurface> Mint(string key, string source);

    public static Fin<MediaSurface> Admit(string key, string destination) =>
        toSeq(Items).Find(row => row.Extensions.Exists(extension =>
                destination.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
            .ToFin(new ContentFault.CodecAbsent($"media/extension: {destination}"))
            .Bind(row => row.Mint(key, destination));
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct LinkHit(SourceSpan Span, string Url);

public readonly record struct MarkdownAnchor(Option<string> Anchor, TypographyRole Role, string Text, SourceSpan Span) {
    static readonly int Deepest = toSeq(TypographyRole.Items).Choose(static role => role.Heading).Fold(0, Math.Max);

    public int Depth => Role.Heading.IfNone(Deepest);

    public Option<SearchOpen> Open(string documentKey) =>
        Anchor.Map(_ => (SearchOpen)new SearchOpen.ProsePane(documentKey, Span));
}

public readonly record struct SpanVerdict(int Cells, int Flattened) {
    public bool Faithful => Flattened == 0;
}

public readonly record struct MarkdownCells(int Ordinal, Seq<string> Values) {
    public string At(int column) => column >= 0 && column < Values.Count ? Values[column] : string.Empty;
}

public sealed record MarkdownGrid(Seq<TableColumnRow<MarkdownCells>> Columns, Seq<MarkdownCells> Rows, SpanVerdict Verdict);

public sealed record CalloutTint(IBrush Fill, IBrush Edge, IBrush Ink, Option<IImage> Icon) {
    public static Validation<Error, CalloutTint> Of(ResolvedTheme theme, AssetRuntime assets, CalloutPaint row) =>
        (MarkdownStyling.Ink(theme, row.Status, rung: 1),
         MarkdownStyling.Ink(theme, row.Status),
         MarkdownStyling.Ink(theme, PaintRole.Text))
            .Apply((fill, edge, ink) => new CalloutTint(fill, edge, ink,
                IconSurface.Resolve(assets, new AssetRequest(row.Icon, Step: 3, Scale: 1d, FlowDirection.LeftToRight, new GlyphForm.Image()), theme)
                    .Bind(static product => product.Image)
                    .Match(Succ: Some, Fail: static _ => Option<IImage>.None)))
            .As();
}

public readonly record struct MarkdownStyling(
    FontChain Chain,
    Seq<Typeface> MathFaces,
    PerceptualColor Ink,
    FrozenDictionary<SkinSlot, IBrush> Inks,
    FrozenDictionary<SkinMetric, double> Metrics,
    FrozenDictionary<CalloutKind, CalloutTint> Callouts,
    RasmRegistry Grammar,
    ResolvedTheme Theme,
    TableChrome Chrome,
    CodePane Fence) {
    public static Fin<MarkdownStyling> Of(
        ResolvedTheme theme, AssetRuntime assets, FontChain chain, Seq<Typeface> faces,
        PerceptualColor ink, RasmRegistry grammar, TableChrome chrome, CodePane fence) =>
        (toSeq(SkinSlot.Items).Traverse(slot => Ink(theme, slot.Role, slot.Rung).Map(brush => (slot, brush))).As(),
         toSeq(SkinMetric.Items).Traverse(row => theme.Metric(row.Family, row.Step)
             .ToValidation<Error>(new ContentFault.UnresolvedRole($"markdown/metric: {row.Key}"))
             .Map(step => (row, step))).As(),
         toSeq(CalloutKind.Items).Traverse(kind => CalloutPaint.Of(kind).ToValidation()
             .Bind(paint => CalloutTint.Of(theme, assets, paint))
             .Map(tint => (kind, tint))).As())
            .Apply((inks, metrics, callouts) => new MarkdownStyling(
                chain, faces, ink,
                inks.ToFrozenDictionary(static pair => pair.slot, static pair => pair.brush),
                metrics.ToFrozenDictionary(static pair => pair.row, static pair => pair.step),
                callouts.ToFrozenDictionary(static pair => pair.kind, static pair => pair.tint),
                grammar, theme, chrome, fence))
            .As()
            .ToFin();

    public IBrush Paint(SkinSlot slot) => Inks[slot];

    public double Step(SkinMetric metric) => Metrics[metric];

    public CalloutTint Tint(CalloutKind kind) => Callouts[kind];

    internal static Validation<Error, IBrush> Ink(ResolvedTheme theme, PaintRole role, int rung = 0) =>
        theme.Paint(role, rung).Map(static colour => (IBrush)new SolidColorBrush(colour))
            .ToValidation<Error>(new ContentFault.UnresolvedRole($"markdown/skin: {role.Key}"));
}

public sealed record MarkdownRendered(
    Seq<Control> Blocks,
    Seq<LinkHit> Links,
    Seq<MarkdownAnchor> Outline,
    Seq<MediaSurface> Media,
    Seq<Error> Refusals,
    Seq<CodeSession> Mounts) : Monoid<MarkdownRendered>, IDisposable {
    public static MarkdownRendered Empty { get; } = new([], [], [], [], [], []);

    public MarkdownRendered Combine(MarkdownRendered other) =>
        new(Blocks + other.Blocks, Links + other.Links, Outline + other.Outline,
            Media + other.Media, Refusals + other.Refusals, Mounts + other.Mounts);

    public void Dispose() => Mounts.Iter(static mount => mount.Dispose());
}

public readonly record struct MathBox(float Width, float Height, float Ascent);

// --- [ERRORS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Content;
    private ContentFault(string detail) { Detail = detail; }

    public string Detail { get; }

    public override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record UnresolvedRole(string Detail) : ContentFault(Detail);
    [FaultCase(1)]
    public sealed partial record CodecAbsent(string Detail)    : ContentFault(Detail);
    [FaultCase(2)]
    public sealed partial record DecodeFailed(string Detail)   : ContentFault(Detail);
    [FaultCase(3)]
    public sealed partial record GrammarAbsent(string Detail)  : ContentFault(Detail);

}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class MathFaces {
    static readonly Atom<HashMap<string, Seq<Typeface>>> Loaded = Atom(HashMap<string, Seq<Typeface>>());

    public static Seq<Typeface> Of(FontChain chain, SKFontManager manager) =>
        Loaded.Value.Find(chain.Key).Match(
            Some: static held => held,
            None: () => Cell.Claim(Loaded, chain.Key, () => Read(chain, manager)) switch {
                var settled => settled.Current.Find(chain.Key).IfNone(Seq<Typeface>()),
            });

    static Seq<Typeface> Read(FontChain chain, SKFontManager manager) =>
        toSeq((chain.Sans + chain.Mono + Seq(chain.Symbols)).Distinct())
            .Choose(family => Optional(manager.MatchFamily(family)))
            .Choose(Face);

    static Option<Typeface> Face(SKTypeface resolved) {
        using SKTypeface face = resolved;
        using SKStreamAsset asset = face.OpenStream();
        using SKData data = SKData.Create(asset);
        using Stream managed = data.AsStream();
        return Optional(new OpenFontReader().Read(managed));
    }
}

public static class MathTypeset {
    public static Fin<MathBox> Measure(string source, MathStyle style, MarkdownStyling styling, float width) =>
        Admit(source, style, styling).Map(painter => Boxed(painter, width));

    public static Fin<MathBox> Draw(
        SKCanvas canvas, string source, MathStyle style, MarkdownStyling styling, float offsetX, float offsetY, float width) =>
        Admit(source, style, styling).Map(painter => {
            painter.Draw(canvas, style.Alignment, default, offsetX, offsetY);
            return Boxed(painter, width);
        });

    static Fin<MathPainter> Admit(string source, MathStyle style, MarkdownStyling styling) {
        TextStyleRow row = TypeScale.Resolve(style.Role, styling.Chain);
        MathPainter painter = new() {
            FontSize = (float)row.Size,
            LineStyle = style.Line,
            TextColor = styling.Ink.ToRgb() switch { var (red, green, blue, alpha) => new SKColor(red, green, blue, alpha) },
            AntiAlias = true,
            LocalTypefaces = styling.MathFaces,
            LaTeX = source,
        };
        return painter.ErrorMessage is { } message
            ? Fin.Fail<MathPainter>(new ContentFault.DecodeFailed($"math/latex: {message}"))
            : Fin.Succ(painter);
    }

    static MathBox Boxed(MathPainter painter, float width) =>
        painter.Measure(width) switch { var bounds => new MathBox(bounds.Width, bounds.Height, -bounds.Top) };
}

public sealed record MathRun(string Source, MathStyle Style, MarkdownStyling Styling, Rect Bounds) : ICustomDrawOperation {
    public bool Equals(ICustomDrawOperation? other) => Equals(other as MathRun);

    public bool HitTest(Point point) => Bounds.Contains(point);

    public void Render(ImmediateDrawingContext context) =>
        Optional(context.TryGetFeature<ISkiaSharpApiLeaseFeature>())
            .Map(static feature => new DrawSource.Borrowed(feature.Lease()))
            .Iter(borrowed => {
                using ISkiaSharpApiLease lease = borrowed.Lease;
                ignore(MathTypeset.Draw(
                    lease.SkCanvas, Source, Style, Styling, (float)Bounds.X, (float)Bounds.Y, (float)Bounds.Width));
            });

    public void Dispose() { }
}

public sealed class MathInlineVisual(string source, MathStyle style, MarkdownStyling styling) : Control {
    public override void Render(DrawingContext context) =>
        context.Custom(new MathRun(source, style, styling, new Rect(Bounds.Size)));

    protected override Size MeasureOverride(Size available) =>
        MathTypeset.Measure(source, style, styling, (float)available.Width)
            .Match(Succ: box => new Size(box.Width, box.Height), Fail: static _ => default);
}
```

```csharp
public static class MarkdownRenderer {
    public static MarkdownRendered Render(MarkdownDocumentRows rows, MarkdownStyling styling) =>
        rows.Body.Fold(MarkdownRendered.Empty, (acc, row) => Block(row, styling, acc));

    public static Seq<MarkdownAnchor> Anchors(MarkdownDocumentRows rows) =>
        rows.Body.Bind(Anchored);

    static Seq<MarkdownAnchor> Anchored(MarkdownRow row) => row.Switch(
        heading: static h => Seq(new MarkdownAnchor(h.Anchor, h.Role, Flat(h.Runs), h.Span)),
        paragraph: static _ => Seq<MarkdownAnchor>(),
        quote: static q => q.Children.Bind(Anchored),
        callout: static c => c.Children.Bind(Anchored),
        listRows: static l => l.Items.Bind(static item => item.Bind(Anchored)),
        definitions: static d => d.Items.Bind(static entry => entry.Body.Bind(Anchored)),
        grid: static _ => Seq<MarkdownAnchor>(),
        codeFence: static _ => Seq<MarkdownAnchor>(),
        math: static _ => Seq<MarkdownAnchor>(),
        rule: static _ => Seq<MarkdownAnchor>(),
        opaque: static _ => Seq<MarkdownAnchor>());

    public static Fin<string> Scope(RasmRegistry grammar, string language) =>
        string.IsNullOrWhiteSpace(language)
            ? Fin.Fail<string>(new ContentFault.GrammarAbsent("markdown/fence: no language declared"))
            : grammar.Scope(language)
                .MapFail(_ => (Error)new ContentFault.GrammarAbsent($"markdown/fence: {language}"));

    public static MarkdownGrid Project(MarkdownRow.Grid grid) {
        (int Width, int Cells, int Flattened) walked = grid.Rows.Fold(
            (Width: 0, Cells: 0, Flattened: 0),
            static (held, row) => row.Cells.Fold(held, static (inner, cell) => (
                Math.Max(inner.Width, cell.ColumnIndex + cell.ColumnSpan),
                inner.Cells + 1,
                inner.Flattened + (cell.ColumnSpan > 1 || cell.RowSpan > 1 ? 1 : 0))));
        HashMap<int, string> headers = grid.Rows.Find(static row => row.Band == GridBand.Header)
            .Map(row => toHashMap(Seated(row, walked.Width).Map(static (runs, column) => (column, Flat(runs)))))
            .IfNone(HashMap<int, string>());
        Seq<MarkdownCells> body = grid.Rows.Filter(static row => row.Band == GridBand.Body)
            .Map((row, ordinal) => new MarkdownCells(ordinal, Seated(row, walked.Width).Map(Flat)));
        return new MarkdownGrid(
            toSeq(Enumerable.Range(0, walked.Width)).Map(column => new TableColumnRow<MarkdownCells>(
                Key: AggregateColumn.Create($"grid.{column.ToString(CultureInfo.InvariantCulture)}"),
                Header: headers.Find(column).IfNone(string.Empty),
                Kind: TableCellKind.Text,
                Access: new TableColumnAccess<MarkdownCells>.Plain(
                    Cell: Some<BindingBase>(new Binding($"{nameof(MarkdownCells.Values)}[{column}]")),
                    Export: cells => cells.At(column)),
                Width: new DataGridLength(1d, DataGridLengthUnitType.Star),
                Traits: CapabilitySet<ColumnTrait>.None)),
            body,
            new SpanVerdict(walked.Cells, walked.Flattened));
    }

    static Seq<Seq<InlineRun>> Seated(GridRow row, int width) =>
        row.Cells.Fold(
            HashMap<int, Seq<InlineRun>>(),
            static (seated, cell) => cell.ColumnIndex >= 0 ? seated.AddOrUpdate(cell.ColumnIndex, cell.Runs) : seated)
        switch {
            var occupied => toSeq(Enumerable.Range(0, width)).Map(column => occupied.Find(column).IfNone(Seq<InlineRun>())),
        };

    public static string Flat(Seq<InlineRun> runs) =>
        string.Concat(runs.Map(static run => run.Content.Switch(
            text: static t => t.Value,
            code: static c => c.Value,
            math: static m => m.Value,
            @break: static b => b.Strength.Equals(BreakStrength.Mandatory) ? "\n" : " ",
            task: static t => t.State.Equals(TaskState.Done) ? "☑" : "☐",
            opaque: static _ => string.Empty)));

    static MarkdownRendered Block(MarkdownRow row, MarkdownStyling styling, MarkdownRendered acc) => row.Switch(
        state: (Styling: styling, Acc: acc),
        heading: static (s, h) => Anchored(Styled(h.Runs, h.Role, s.Styling, s.Acc), h),
        paragraph: static (s, p) => Styled(p.Runs, TypographyRole.Body, s.Styling, s.Acc),
        quote: static (s, q) => Nested(q.Children, s.Styling, s.Acc, static (styling, children) =>
            Bordered(children, styling.Paint(SkinSlot.QuoteBar), styling, BorderEdge.Leading)),
        callout: static (s, c) => Nested(c.Children, s.Styling, s.Acc, (styling, children) =>
            Called(c.Kind, children, styling)),
        listRows: static (s, l) => l.Items.Fold(s.Acc, (state, item) =>
            Nested(item, s.Styling, state, (styling, children) => Bulleted(l.Grammar, children, styling))),
        definitions: static (s, d) => d.Items.Fold(s.Acc, (state, item) =>
            Nested(item.Body, s.Styling, Styled(item.Term, TypographyRole.Label, s.Styling, state),
                static (styling, children) => Bordered(children, styling.Paint(SkinSlot.Border), styling, BorderEdge.Leading))),
        grid: static (s, g) => Gridded(g, s.Styling, s.Acc),
        codeFence: static (s, f) => Fenced(f, s.Styling, s.Acc),
        math: static (s, m) => s.Acc with {
            Blocks = s.Acc.Blocks.Add(Padded(new MathInlineVisual(m.Source, MathStyle.Display, s.Styling), s.Styling)),
        },
        rule: static (s, _) => s.Acc with {
            Blocks = s.Acc.Blocks.Add(new Border {
                Height = 1d, Background = s.Styling.Paint(SkinSlot.RuleInk),
                Margin = Gapped(s.Styling),
            }),
        },
        opaque: static (s, o) => s.Acc with {
            Blocks = s.Acc.Blocks.Add(new SelectableTextBlock {
                Text = o.Node, Foreground = s.Styling.Paint(SkinSlot.Muted),
                FontFamily = new FontFamily(TypeScale.Resolve(TypographyRole.Code, s.Styling.Chain).Family),
            }),
        });

    static MarkdownRendered Nested(
        Seq<MarkdownRow> children, MarkdownStyling styling, MarkdownRendered acc,
        Func<MarkdownStyling, Seq<Control>, Control> chrome) =>
        children.Fold(MarkdownRendered.Empty, (state, child) => Block(child, styling, state)) switch {
            var inner => acc.Combine(inner with { Blocks = Seq(chrome(styling, inner.Blocks)) }),
        };

    static MarkdownRendered Anchored(MarkdownRendered acc, MarkdownRow.Heading heading) =>
        acc with {
            Outline = acc.Outline.Add(new MarkdownAnchor(heading.Anchor, heading.Role, Flat(heading.Runs), heading.Span)),
        };

    static MarkdownRendered Fenced(MarkdownRow.CodeFence fence, MarkdownStyling styling, MarkdownRendered acc) =>
        Scope(styling.Grammar, fence.Language)
            .Bind(_ => Seated(fence, styling))
            .Match(
                Succ: seated => acc with {
                    Blocks = acc.Blocks.Add(Recessed(seated.Editor, styling)),
                    Mounts = acc.Mounts.Add(seated.Session),
                },
                Fail: error => Plain(fence, styling, acc, error));

    static Fin<(TextEditor Editor, CodeSession Session)> Seated(MarkdownRow.CodeFence fence, MarkdownStyling styling) =>
        Custody.Bracket(
            () => new TextEditor {
                Document = new TextDocument(fence.Source),
                Background = styling.Paint(SkinSlot.CodeSurface),
            },
            editor => styling.Fence
                .Open(editor, styling.Grammar, fence.Language, styling.Theme, static _ => Seq<TextSegment>())
                .Map(session => (Editor: editor, Session: session)),
            Mounting);

    static readonly Op Mounting = Op.Of(name: "appui.markdown.fence");

    static MarkdownRendered Plain(MarkdownRow.CodeFence fence, MarkdownStyling styling, MarkdownRendered acc, Error refusal) =>
        acc with {
            Blocks = acc.Blocks.Add(Recessed(new SelectableTextBlock {
                Text = fence.Source, Foreground = styling.Paint(SkinSlot.Text),
                FontFamily = new FontFamily(TypeScale.Resolve(TypographyRole.Code, styling.Chain).Family),
            }, styling)),
            Refusals = acc.Refusals.Add(refusal),
        };

    static MarkdownRendered Gridded(MarkdownRow.Grid grid, MarkdownStyling styling, MarkdownRendered acc) =>
        Project(grid) switch {
            var projected => projected.Columns
                .Traverse(row => row.Column(styling.Chrome)).As()
                .Map(static seated => seated.Somes())
                .Match(
                    Succ: columns => acc with { Blocks = acc.Blocks.Add(Tabled(projected, columns, styling)) },
                    Fail: error => (acc with { Blocks = acc.Blocks.Add(Tabled(projected, Seq<DataGridColumn>(), styling)) })
                        with { Refusals = acc.Refusals.Add(error) }),
        };

    static Control Tabled(MarkdownGrid projected, Seq<DataGridColumn> columns, MarkdownStyling styling) =>
        new DataGrid {
            ItemsSource = projected.Rows.ToArray(),
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Background = styling.Paint(SkinSlot.Surface),
            BorderBrush = styling.Paint(SkinSlot.Border),
            BorderThickness = new Thickness(1d),
            Columns = { [.. columns] },
        };

    static MarkdownRendered Styled(Seq<InlineRun> runs, TypographyRole role, MarkdownStyling styling, MarkdownRendered acc) =>
        runs.Fold(
            (Harvest: acc, Inlines: Seq<Inline>()),
            (state, run) => Run(run, role, styling, state))
        switch {
            var folded => folded.Harvest with {
                Blocks = folded.Harvest.Blocks.Add(new SelectableTextBlock {
                    Inlines = [.. folded.Inlines],
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = styling.Paint(SkinSlot.Text),
                }),
            },
        };

    static (MarkdownRendered Harvest, Seq<Inline> Inlines) Run(
        InlineRun run, TypographyRole role, MarkdownStyling styling,
        (MarkdownRendered Harvest, Seq<Inline> Inlines) state) {
        TextStyleRow style = TypeScale.Resolve(run.Content is InlineContent.Code ? TypographyRole.Code : role, styling.Chain);
        CapabilitySet<RunDecoration> decor = RunDecoration.Of(run.Styles, run.Link);
        bool linked = decor.Admits(RunDecoration.Underline);
        Inline inline = run.Content.Switch<(TextStyleRow Style, CapabilitySet<RunDecoration> Decor, bool Linked, MarkdownStyling Styling), Inline>(
            state: (style, decor, linked, styling),
            text: static (s, t) => Dressed(t.Value, s.Style, s.Decor, s.Linked, s.Styling),
            code: static (s, c) => Dressed(c.Value, s.Style, s.Decor, s.Linked, s.Styling),
            math: static (s, m) => new InlineUIContainer(new MathInlineVisual(m.Value, MathStyle.Inline, s.Styling)),
            @break: static (_, b) => b.Strength.Equals(BreakStrength.Mandatory) ? new LineBreak() : new Run(" "),
            task: static (s, t) => Dressed(t.State.Equals(TaskState.Done) ? "☑ " : "☐ ", s.Style, s.Decor, s.Linked, s.Styling),
            opaque: static (s, _) => Dressed(string.Empty, s.Style, s.Decor, s.Linked, s.Styling));
        inline = run.Styles.Admits(InlineStyle.Strong) ? new Bold { Inlines = { inline } } : inline;
        inline = run.Styles.Admits(InlineStyle.Emphasis) ? new Italic { Inlines = { inline } } : inline;
        return run.Link.Match(
            Some: target => target.Switch(
                state: (State: state, Inline: inline, run.Span),
                hyperlink: static (s, link) => (
                    s.State.Harvest with { Links = s.State.Harvest.Links.Add(new LinkHit(s.Span, link.Destination)) },
                    s.State.Inlines.Add(s.Inline)),
                image: static (s, image) => (MediaCodecRow.Admit($"link@{s.Span.Start}", image.Destination).Match(
                        Succ: surface => s.State.Harvest with { Media = s.State.Harvest.Media.Add(surface) },
                        Fail: error => s.State.Harvest with {
                            Refusals = s.State.Harvest.Refusals.Add(error),
                        }),
                    s.State.Inlines)),
            None: () => (state.Harvest, state.Inlines.Add(inline)));
    }

    static Inline Dressed(
        string text, TextStyleRow style, CapabilitySet<RunDecoration> decor, bool linked, MarkdownStyling styling) =>
        new Run(text) {
            FontFamily = new FontFamily(style.Family), FontSize = style.Size, FontWeight = (FontWeight)style.Weight,
            Foreground = styling.Paint(linked ? SkinSlot.Link : SkinSlot.Text),
            TextDecorations = RunDecoration.Fold(decor),
        };

    // --- [BLOCK_CHROME]

    static Control Called(CalloutKind kind, Seq<Control> children, MarkdownStyling styling) =>
        styling.Tint(kind) switch {
            var tint => new Border {
                Background = tint.Fill, BorderBrush = tint.Edge, BorderThickness = new Thickness(0d, 0d, 0d, 0d),
                CornerRadius = new CornerRadius(styling.Step(SkinMetric.Radius)),
                Padding = new Thickness(styling.Step(SkinMetric.Gutter)),
                Margin = Gapped(styling),
                Child = new DockPanel {
                    Children = {
                        tint.Icon.Match(
                            Some: image => (Control)new Image {
                                Source = image, Width = 16d, Height = 16d,
                                Margin = new Thickness(0d, 0d, styling.Step(SkinMetric.Gap), 0d),
                                [DockPanel.DockProperty] = Dock.Left,
                            },
                            None: static () => (Control)new Panel { Width = 0d, [DockPanel.DockProperty] = Dock.Left }),
                        Stacked(children, styling),
                    },
                },
            },
        };

    static Control Bordered(Seq<Control> children, IBrush edge, MarkdownStyling styling, BorderEdge side) =>
        new Border {
            BorderBrush = edge,
            BorderThickness = side.Thickness,
            Padding = new Thickness(styling.Step(SkinMetric.Gutter), 0d, 0d, 0d),
            Margin = Gapped(styling),
            Child = Stacked(children, styling),
        };

    static Control Bulleted(ListGrammar grammar, Seq<Control> children, MarkdownStyling styling) =>
        new DockPanel {
            Margin = new Thickness(styling.Step(SkinMetric.Gutter), 0d, 0d, 0d),
            Children = {
                new TextBlock {
                    Text = grammar.Switch(
                        ordered: static o => $"{o.Start.ToString(CultureInfo.InvariantCulture)}.",
                        bulleted: static b => b.Mark.ToString()),
                    Foreground = styling.Paint(SkinSlot.Muted),
                    Margin = new Thickness(0d, 0d, styling.Step(SkinMetric.Gap), 0d),
                    [DockPanel.DockProperty] = Dock.Left,
                },
                Stacked(children, styling),
            },
        };

    static Control Recessed(Control content, MarkdownStyling styling) =>
        new Border {
            Background = styling.Paint(SkinSlot.CodeSurface), BorderBrush = styling.Paint(SkinSlot.Border),
            BorderThickness = new Thickness(1d),
            CornerRadius = new CornerRadius(styling.Step(SkinMetric.Radius)),
            Padding = new Thickness(styling.Step(SkinMetric.Gutter)),
            Margin = Gapped(styling), Child = content,
        };

    static Control Padded(Control content, MarkdownStyling styling) =>
        new Border { Padding = Gapped(styling), Child = content };

    static Control Stacked(Seq<Control> children, MarkdownStyling styling) =>
        new StackPanel { Spacing = styling.Step(SkinMetric.Gap), Children = { [.. children] } };

    static Thickness Gapped(MarkdownStyling styling) =>
        new(0d, styling.Step(SkinMetric.Gap), 0d, styling.Step(SkinMetric.Gap));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BorderEdge {
    public static readonly BorderEdge Leading = new("leading", new Thickness(2d, 0d, 0d, 0d));
    public static readonly BorderEdge Trailing = new("trailing", new Thickness(0d, 0d, 0d, 1d));

    public Thickness Thickness { get; }
}
```

## [03]-[MEDIA_SURFACE]

- Owner: `MediaSurface` the `[Union]` codec-row family keyed on its own `MediaCodecRow`; `PlaybackPolicy` the playback operating envelope over `CapabilitySet<PlaybackTrait>` with `LoopMode` its repeat vocabulary and `RedrivePolicy` its reacquire law; `MediaLease` the control-plus-native lifetime capsule over one kernel `Lease`; `MediaRuntime` the one composition-bound capability set every media arm reads; `AppUiFact.Media` the materialization fact.
- Cases: `MediaSurface` = Image | Svg | Video | Audio, each answering its own codec row; `PlaybackTrait` = auto-play · muted; `LoopMode` = None | File(Option<int>) | Playlist(Option<int>) — the count-bearing repeat vocabulary the catalogued `LoopFile`/`LoopPlaylist`/`AbLoopCount` options carry.
- Entry: `public static IO<Fin<MediaLease>> Materialize(MediaSurface surface, MediaRuntime runtime)` — the ONE codec dispatch: every row's intake completes on the result BEFORE the lease returns, and every native or cached resource releases on failed intake and on lease disposal; `public static Fin<PlaybackPolicy> Create(...)` on `PlaybackPolicy` — the accumulating envelope admission.
- Auto: the `Image` case resolves its bitmap through the shared `IAsyncImageLoader` FIRST and constructs the control only over a resolved bitmap, so a successful materialization means a decoded image rather than an assigned URL — the control's own `Source` then hits the same loader's cache; `FallbackImage`, `IsLoading`, and `CurrentImage` remain host-bindable projections for the gallery's live states. The `Svg` case materializes through the asset runtime's OWN `SvgPipeline` under `SvgPosture.PictureOnly`, so a vector shares the module's retained-document cache, its typeface provider, and its tint election. Video and audio compose `MpvContext` with the OpenGL renderer, the `start` OPTION carries the entry position because `time-pos` does not exist before a load completes, and whether a view is seated at all is the codec row's own `Visual` column rather than a flag the dispatch passes down. A transient source reacquires under the policy's own kernel `RedrivePolicy`, so a stalled network read retries on a declared schedule and a bounded budget rather than on a hand-spaced loop.
- Evidence: `MediaSurfaces.Observed` fires `AppUiFact.Media` directly through the runtime `HookSet`; fault absence means ready and fault presence means failed. The mounted and failed instrument rows contribute inward through `MediaSurfaces.TelemetryRow`.
- Packages: AsyncImageLoader.Avalonia, HanumanInstitute.LibMpv, HanumanInstitute.LibMpv.Avalonia, Avalonia, Rasm (project — `Lease`, `Custody`, `Cell`, `CapabilitySet`, `RedrivePolicy`, `Redrive`, `Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new codec is one `MediaSurface` case with one `Materialize` arm and one `MediaCodecRow` row carrying its extensions, its mint, and its visual and timed postures; a new repeat modality is one `LoopMode` case; a new playback posture is one `PlaybackTrait` row; one media instrument is one `InstrumentSpec` row; zero new surface.
- Boundary: the media vocabulary is the one `MediaSurface` union — a per-surface codec, a second image cache, and a parallel video player are the rejected forms; the materialized control crosses to its host through the ONE `Shell/hosts.md` `Surfaces.Mount` entry composed at the shell edge, whose `SurfacePort` carries mount delegate COLUMNS rather than a mount method, so a media-local `port.Mount(view)` spelling is a phantom. Source intake runs on the `IO` effect BEFORE the control returns for EVERY row, not only the audiovisual pair: a mid-pipeline `.Run()` whose `Fin` is discarded and a successful fact over an unresolved async load are the two deleted forms, because readiness over a 404 is evidence that lies. LEASE RELEASE is the kernel's: the lease holds a kernel `Lease<IDisposable>` in a cell the dispose DRAINS, so the second dispose finds an empty cell and releases nothing — the interlocked flag it replaces was a hand-rolled idempotence beside a hand-rolled release closure — and a failed audiovisual intake rolls back through `Custody.Rollback`, which is the acquire-chain member whose SUCCESS transfers custody into the returned lease, so the dual release spelled once in the success closure and once in the catch is unspellable. The AUDIO row mounts NO video plane: an `MpvView` on the OpenGL renderer with no video track is a zero-area surface pretending to be a control, so its codec row declares `Visual` false, the audio lease carries `Option<Control>.None`, and its chrome is the transport bar alone. The video/audio row is `HanumanInstitute.LibMpv.Avalonia` on the OpenGL render path, so a bundled libmpv native binary and a `NativeControlHost` airspace embedding are the rejected forms (`.api/api-libmpv.md` reject law), the libmpv native provisioning at the app-host distribution layer; the media surface never owns an `SKSurface`; playback control flows through the `MpvContext` the bound `IVideoView` exposes, never a hand-rolled mpv command marshaller; every `MpvContext`/view/overlay disposes through the seated release the lease owns. A vector reached by arbitrary document URI has NO admission: the SVG pipeline is the asset catalogue's retained-document owner, so a product vector is an `AssetKey` and a document-embedded picture is a raster — a second SVG intake beside the asset pipeline is the deleted form. The runtime is ONE capability set: the asset runtime already carries the SVG pipeline, so a second vector column beside it is a knob a reader can reconstruct, and the caption engine rides here rather than as a fourth parallel runtime record.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MediaSurface(string Key, string Source) {
    public sealed record Image(string Key, string Source, Stretch Stretch) : MediaSurface(Key, Source);
    public sealed record Svg(string Key, string Source, AssetKey Asset) : MediaSurface(Key, Source);
    public sealed record Video(string Key, string Source, PlaybackPolicy Playback) : MediaSurface(Key, Source);
    public sealed record Audio(string Key, string Source, PlaybackPolicy Playback) : MediaSurface(Key, Source);

    public MediaCodecRow Codec => Switch(
        image: static _ => MediaCodecRow.Raster, svg: static _ => MediaCodecRow.Vector,
        video: static _ => MediaCodecRow.Video, audio: static _ => MediaCodecRow.Audio);

    public string Kind => Codec.Key;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaybackTrait : ICapability<PlaybackTrait> {
    public static readonly PlaybackTrait AutoPlay = new("auto-play");
    public static readonly PlaybackTrait Muted = new("muted");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoopMode {
    private LoopMode() { }
    public sealed record None : LoopMode;
    public sealed record File(Option<int> Repeats) : LoopMode;
    public sealed record Playlist(Option<int> Repeats) : LoopMode;

    public static string Token(Option<int> repeats) =>
        repeats.Match(Some: static count => count.ToString(CultureInfo.InvariantCulture), None: static () => "inf");
}

// --- [MODELS] --------------------------------------------------------------------------

[ComplexValueObject]
public sealed partial class PlaybackPolicy {
    public CapabilitySet<PlaybackTrait> Traits { get; }
    public LoopMode Loop { get; }
    public double Rate { get; }
    public Option<double> Start { get; }
    public Option<double> Stop { get; }
    public Option<int> SectionRepeats { get; }
    public RedrivePolicy Redrive { get; }

    public static PlaybackPolicy Embedded { get; } =
        Create(CapabilitySet<PlaybackTrait>.None, new LoopMode.None(), rate: 1d, None, None, None, RedrivePolicy.None);

    public bool AutoPlay => Traits.Admits(PlaybackTrait.AutoPlay);

    public bool Muted => Traits.Admits(PlaybackTrait.Muted);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilitySet<PlaybackTrait> traits,
        ref LoopMode loop,
        ref double rate,
        ref Option<double> start,
        ref Option<double> stop,
        ref Option<int> sectionRepeats,
        ref RedrivePolicy redrive) =>
        validationError = Defects(rate, start, stop, sectionRepeats) switch {
            { IsEmpty: true } => validationError,
            var rows => new ValidationError($"playback policy: {string.Join("; ", rows)}"),
        };

    static Seq<string> Defects(double rate, Option<double> start, Option<double> stop, Option<int> repeats) =>
        Seq((!double.IsFinite(rate) || rate <= 0d, "rate is not a finite positive multiplier"),
            (start.Exists(static value => !double.IsFinite(value) || value < 0d), "section start is not a finite non-negative offset"),
            (stop.Exists(static value => !double.IsFinite(value) || value < 0d), "section stop is not a finite non-negative offset"),
            ((start, stop).Apply(static (from, to) => from >= to).IfNone(false), "section start does not precede its stop"),
            (repeats.Exists(static count => count <= 0), "section repeat count is not positive"),
            (repeats.IsSome && stop.IsNone, "a repeat count carries no bounded section"))
            .Filter(static row => row.Item1).Map(static row => row.Item2);
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class MediaLease : IDisposable {
    readonly Atom<Option<Lease<IDisposable>>> held;

    MediaLease(Option<Control> control, Option<MpvContext> context, Option<Lease<IDisposable>> resource) {
        Control = control;
        Context = context;
        held = Atom(resource);
    }

    public static MediaLease Of(Option<Control> control, Option<MpvContext> context, Option<Lease<IDisposable>> resource) =>
        new(control, context, resource);

    public Option<Control> Control { get; }

    public Option<MpvContext> Context { get; }

    public void Dispose() => Cell.Take(held).Current.Iter(static lease => ignore(lease.Dispose()));
}

public sealed record CaptionEngine(
    Func<GgmlType, IO<Fin<WhisperFactory>>> Model,
    Func<SileroVadType, IO<Fin<WhisperVadFactory>>> Vad,
    Func<string, IO<Fin<float[]>>> Samples);

public sealed record MediaRuntime(
    IAsyncImageLoader Images,
    AssetRuntime Assets,
    Option<IStorageProvider> Storage,
    VisualRuntime Visual,
    CaptionEngine Captions);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class MediaSurfaces {
    static readonly Op Mounting = Op.Of(name: "appui.media.materialize");

    public static readonly InstrumentSpec Mounted = InstrumentSpec.Create(
        "rasm.appui.media.mounted", InstrumentKind.Count, MeasureForm.Whole, "{mount}",
        "media surfaces mounted by codec", Seq(AppUiTelemetry.CodecSlot), None, None, None);

    public static readonly InstrumentSpec Failed = InstrumentSpec.Create(
        "rasm.appui.media.failed", InstrumentKind.Count, MeasureForm.Whole, "{mount}",
        "media mounts failed by codec", Seq(AppUiTelemetry.CodecSlot), None, None, None);

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Mounted, Failed);

    public static IO<Fin<MediaLease>> Materialize(MediaSurface surface, MediaRuntime runtime) =>
        surface.Switch<MediaRuntime, IO<Fin<MediaLease>>>(
            state: runtime,
            image: static (media, i) => Observed(media, i, Raster(media, i)),
            svg: static (media, s) => Observed(media, s, Vector(media, s)),
            video: static (media, v) => Observed(media, v, Wired(v, v.Playback)),
            audio: static (media, a) => Observed(media, a, Wired(a, a.Playback)));

    static IO<Fin<MediaLease>> Raster(MediaRuntime runtime, MediaSurface.Image row) =>
        IO.liftAsync(async () => Optional(runtime.Images switch {
                IAdvancedAsyncImageLoader advanced => await advanced
                    .ProvideImageAsync(row.Source, Absent(runtime.Storage)).ConfigureAwait(false),
                var plain => await plain.ProvideImageAsync(row.Source).ConfigureAwait(false),
            }))
            .Map(resolved => resolved.Match(
                Some: bitmap => Fin.Succ(MediaLease.Of(
                    Some<Control>(new AdvancedImage(new Uri(row.Source, UriKind.RelativeOrAbsolute)) {
                        Source = row.Source, Stretch = row.Stretch, Loader = runtime.Images, FallbackImage = bitmap,
                    }),
                    None,
                    None)),
                None: () => Fin.Fail<MediaLease>(new ContentFault.DecodeFailed($"media/raster: {row.Source}"))));

    static IO<Fin<MediaLease>> Vector(MediaRuntime runtime, MediaSurface.Svg row) =>
        IO.lift(() => runtime.Assets.Svg.Load(row.Asset, SvgPosture.PictureOnly, None)
            .Bind(lease => runtime.Assets.Svg.Image(row.Asset, Colors.Transparent)
                .Map(image => MediaLease.Of(
                    Some<Control>(new Image { Source = image, Stretch = Stretch.Uniform }),
                    None,
                    Some<Lease<IDisposable>>(new Lease<IDisposable>.Owned(lease))))
                .Rollback(lease)));

    static IO<Fin<MediaLease>> Observed(MediaRuntime runtime, MediaSurface surface, IO<Fin<MediaLease>> mount) =>
        mount.Bind(outcome => IO.lift(() => runtime.Visual.Facts.Fire(
                at: AppUiPoint.Media,
                fact: new AppUiFact.Media(
                    surface.Key,
                    surface.Kind,
                    surface.Source,
                    outcome.Match<Option<Rasm.Contracts.Fault.FaultObservation>>(
                        Succ: static _ => None,
                        Fail: static error => Some(FaultWire.Observe(error)))),
                key: runtime.Visual.FactOp))
            .Bind(static fired => IO.lift(fired))
            .Map(_ => outcome));

    static IO<Fin<MediaLease>> Wired(MediaSurface surface, PlaybackPolicy policy) =>
        IO.lift(() => Seated(policy, surface.Codec.Visual))
            .Bind(held =>
                (Redrive.Run(policy.Redrive, PlaybackTransport.Load(held.Context, surface.Source))
                    .Map(_ => Fin.Succ(MediaLease.Of(
                        held.View.Map(static seated => (Control)seated),
                        Some(held.Context),
                        Some<Lease<IDisposable>>(new Lease<IDisposable>.Owned(held.Release)))))
                | @catch<IO, Fin<MediaLease>>(static _ => true,
                    static error => IO.pure(Fin.Fail<MediaLease>(error))))
                .Map(outcome => outcome.Rollback(held.Release))
                .As());

    static (MpvContext Context, Option<MpvView> View, IDisposable Release) Seated(PlaybackPolicy policy, bool visual) {
        MpvContext context = new();
        policy.Start.Iter(start => context.Start.Set(Seconds(start)));
        context.Pause.Set(!policy.AutoPlay);
        context.Mute.Set(policy.Muted);
        context.Speed.Set(policy.Rate);
        policy.Stop.Iter(stop => context.AbLoopB.Set(Seconds(stop)));
        policy.Start.Filter(_ => policy.Stop.IsSome).Iter(start => context.AbLoopA.Set(Seconds(start)));
        policy.SectionRepeats.Iter(count => context.AbLoopCount.Set(count.ToString(CultureInfo.InvariantCulture)));
        ignore(policy.Loop.Switch(
            state: context,
            none: static (mpv, _) => { mpv.LoopFile.Set("no"); return unit; },
            file: static (mpv, f) => { mpv.LoopFile.Set(LoopMode.Token(f.Repeats)); return unit; },
            playlist: static (mpv, p) => { mpv.LoopPlaylist.Set(LoopMode.Token(p.Repeats)); return unit; }));
        Option<MpvView> view = visual ? Some(Viewed(context)) : Option<MpvView>.None;
        return (context, view, view.Match(Some: static seated => (IDisposable)seated, None: () => context));
    }

    static MpvView Viewed(MpvContext context) {
        MpvView view = new() { Renderer = VideoRenderer.OpenGl };
        view.SetValue(MpvView.MpvContextProperty, context);
        return view;
    }

    internal static string Seconds(double at) => at.ToString("F3", CultureInfo.InvariantCulture);

    static IStorageProvider? Absent(Option<IStorageProvider> storage) =>
        storage.MatchUnsafe(Some: static provider => provider, None: static () => (IStorageProvider?)null);
}
```

## [04]-[PLAYBACK_TRANSPORT]

- Owner: `PlaybackTransport` the one playback path over the libmpv `MpvContext`; `MediaCommand` the `[Union]` whose grammar arm consumes the settled `Render/animation#TIMELINE_EDITOR` `TransportVerb` roster and whose payload arms carry what a media clip alone can express; `MediaIntent` the ONE media key roster both the command's own intent and the chrome's control keys read; `MediaLane` `[SmartEnum<string>]` the track-selection axis; `ScrubPhase`/`PlaylistStep`/`StillForm` the mode rosters the payload arms carry; `TrackTrait` the per-track capability vocabulary; `MediaState` the observed playback snapshot; `MediaTrack` the enumerated track row.
- Cases: `MediaCommand` = Grammar(TransportVerb) | Seek | Volume | Mute | Lane | Sidecar | Section | Scrub | Grab | Playlist; `MediaLane` = audio · subtitle · video; `ScrubPhase` = mark · revert; `PlaylistStep` = previous · next; `StillForm` = frame · subtitled; `TrackTrait` = default · forced · selected · external.
- Entry: `public static IO<Unit> Load(MpvContext context, string source)`; `public static IO<Unit> Command(MpvContext context, MediaCommand command)` — the ONE total dispatch folding every command onto its `MpvContext` member; `public static Channel<MediaCommand> Lane(int depth)` and `public static IO<Unit> Drive(MpvContext context, ChannelReader<MediaCommand> raised, CancellationToken token)` — the raised-command transport and its ordered drain; `public static IObservable<MediaState> Observe(MpvContext context)` — the event-driven state projection; `public static IO<Fin<Seq<MediaTrack>>> Tracks(MpvContext context)` — the enumerated track roster the lane menus render.
- Law: a raised command crosses ONE channel and the drain is single-reader, so the player sees the order a user produced. Two surfaces raising into the same context concurrently is exactly how a lane write lands between a section's two bound writes, and the channel is bounded with the OLDEST superseded raise dropped, because a scrub burst's stale positions are worth nothing while its newest is the whole intent.
- Auto: the nine SHARED verbs are the settled `TransportVerb` grammar and this page consumes them — `Grammar` is one command arm carrying that row, so a surface hosting both a 4D sequence and a media clip drives them through one vocabulary and the `transport.*` intent keys are spelled at exactly one owner. Only the payload-bearing media-local commands live here: an absolute seek, a volume level, a mute state, a track id per lane, a sidecar subtitle or audio file, an A-B section with its repeat count, a scrub phase, a frame grab under its form row, and a playlist step. Every media-local key is a `MediaIntent` row, so the key the command raises and the key the chrome's control carries are ONE constant rather than two literals that bind by accident. Observation is EVENT-DRIVEN off each typed wrapper's own `Changed` event: subscribing that event registers `ObserveProperty` under the wrapper's own `PropertyName` and `MpvFormat` and unsubscribing unregisters it, so the feed carries no raw property-name string, needs no request-id bookkeeping, and releases its registrations with the subscription. Each payload arrives as `MpvValueChangedEventArgs<T,TRaw>.NewValue`, a genuine `T?`, so an absent fact is absent and a scrub bar can distinguish frame zero from an unloaded core.
- Packages: HanumanInstitute.LibMpv, System.Threading.Channels (`.api/api-bcl-channels.md`), System.Reactive, Rasm (project — `CapabilitySet`), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new shared verb is one `TransportVerb` row at its animation owner breaking this page's own total dispatch at compile time, so the clip reading of a verb lands beside the timeline reading rather than defaulting into whichever arm a guard ladder happened to end on; a new media-local command is one `MediaCommand` case, one `MediaIntent` row, and one fold onto its `MpvContext` member; a new selectable lane is one `MediaLane` row carrying its own player token; a new track flag is one `TrackTrait` row; zero new surface.
- Boundary: the transport grammar is CONSUMED, never re-minted — a media-local nine-row verb vocabulary beside `TransportVerb` is the deleted form, because two rosters spelling one concept is exactly how a paused clip under a playing timeline arises, and the `transport.*` intent keys stay the animation owner's. The clip reading of that grammar rides the vocabulary's OWN generated `Switch`, so every verb answers a named arm and the roster's growth detonates here: a key-guard ladder over `TransportVerb.Key` is the deleted form, because its trailing arm swallows every verb no guard names and the swallowing arm is a real body a new verb then silently executes. Playback rides the typed `MpvContext` — a hand-rolled mpv command/property marshaller is the rejected form (`.api/api-libmpv.md` reject), so commands fold onto named members and command intake rides the catalogued `MpvCommand` `InvokeAsync` deferred invocation. THE CARRIER SPLITS BY DIRECTION and the split is stated: raised commands ride a `Channel<T>` because they are a producer-consumer stream whose ORDER is the contract and whose backpressure posture is a declared drop, while observed properties ride Rx because they are a hot multicast fan-in with replay — `Merge`/`Scan`/`Replay(1)`/`RefCount` is the shape a late-subscribing chrome needs and a channel cannot fan out. Position surfaces through the wrappers' own `Changed` events; a polling timer and a per-tick re-read of every property through `Get()` are both deleted, the second because a synchronous native property read per event on the UI thread is a poll wearing an event's name. Media commands derive as `CommandRow` rows executed through the command deck, so playback evidence rides the deck's `DeckOutcome` stream and no transport-local outcome type or command registry exists. A transient scrub MARKS the live position and reverts to that mark, so `Scrub` carries the `ScrubPhase` row the catalogued `RevertSeek(bool)` takes rather than a page-held snapshot. The `AudioId`/`SubId`/`VideoId` rows are `MpvOptionWithAutoNo<int>` sentinel wrappers — a typed id write rides the option base and the `auto`/`no` sentinels ride `SetAuto`/`SetNo`, never a raw property string; the `no` sentinel is how a lane turns OFF, which an int id cannot express. Track enumeration reads the indexed `track-list/{0}/…` wrappers off `TrackListCount` through one traversal whose per-index answer is an `Option`, so an unclaimed lane DROPS by absence rather than through a mutable accumulator's `continue`, and a flag the player did not answer grants NO trait — an unanswered default is not a default, which is exactly what a fabricated `false` claimed.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MediaLane {
    public static readonly MediaLane Audio = new("audio", token: "audio", selectable: true, static mpv => mpv.AudioId);
    public static readonly MediaLane Subtitle = new("subtitle", token: "sub", selectable: true, static mpv => mpv.SubId);
    public static readonly MediaLane Video = new("video", token: "video", selectable: false, static mpv => mpv.VideoId);

    public string Token { get; }

    public bool Selectable { get; }

    [UseDelegateFromConstructor]
    public partial MpvOptionWithAutoNo<int> Option(MpvContext context);

    public static Option<MediaLane> Of(string token) =>
        toSeq(Items).Find(lane => string.Equals(lane.Token, token, StringComparison.Ordinal));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TrackTrait : ICapability<TrackTrait> {
    public static readonly TrackTrait Default = new("default");
    public static readonly TrackTrait Forced = new("forced");
    public static readonly TrackTrait Selected = new("selected");
    public static readonly TrackTrait External = new("external");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LaneChoice {
    private LaneChoice() { }
    public sealed record Track(int Id) : LaneChoice;
    public sealed record Auto : LaneChoice;
    public sealed record Off : LaneChoice;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScrubPhase {
    public static readonly ScrubPhase Mark = new("mark", mark: true);
    public static readonly ScrubPhase Revert = new("revert", mark: false);

    public bool Marks { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaylistStep {
    public static readonly PlaylistStep Previous = new("previous", static mpv => mpv.PlaylistPrev());
    public static readonly PlaylistStep Next = new("next", static mpv => mpv.PlaylistNext());

    [UseDelegateFromConstructor]
    public partial MpvCommand Command(MpvContext context);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StillForm {
    public static readonly StillForm Frame = new("frame", ScreenshotOptions.Video);
    public static readonly StillForm Subtitled = new("subtitled", ScreenshotOptions.Subtitles | ScreenshotOptions.Video);

    public ScreenshotOptions Options { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MediaIntent {
    public static readonly MediaIntent Seek = new("media.seek");
    public static readonly MediaIntent Scrub = new("media.scrub");
    public static readonly MediaIntent Elapsed = new("media.elapsed");
    public static readonly MediaIntent Total = new("media.total");
    public static readonly MediaIntent Step = new("media.step");
    public static readonly MediaIntent Speed = new("media.speed");
    public static readonly MediaIntent Volume = new("media.volume");
    public static readonly MediaIntent Mute = new("media.mute");
    public static readonly MediaIntent Lane = new("media.lane");
    public static readonly MediaIntent Sidecar = new("media.sidecar");
    public static readonly MediaIntent Section = new("media.section");
    public static readonly MediaIntent Grab = new("media.grab");
    public static readonly MediaIntent Playlist = new("media.playlist");
    public static readonly MediaIntent Clock = new("media.clock");

    public string For(MediaLane lane) => $"{Key}.{lane.Key}";

    public string For(PlaylistStep step) => $"{Key}.{step.Key}";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MediaCommand {
    private MediaCommand() { }

    public sealed record Grammar(TransportVerb Verb) : MediaCommand;
    public sealed record Seek(double Seconds) : MediaCommand;
    public sealed record Volume(double Level) : MediaCommand;
    public sealed record Mute(bool Muted) : MediaCommand;
    public sealed record Lane(MediaLane Which, LaneChoice Choice) : MediaCommand;
    public sealed record Sidecar(MediaLane Which, string Path, Option<string> Title, Option<string> Language) : MediaCommand;
    public sealed record Section(double From, double To, Option<int> Repeats) : MediaCommand;
    public sealed record Scrub(ScrubPhase Phase) : MediaCommand;
    public sealed record Grab(string AbsolutePath, StillForm Form) : MediaCommand;
    public sealed record Playlist(PlaylistStep Step) : MediaCommand;

    public string Intent => Switch(
        grammar: static g => g.Verb.IntentKey,
        seek: static _ => MediaIntent.Seek.Key,
        volume: static _ => MediaIntent.Volume.Key,
        mute: static _ => MediaIntent.Mute.Key,
        lane: static l => MediaIntent.Lane.For(l.Which),
        sidecar: static s => MediaIntent.Sidecar.For(s.Which),
        section: static _ => MediaIntent.Section.Key,
        scrub: static _ => MediaIntent.Scrub.Key,
        grab: static _ => MediaIntent.Grab.Key,
        playlist: static p => MediaIntent.Playlist.For(p.Step));
}

// --- [MODELS] --------------------------------------------------------------------------

[Equatable]
public readonly partial record struct MediaState(
    Option<double> Position,
    Option<double> Duration,
    Option<double> Remaining,
    Option<double> Buffered,
    Option<bool> Playing,
    Option<bool> Seeking,
    Option<bool> Ended,
    Option<double> Volume,
    Option<bool> Muted,
    Option<int> PlaylistPosition,
    Option<int> PlaylistCount,
    CaptionCue Cue) {
    public static MediaState Empty { get; } = new(
        None, None, None, None, None, None, None, None, None, None, None, CaptionCue.Silent);

    public Option<double> Fraction =>
        (Position, Duration).Apply(static (at, span) => span > 0d ? at / span : 0d).As();

    public Option<double> BufferedFraction =>
        (Buffered, Duration).Apply(static (at, span) => span > 0d ? Math.Clamp(at / span, 0d, 1d) : 0d).As();
}

public readonly record struct CaptionCue(Option<string> Text, Option<double> From, Option<double> Until) {
    public static CaptionCue Silent { get; } = new(None, None, None);

    public bool Visible => Text.Exists(static text => !string.IsNullOrWhiteSpace(text));
}

public readonly record struct MediaTrack(
    int Id, MediaLane Lane, Option<string> Language, Option<string> Codec, CapabilitySet<TrackTrait> Traits) {
    public bool Selected => Traits.Admits(TrackTrait.Selected);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class PlaybackTransport {
    public static IO<Unit> Load(MpvContext context, string source) =>
        IO.liftAsync(async () => { await context.LoadFile(source).InvokeAsync().ConfigureAwait(false); return unit; });

    public static Channel<MediaCommand> Lane(int depth) =>
        Channel.CreateBounded<MediaCommand>(new BoundedChannelOptions(depth) {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
        });

    public static IO<Unit> Drive(MpvContext context, ChannelReader<MediaCommand> raised, CancellationToken token) =>
        IO.liftAsync(async () => {
            await foreach (MediaCommand command in raised.ReadAllAsync(token).ConfigureAwait(false)) {
                await Command(context, command).RunAsync().ConfigureAwait(false);
            }
            return unit;
        });

    public static IO<Unit> Command(MpvContext context, MediaCommand command) => command.Switch(
        state: context,
        grammar:  static (mpv, g) => Shared(mpv, g.Verb),
        seek:     static (mpv, s) => Write(mpv.TimePos, s.Seconds),
        volume:   static (mpv, v) => Write(mpv.Volume, v.Level),
        mute:     static (mpv, m) => Write(mpv.Mute, m.Muted),
        lane:     static (mpv, l) => l.Choice.Switch(
            state: l.Which.Option(mpv),
            track: static (option, t) => IO.liftAsync(async () => { await option.SetAsync(t.Id).ConfigureAwait(false); return unit; }),
            auto:  static (option, _) => IO.liftAsync(async () => { await option.SetAutoAsync().ConfigureAwait(false); return unit; }),
            off:   static (option, _) => IO.liftAsync(async () => { await option.SetNoAsync().ConfigureAwait(false); return unit; })),
        sidecar:  static (mpv, s) => Invoke(s.Which.Key == MediaLane.Audio.Key
            ? mpv.AudioAdd(s.Path, LoadOption.Select, Absent(s.Title), Absent(s.Language))
            : mpv.SubAdd(s.Path, LoadOption.Select, Absent(s.Title), Absent(s.Language))),
        section:  static (mpv, l) => IO.liftAsync(async () => {
            await mpv.AbLoopA.SetAsync(MediaSurfaces.Seconds(l.From)).ConfigureAwait(false);
            await mpv.AbLoopB.SetAsync(MediaSurfaces.Seconds(l.To)).ConfigureAwait(false);
            await mpv.AbLoopCount.SetAsync(LoopMode.Token(l.Repeats)).ConfigureAwait(false);
            return unit;
        }),
        scrub:    static (mpv, s) => Invoke(mpv.RevertSeek(s.Phase.Marks)),
        grab:     static (mpv, g) => Invoke(mpv.ScreenshotToFile(g.AbsolutePath, g.Form.Options)),
        playlist: static (mpv, p) => Invoke(p.Step.Command(mpv)));

    static IO<Unit> Shared(MpvContext context, TransportVerb verb) => verb.Switch(
        state: context,
        play:        static mpv => Write(mpv.Pause, false),
        pause:       static mpv => Write(mpv.Pause, true),
        stop:        static mpv => Invoke(mpv.Stop()),
        stepBack:    static mpv => Invoke(mpv.FrameBackStep()),
        stepForward: static mpv => Invoke(mpv.FrameStep()),
        jumpIn:      static mpv => Write(mpv.TimePos, 0d),
        jumpOut:     static mpv => IO.liftAsync(async () => {
            double? span = await mpv.Duration.GetAsync().ConfigureAwait(false);
            if (span is { } end) { await mpv.TimePos.SetAsync(end).ConfigureAwait(false); }
            return unit;
        }),
        loop:        static mpv => IO.liftAsync(async () => {
            await mpv.LoopFile.SetAsync("inf").ConfigureAwait(false);
            return unit;
        }),
        speed:       static mpv => IO.liftAsync(async () => {
            double held = await mpv.Speed.GetAsync().ConfigureAwait(false) ?? 1d;
            SpeedRung rung = SpeedRung.TryGet(held, out SpeedRung? at) ? at : SpeedRung.Normal;
            await mpv.Speed.SetAsync(rung.Next().Rate).ConfigureAwait(false);
            return unit;
        }));

    public static IObservable<MediaState> Observe(MpvContext context) =>
        Observable.Merge(
                Changed<double>(context.TimePos, static (s, v) => s with { Position = v }),
                Changed<double>(context.Duration, static (s, v) => s with { Duration = v }),
                Changed<double>(context.TimeRemaining, static (s, v) => s with { Remaining = v }),
                Changed<double>(context.DemuxerCacheTime, static (s, v) => s with { Buffered = v }),
                Changed<bool>(context.Pause, static (s, v) => s with { Playing = v.Map(static paused => !paused) }),
                Changed<bool>(context.Seeking, static (s, v) => s with { Seeking = v }),
                Changed<bool>(context.EofReached, static (s, v) => s with { Ended = v }),
                Changed<double>(context.Volume, static (s, v) => s with { Volume = v }),
                Changed<bool>(context.Mute, static (s, v) => s with { Muted = v }),
                Changed<int>(context.PlaylistPosition, static (s, v) => s with { PlaylistPosition = v }),
                Changed<int>(context.PlaylistCount, static (s, v) => s with { PlaylistCount = v }),
                ChangedText(context.SubText, static (s, v) => s with { Cue = s.Cue with { Text = v } }),
                Changed<float>(context.SubStart, static (s, v) => s with { Cue = s.Cue with { From = v.Map(static value => (double)value) } }),
                Changed<float>(context.SubEnd, static (s, v) => s with { Cue = s.Cue with { Until = v.Map(static value => (double)value) } }))
            .Scan(MediaState.Empty, static (held, apply) => apply(held))
            .Replay(1)
            .RefCount();

    public static IO<Fin<Seq<MediaTrack>>> Tracks(MpvContext context) =>
        (IO.liftAsync(async () => await context.TrackListCount.GetAsync().ConfigureAwait(false))
            .Bind(count => toSeq(Enumerable.Range(0, count ?? 0)).Traverse(index => Row(context, index)).As())
            .Map(static rows => Fin.Succ(rows.Somes()))
        | @catch<IO, Fin<Seq<MediaTrack>>>(static _ => true,
            static error => IO.pure(Fin.Fail<Seq<MediaTrack>>(error)))).As();

    static IO<Option<MediaTrack>> Row(MpvContext context, int index) =>
        IO.liftAsync(async () => {
            Option<MediaLane> lane = Optional(await context.TrackListType[index].GetAsync().ConfigureAwait(false)).Bind(MediaLane.Of);
            Option<int> id = Optional(await context.TrackListId[index].GetAsync().ConfigureAwait(false));
            Option<string> language = Optional(await context.TrackListLanguage[index].GetAsync().ConfigureAwait(false));
            Option<string> codec = Optional(await context.TrackListCodec[index].GetAsync().ConfigureAwait(false));
            Seq<(TrackTrait Trait, Option<bool> Answer)> flags = Seq(
                (TrackTrait.Default, Optional(await context.TrackListIsDefault[index].GetAsync().ConfigureAwait(false))),
                (TrackTrait.Forced, Optional(await context.TrackListIsForced[index].GetAsync().ConfigureAwait(false))),
                (TrackTrait.Selected, Optional(await context.TrackListIsSelected[index].GetAsync().ConfigureAwait(false))),
                (TrackTrait.External, Optional(await context.TrackListIsExternal[index].GetAsync().ConfigureAwait(false))));
            return (lane, id).Apply((claimed, ordinal) => new MediaTrack(
                ordinal, claimed, language, codec,
                flags.Filter(static row => row.Answer.IfNone(false))
                    .Fold(CapabilitySet<TrackTrait>.None, static (held, row) => held.With(row.Trait)))).As();
        });

    static IObservable<Func<MediaState, MediaState>> Changed<T>(
        MpvPropertyRead<T> property, Func<MediaState, Option<T>, MediaState> apply) where T : struct =>
        Observable.FromEventPattern<MpvValueChangedEventArgs<T, T>>(
                handler => property.Changed += handler,
                handler => property.Changed -= handler)
            .Select(pattern => fun((MediaState held) => apply(held, Optional(pattern.EventArgs.NewValue))));

    static IObservable<Func<MediaState, MediaState>> ChangedText(
        MpvPropertyReadString property, Func<MediaState, Option<string>, MediaState> apply) =>
        Observable.FromEventPattern<MpvValueChangedEventArgsRef<string, string>>(
                handler => property.Changed += handler,
                handler => property.Changed -= handler)
            .Select(pattern => fun((MediaState held) => apply(held, Optional(pattern.EventArgs.NewValue))));

    static IO<Unit> Write<T>(MpvPropertyWrite<T> property, T value) where T : struct =>
        IO.liftAsync(async () => { await property.SetAsync(value).ConfigureAwait(false); return unit; });

    static IO<Unit> Invoke(MpvCommand command) =>
        IO.liftAsync(async () => { await command.InvokeAsync().ConfigureAwait(false); return unit; });

    static string? Absent(Option<string> value) =>
        value.MatchUnsafe(Some: static text => text, None: static () => (string?)null);
}
```

## [05]-[TRANSPORT_CHROME]

- Owner: `ScrubTrack` the position-and-buffer bar model; `LoopRegion` the admitted section handles with their repeat count; `LaneMenu` the per-lane track menu projection; `MediaClockRole` `[SmartEnum<string>]` the playhead-authority posture carrying its own raise projection; `TransportChrome` the bar composition, the caption band, and the frame grab.
- Cases: `MediaClockRole` = independent · follower — under a 4D sequence the animation clock owns time and the transport follows.
- Law: exactly one clock owns the playhead. In `Independent` the player's own `time-pos` is truth and the animation timeline reads it; in `Follower` the `Render/animation` `TransportState.Head.Position` is truth and every tick seeks the player to it, so the transport bar becomes a READOUT of the sequence clock and its own play verb raises the timeline's verb rather than the player's.
- Entry: `public static Seq<ControlIntent> Bar(MediaState state, Seq<MediaTrack> tracks, Option<LoopRegion> section, MediaClockRole role, ResolvedLocale locale)` — the transport bar's intent rows; `public Fin<string> Raised(MediaCommand command)` on `MediaClockRole` — the command-to-deck intent-key projection the posture row itself carries; `public static Option<Control> Band(MediaState state, MarkdownStyling styling, ResolvedTheme theme)` — the caption band bracketing the playhead; `public static IO<Fin<MediaSurface>> Still(MpvContext context, string pickedPath, StillForm form)` — the frame grab minting the still's own media row, whose KEY is what the issue attachment arm consumes; `public static Fin<LoopRegion> Of(double from, double to, Option<int> repeats)` on `LoopRegion` — the section admission.
- Auto: the scrub track renders position over duration with the demuxer's own cache time shading the buffered extent, so a streamed source shows what it can seek into rather than a uniform bar; the timecode formats through `ResolvedLocale.Span`, so elapsed grammar is the locale's and no clock literal exists here. The frame-step pair, the speed menu reading the shared grammar's own published ladder, the loop-region handles with their repeat count, the SELECTABLE lane menus, and the volume slider are `ControlIntent` rows the one control factory materializes, so the bar mints no bespoke control and every key it carries is a `MediaIntent` row the command side already reads. Playlist verbs render only when the player reports more than one entry, because a next/previous pair over a single clip is chrome that teaches nothing. The frame grab writes through the player's own `screenshot-to-file` command under the destination the export delivery admitted, then hands the sealed path to the `Collab/issues#ISSUE_REGISTER` attachment arm, so a review still and an uploaded photo enter the issue board identically.
- Evidence: every raised command returns and fires the `Shell/commands` deck's `DeckOutcome`; this cluster has no parallel result.
- Packages: Avalonia, System.Reactive, HanumanInstitute.LibMpv, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new bar affordance is one `ControlIntent` row on the existing fold under one `MediaIntent` row; a new clock posture is one `MediaClockRole` row carrying its own raise projection; zero new surface.
- Boundary: the bar is a PROJECTION of observed state onto intent rows — a transport bar holding its own position field, its own play flag, or its own speed value is the deleted form, because a bar that can disagree with the player is a bar that will. THE CLOCK LAW IS THE ROW'S: each posture carries its own total raise projection, so under `Follower` the shared grammar arms route to the timeline's verb and only the media-local arms reach the player — a user pressing play under a 4D sequence advances the sequence and the clip follows. The catch-all arm that projection replaces was the defect its own posture existed to prevent: a tuple match over `(follows, command)` ending in `_` passed every command the two named guards missed straight through to the player, so a new media command was independent under a follower clock by default and the desynchronization the one-grammar law forecloses arrived through the arm that was supposed to close it. A SECTION is admitted at construction — finite, ordered, positively repeated — so a `LoopRegion` value is a raisable command by existence and no consumer re-guards its bounds; a handle pair that never reaches the bar is a type nothing can produce, which is why the section rides `Bar` as an option rather than as a model nothing seats. LANE MENUS render the SELECTABLE rows, so the lane roster's own column decides what the bar offers and a lane the enumeration must claim but no menu can select says so on its row rather than through an absent menu a reader has to interpret. The caption band renders the player's OWN cue through the caption typography role; a page-side cue parser beside the player's subtitle decoder is the deleted form. The grab path arrives as a VALUE from the save picker exactly as the export owner's file arm receives one, and the player writes the file itself, so no raster crosses this page and a media-local path computation is rejected; the grab's PRODUCT is a `MediaSurface` row, because `Collab/issues#ISSUE_REGISTER` `IssueOp.Attach` names a media key and never a blob or a path — this is the one site that keys a still, so an attachment, a gallery item, and a later export all resolve one referent.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MediaClockRole {
    public static readonly MediaClockRole Independent = new("independent", follows: false,
        static command => Fin.Succ(command.Intent));

    public static readonly MediaClockRole Follower = new("follower", follows: true,
        static command => command.Switch(
            grammar:  static g => Fin.Succ(g.Verb.IntentKey),
            seek:     static _ => Refused(MediaIntent.Seek),
            scrub:    static _ => Refused(MediaIntent.Scrub),
            volume:   static c => Fin.Succ(((MediaCommand)c).Intent),
            mute:     static c => Fin.Succ(((MediaCommand)c).Intent),
            lane:     static c => Fin.Succ(((MediaCommand)c).Intent),
            sidecar:  static c => Fin.Succ(((MediaCommand)c).Intent),
            section:  static c => Fin.Succ(((MediaCommand)c).Intent),
            grab:     static c => Fin.Succ(((MediaCommand)c).Intent),
            playlist: static c => Fin.Succ(((MediaCommand)c).Intent)));

    public bool Follows { get; }

    [UseDelegateFromConstructor]
    public partial Fin<string> Raised(MediaCommand command);

    static Fin<string> Refused(MediaIntent intent) =>
        Fin.Fail<string>(new ContentFault.UnresolvedRole($"media/clock: {intent.Key} on a followed clip that does not own its playhead"));
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct ScrubTrack(
    Option<double> Fraction, Option<double> Buffered, string Elapsed, string Total, bool Seeking) {
    public static ScrubTrack Of(MediaState state, ResolvedLocale locale) =>
        new(state.Fraction,
            state.BufferedFraction,
            Timecode(state.Position, locale),
            Timecode(state.Duration, locale),
            state.Seeking.IfNone(false));

    static string Timecode(Option<double> seconds, ResolvedLocale locale) =>
        seconds.Map(static value => Duration.FromSeconds(value))
            .Match(Some: locale.Span, None: static () => LocaleStrings.Key(nameof(ScrubTrack), "absent"));
}

public readonly record struct LoopRegion {
    LoopRegion(double from, double to, Option<int> repeats) => (From, To, Repeats) = (from, to, repeats);

    public double From { get; }

    public double To { get; }

    public Option<int> Repeats { get; }

    public static Fin<LoopRegion> Of(double from, double to, Option<int> repeats) =>
        double.IsFinite(from) && double.IsFinite(to) && to > from && repeats.ForAll(static count => count > 0)
            ? Fin.Succ(new LoopRegion(from, to, repeats))
            : Fin.Fail<LoopRegion>(new ContentFault.GrammarAbsent($"media/loop-region: [{from}, {to}]"));

    public MediaCommand Command() => new MediaCommand.Section(From, To, Repeats);
}

public readonly record struct LaneMenu(MediaLane Lane, Seq<OptionRow> Options, Option<int> Selected) {
    public static LaneMenu Of(MediaLane lane, Seq<MediaTrack> tracks, ResolvedLocale locale) =>
        tracks.Filter(track => track.Lane == lane) switch {
            var rows => new LaneMenu(
                lane,
                Seq(new OptionRow($"{lane.Key}.auto", LocaleStrings.Key(nameof(LaneMenu), "auto"), None, None),
                    new OptionRow($"{lane.Key}.off", LocaleStrings.Key(nameof(LaneMenu), "off"), None, None))
                + rows.Map(track => new OptionRow(
                    $"{lane.Key}.{track.Id.ToString(CultureInfo.InvariantCulture)}",
                    track.Language.IfNone(() => $"{lane.Key} {track.Id}"),
                    track.Codec,
                    None)),
                rows.Find(static track => track.Selected).Map(static track => track.Id)),
        };

    public Fin<MediaCommand> Choose(string optionKey) =>
        optionKey == $"{Lane.Key}.auto" ? Fin.Succ<MediaCommand>(new MediaCommand.Lane(Lane, new LaneChoice.Auto()))
        : optionKey == $"{Lane.Key}.off" ? Fin.Succ<MediaCommand>(new MediaCommand.Lane(Lane, new LaneChoice.Off()))
        : int.TryParse(optionKey.AsSpan(Lane.Key.Length + 1), CultureInfo.InvariantCulture, out int id)
            ? Fin.Succ<MediaCommand>(new MediaCommand.Lane(Lane, new LaneChoice.Track(id)))
            : Fin.Fail<MediaCommand>(new ContentFault.UnresolvedRole($"media/lane-option: {optionKey}"));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class TransportChrome {
    public static Seq<ControlIntent> Bar(
        MediaState state, Seq<MediaTrack> tracks, Option<LoopRegion> section, MediaClockRole role, ResolvedLocale locale) =>
        ScrubTrack.Of(state, locale) switch {
            var track =>
                Seq<ControlIntent>(
                    new ControlIntent.Slider(MediaIntent.Scrub.Key, 0d, 1d, 0.0005d, IntentBinding.Of(PaintRole.Accent)),
                    new ControlIntent.Chip(MediaIntent.Elapsed.Key, track.Elapsed, ChipPosture.Static, IntentBinding.Of(PaintRole.TextMuted)),
                    new ControlIntent.Chip(MediaIntent.Total.Key, track.Total, ChipPosture.Static, IntentBinding.Of(PaintRole.TextFaint)),
                    new ControlIntent.Segmented(MediaIntent.Step.Key, SegmentPosture.Command,
                        Seq(new OptionRow(TransportVerb.StepBack.Key, TransportVerb.StepBack.IntentKey, None, None),
                            new OptionRow(TransportVerb.StepForward.Key, TransportVerb.StepForward.IntentKey, None, None)),
                        IntentBinding.Of(PaintRole.Panel)),
                    new ControlIntent.Select(MediaIntent.Speed.Key, SelectPosture.Closed,
                        new OptionSource.Inline(TransportVerb.SpeedLadder.Map(static rate => new OptionRow(
                            Rate(rate), Rate(rate), None, None))),
                        VirtualWindowSpec.FixedRow(RateViewport), IntentBinding.Of(PaintRole.Panel)),
                    new ControlIntent.Slider(MediaIntent.Volume.Key, 0d, 100d, 1d, IntentBinding.Of(PaintRole.Accent)),
                    new ControlIntent.Toggle(MediaIntent.Mute.Key, LocaleStrings.Key(nameof(TransportChrome), "mute"),
                        IntentBinding.Of(PaintRole.Panel)))
                + Sections(section)
                + Lanes(tracks, locale)
                + Playlists(state)
                + Seq<ControlIntent>(new ControlIntent.Chip(MediaIntent.Clock.Key, role.Key, ChipPosture.Static,
                    IntentBinding.Of(role.Follows ? PaintRole.Warning : PaintRole.TextFaint))),
        };

    public static Option<Control> Band(MediaState state, MarkdownStyling styling, ResolvedTheme theme) =>
        state.Cue.Text.Filter(static text => !string.IsNullOrWhiteSpace(text))
            .Bind(text => theme.Type(TypographyRole.Caption, TypeEmphasis.Regular).Map(row => (Text: text, Row: row)))
            .Map(cue => (Control)new Border {
                Background = styling.Paint(SkinSlot.Surface),
                CornerRadius = new CornerRadius(styling.Step(SkinMetric.Radius)),
                Padding = new Thickness(styling.Step(SkinMetric.Gutter), styling.Step(SkinMetric.Gap)),
                Child = new SelectableTextBlock {
                    Text = cue.Text,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = styling.Paint(SkinSlot.Text),
                    FontFamily = new FontFamily(cue.Row.Family),
                    FontSize = cue.Row.Size,
                    FontWeight = (FontWeight)cue.Row.Weight,
                },
            });

    public static IO<Fin<MediaSurface>> Still(MpvContext context, string pickedPath, StillForm form) =>
        (PlaybackTransport
            .Command(context, new MediaCommand.Grab(pickedPath, form))
            .Map(_ => MediaCodecRow.Admit(StillKey(pickedPath), pickedPath))
            | @catch<IO, Fin<MediaSurface>>(static _ => true,
                static error => IO.pure(Fin.Fail<MediaSurface>(error)))).As();

    public const string GrabPrefix = "grab";

    public static string StillKey(string absolutePath) =>
        $"{GrabPrefix}/{Path.GetFileNameWithoutExtension(absolutePath)}";

    const double RateViewport = 200d;

    static string Rate(double rung) => rung.ToString("0.##", CultureInfo.InvariantCulture);

    static Seq<ControlIntent> Sections(Option<LoopRegion> section) =>
        section.Match(
            Some: static region => Seq<ControlIntent>(new ControlIntent.Range(
                MediaIntent.Section.Key, region.From, region.To, 0.0005d,
                $"{MediaIntent.Section.Key}.upper", IntentBinding.Of(PaintRole.Accent))),
            None: static () => Seq<ControlIntent>());

    static Seq<ControlIntent> Lanes(Seq<MediaTrack> tracks, ResolvedLocale locale) =>
        toSeq(MediaLane.Items).Filter(static lane => lane.Selectable)
            .Map(lane => LaneMenu.Of(lane, tracks, locale))
            .Filter(static menu => menu.Options.Count > 2)
            .Map(menu => (ControlIntent)new ControlIntent.Select(
                MediaIntent.Lane.For(menu.Lane), SelectPosture.Closed, new OptionSource.Inline(menu.Options),
                VirtualWindowSpec.FixedRow(RateViewport), IntentBinding.Of(PaintRole.Panel)));

    static Seq<ControlIntent> Playlists(MediaState state) =>
        state.PlaylistCount.Filter(static count => count > 1).Match(
            Some: static _ => Seq<ControlIntent>(new ControlIntent.Segmented(
                MediaIntent.Playlist.Key, SegmentPosture.Command,
                toSeq(PlaylistStep.Items).Map(static step => new OptionRow(
                    step.Key, LocaleStrings.Key(nameof(TransportChrome), step.Key), None, None)),
                IntentBinding.Of(PaintRole.Panel))),
            None: static () => Seq<ControlIntent>());
}
```

## [06]-[CAPTION_TRACK]

- Owner: `CaptionTrack` the media-to-caption fold; `CaptionRequest` the transcription request under the locale caption policy; `CaptionCueRow` the emitted cue; `CaptionSidecar` the produced subtitle artifact.
- Law: libmpv exposes NO PCM tap — its every render path is a video path and its audio surface is device output, so nothing on this page can read decoded samples out of the player. The media-to-caption route is therefore the SIDECAR: audio is transcribed from the source independently, written as a subtitle artifact, and joined to the SAME player through `SubAdd`, after which the player's own `sub-text`/`sub-start`/`sub-end` properties time the band. A live microphone caption is a capture concern belonging to the mic owner and never enters this page.
- Entry: `public static IO<Fin<CaptionSidecar>> Transcribe(CaptionRequest request, MediaRuntime runtime, InstrumentSet set, Option<ChannelWriter<CaptionCueRow>> live)` — VAD-gated streaming transcription under the locale policy, sealed as a sidecar artifact with both caption instruments written from the run's own cues; `public static IAsyncEnumerable<CaptionCueRow> Stream(CaptionRequest request, WhisperFactory model, IReadOnlyList<VadSegmentData> spans, float[] samples)` — the cue stream the fold and the live feed share; `public static MediaCommand Attach(CaptionSidecar sidecar)` — the one join, an ordinary `MediaCommand.Sidecar` on the subtitle lane.
- Auto: the language and translation election is the `Theme/locale#SPEECH_POLICY` `CaptionPolicy` — this page reads it and never decides it, so a caption's target language and its translate task come from the locale owner and a media-local language knob is unrepresentable. The MODEL and VAD elections are the request's own and reach the runtime's per-election mint, so a run against a small model reports a small model. Silero VAD gates the audio to speech spans BEFORE transcription, so silence costs nothing and a segment's timing starts from a detected span rather than from a fixed window. Each emitted `SegmentData` carries its own `Start`/`End` `TimeSpan`s, which become the cue's bounds directly, and its confidence column folds as a caption-quality fact on the telemetry spine. The transcript text shapes through the policy's own `Annotate`, so a complex-script caption carries the locale's `RunSpec` and its caption typography role.
- Evidence: both caption instruments are written by the run that holds the cues, so coverage and confidence enter at the fold without a parallel carrier.
- Packages: Whisper.net, System.Threading.Channels (`.api/api-bcl-channels.md`), HanumanInstitute.LibMpv, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions
- Growth: a new caption target is one `LocaleRow` at the locale owner; a new segmentation knob is one builder row on the one `With*` fold; a new emitted cue column is one `CaptionCueRow` member the serializer and the meter read; zero new surface.
- Boundary: transcription rides ONE loaded `WhisperFactory` per model election streaming through `ProcessAsync`; a cloud STT dependency, a hand-rolled VAD beside the Silero pipeline, an inline translation beside `WithTranslate`, and a leaked factory or processor handle are the four rejected forms (`.api/api-whisper-net.md` reject law) — the factories are the runtime's memoized per-election handles and the two PROCESSORS are per-run and release before the artifact returns. THE CUE IS A ROW: a four-slot anonymous tuple threaded through five signatures is an erased type past a boundary, and its slots were positional at every read. THE STREAM IS THE PACKAGE'S: `ProcessAsync` already answers `IAsyncEnumerable<SegmentData>`, so the cues surface as one `IAsyncEnumerable<CaptionCueRow>` and a nested pair of hand loops materializing a `Seq` before anything can read it is the deleted form — the fold consumes that stream ONCE, and a live consumer takes the same rows through an optional `ChannelWriter` so a progress surface renders cues as they land rather than after the whole file. A dropped live write never fails the transcription, because a caption run is the durable work and a progress feed is not. The band is rendered by `[05]`, timed by the PLAYER, so this cluster produces an artifact and never a live UI feed — a page-side cue clock beside the player's own subtitle timing is the deleted form. The sidecar is delivered through the one `Document/export#EXPORT_DESTINATIONS` `VisualDestination` gate, so a caption file lands under a profile root exactly as every other artifact does. The WebVTT timestamp is one declared NodaTime pattern, so the artifact's own grammar is a value rather than four format specifiers a reader has to reassemble.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct CaptionRequest(
    string Source, CaptionPolicy Policy, VisualDestination Destination, GgmlType Model, SileroVadType Vad);

public readonly record struct CaptionCueRow(Duration From, Duration Until, string Text, float Probability) {
    public Duration Covered => Until - From;
}

public sealed record CaptionSidecar(string Path, int Cues, Duration Covered, LocaleRow Target) {
    public MediaCommand Attach() =>
        new MediaCommand.Sidecar(MediaLane.Subtitle, Path, Some(Target.Key), Some(Target.Key));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class CaptionTrack {
    public static readonly InstrumentSpec Cues = InstrumentSpec.Create(
        "rasm.appui.media.caption.cues", InstrumentKind.Count, MeasureForm.Whole, "{cue}",
        "caption cues emitted by media source", Seq(AppUiTelemetry.DocSlot), None, None, None);

    public static readonly InstrumentSpec LowConfidence = InstrumentSpec.Create(
        "rasm.appui.media.caption.low-confidence", InstrumentKind.Count, MeasureForm.Whole, "{cue}",
        "caption cues below the confidence floor by media source", Seq(AppUiTelemetry.DocSlot), None, None, None);

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Cues, LowConfidence);

    const float ConfidenceFloor = 0.4f;

    const int SampleRate = 16_000;

    public static IO<Fin<CaptionSidecar>> Transcribe(
        CaptionRequest request, MediaRuntime runtime, InstrumentSet set, Option<ChannelWriter<CaptionCueRow>> live) =>
        (from samples in new FinT<IO, float[]>(runtime.Captions.Samples(request.Source))
         from vad in new FinT<IO, WhisperVadFactory>(runtime.Captions.Vad(request.Vad))
         from model in new FinT<IO, WhisperFactory>(runtime.Captions.Model(request.Model))
         from spans in FinT.liftIO<IO, IReadOnlyList<VadSegmentData>>(Detected(vad, samples))
         from cues in FinT.liftIO<IO, Seq<CaptionCueRow>>(Collected(request, model, spans, samples, live))
         from delivered in FinT.liftIO<IO, string>(
             ExportDelivery.Deliver(runtime.Visual, request.Destination, Serialized(cues, request.Policy)))
         let sidecar = new CaptionSidecar(
             delivered,
             cues.Count,
             cues.Fold(Duration.Zero, static (held, cue) => held + cue.Covered),
             request.Policy.Target)
         from _ in FinT.lift<IO, Unit>(Observed(set, request.Source, sidecar, cues))
         select sidecar).runFin.As();

    static IO<IReadOnlyList<VadSegmentData>> Detected(WhisperVadFactory vad, float[] samples) =>
        IO.liftAsync(async () => {
            await using WhisperVadProcessor detector = vad.CreateBuilder().Build();
            return await detector.DetectSpeechAsync(samples).ConfigureAwait(false);
        });

    public static async IAsyncEnumerable<CaptionCueRow> Stream(
        CaptionRequest request, WhisperFactory model, IReadOnlyList<VadSegmentData> spans, float[] samples) {
        WhisperProcessorBuilder builder = model.CreateBuilder();
        builder = request.Policy.Source.Match(
            Some: language => builder.WithLanguage(language),
            None: () => builder.WithLanguageDetection());
        builder = request.Policy.Translate ? builder.WithTranslate() : builder;
        await using WhisperProcessor processor = builder.SplitOnWord().WithTokenTimestamps().Build();
        foreach (VadSegmentData span in spans) {
            await foreach (SegmentData segment in processor.ProcessAsync(Sliced(samples, span)).ConfigureAwait(false)) {
                yield return new CaptionCueRow(
                    Duration.FromTimeSpan(span.Start + segment.Start),
                    Duration.FromTimeSpan(span.Start + segment.End),
                    segment.Text,
                    segment.Probability);
            }
        }
    }

    static IO<Seq<CaptionCueRow>> Collected(
        CaptionRequest request, WhisperFactory model, IReadOnlyList<VadSegmentData> spans, float[] samples,
        Option<ChannelWriter<CaptionCueRow>> live) =>
        IO.liftAsync(async () => {
            Seq<CaptionCueRow> held = Seq<CaptionCueRow>();
            await foreach (CaptionCueRow cue in Stream(request, model, spans, samples).ConfigureAwait(false)) {
                held = held.Add(cue);
                live.Iter(writer => ignore(writer.TryWrite(cue)));
            }
            live.Iter(static writer => ignore(writer.TryComplete()));
            return held;
        });

    static float[] Sliced(float[] samples, VadSegmentData span) {
        int from = Math.Clamp((int)(span.Start.TotalSeconds * SampleRate), 0, samples.Length);
        int until = Math.Clamp((int)(span.End.TotalSeconds * SampleRate), from, samples.Length);
        return samples[from..until];
    }

    static byte[] Serialized(Seq<CaptionCueRow> cues, CaptionPolicy policy) =>
        Encoding.UTF8.GetBytes(string.Concat(
            Seq("WEBVTT\n\n") + cues.Map(cue =>
                $"{Stamp.Format(cue.From)} --> {Stamp.Format(cue.Until)}\n{policy.Annotate(cue.Text).Text}\n\n")));

    static readonly DurationPattern Stamp = DurationPattern.CreateWithInvariantCulture("HH:mm:ss.fff");

    static Fin<Unit> Observed(InstrumentSet set, string mediaKey, CaptionSidecar sidecar, Seq<CaptionCueRow> cues) =>
        InstrumentSet.Tags((AppUiTelemetry.DocSlot, mediaKey)) switch {
            var doc =>
                from _cues in set.Write(Cues, sidecar.Cues, doc)
                from done in set.Write(LowConfidence, cues.Filter(static cue => cue.Probability < ConfidenceFloor).Count, doc)
                select done,
        };
}
```

## [07]-[GALLERY_SURFACE]

- Owner: `GalleryItem` the one browsable-image row; `GallerySource` `[SmartEnum<string>]` the intake provenance; `GalleryState` the selection projection; `GalleryIntake` the generated mapper over the three provenances; `GallerySurface` the filmstrip, the lightbox, and the zoom seating.
- Cases: `GallerySource` = capture · upload · grab — render captures, uploaded photos, and transport frame grabs browse identically because they are three rows of one vocabulary, not three surfaces.
- Entry: `public static Seq<GalleryItem> Of(Seq<ThumbnailRow> captures, Seq<string> uploads, Seq<MediaSurface> grabs)` — the capture-set consumption; `public static IO<Fin<MediaLease>> Filmstrip(GalleryItem item, MediaRuntime runtime)` — the thumbnail intake through the shared loader cache; `public static DialogIntent Lightbox(GalleryState state)` — the overlay-canvas seating; `public static ZoomBorder Zoom(Control content)` — the settled pan-zoom row; `public static Seq<ControlIntent> Strip(GalleryState state, VirtualWindowSpec window)` — the windowed strip rows.
- Auto: the filmstrip renders the `Render/capture#THUMBNAIL_PIPELINE` `Gallery`/`GalleryRetina` variants under the capture owner's OWN published variant key, so a strip cell and the file a capture sealed are one name; intake rides the SHARED `IAsyncImageLoader` through the `[03]` raster arm, so a thumbnail already on screen in a list is a cache hit in the strip. The three provenances cross onto one row through ONE generated mapper, so a fourth intake is one method beside three rather than a fourth hand projection with its own caption rule. The lightbox is a `DialogIntent.Layer` on `OverlayShape.Editor` on the canvas stack, so it inherits `OverlayShape.Editor`'s full-surface posture, its depth and material tiers, its dialog motion plan, and its registration and teardown from the one stack owner; next and previous walk the same ordered item sequence the strip renders, so the strip's order and the lightbox's traversal cannot disagree. Zoom is `PanAndZoom` `ZoomBorder` hosting the resolved image as its `Child`, so the affine, the gestures, the clamps, the double-click ladder, and the fit commands are the settled owner's and this page mints no transform. Load state is the CONTROL's own: `IsLoading` and `CurrentImage` are `DirectProperty` projections the item template binds, and a failed resolve renders the row's `FallbackImage` under the item's error caption, so a broken thumbnail says so.
- Packages: AsyncImageLoader.Avalonia, PanAndZoom, Avalonia, SkiaSharp, Riok.Mapperly, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new intake provenance is one `GallerySource` row and one `GalleryIntake` method; a new gallery affordance is one `ControlIntent` row on the strip fold; zero new surface.
- Boundary: the gallery COMPOSES five settled owners and owns none of them — a gallery-local thumbnail cache, a gallery-local overlay host, a gallery-local matrix transform, a gallery-local spinner, and a gallery-local artifact-key spelling are the five deleted forms. THE VARIANT KEY IS THE CAPTURE OWNER'S: `ThumbnailRow.BlobKey(variant)` is the one blob-address authority and this page READS it, because a key rebuilt here is a second spelling of a delivered file's identity and points the strip at files no capture sealed the moment either side retunes — a comment claiming the key is read off the capture owner while the fence rebuilt it verbatim was the exact divergence a shared identity cannot survive. The strip windows through the ONE `Shell/virtualization#WINDOW_OWNER` fabric, so a thousand-capture gallery realizes exactly its viewport and a gallery-local list is rejected. Loading and failure states come from the loader's OWN signals — a page-held `bool loading` beside `AdvancedImage.IsLoading` is the deleted form, because two truths about one load is one truth too many. The lightbox seats on the CANVAS stack and never the session stack, so opening one over an in-flight modal is representable and the session stack's single occupancy is untouched. The strip cell names the `UniformToFill` election and nothing else, which is why it is a projection onto the codec union's own raster arm rather than a second materialize.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GallerySource {
    public static readonly GallerySource Capture = new("capture");
    public static readonly GallerySource Upload = new("upload");
    public static readonly GallerySource Grab = new("grab");
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct GalleryItem(string Key, GallerySource Source, string Thumb, string Full, string Caption);

public readonly record struct GalleryState(Seq<GalleryItem> Items, int Focused) {
    public Option<GalleryItem> Current => Items.Skip(Focused).Head;

    public GalleryState Step(int delta) =>
        Items.IsEmpty ? this : this with { Focused = Math.Clamp(Focused + delta, 0, Items.Count - 1) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class GallerySurface {
    public static Seq<GalleryItem> Of(Seq<ThumbnailRow> captures, Seq<string> uploads, Seq<MediaSurface> grabs) =>
        captures.Map(GalleryIntake.ToItem) + uploads.Map(GalleryIntake.ToItem) + grabs.Map(GalleryIntake.ToItem);

    public static IO<Fin<MediaLease>> Filmstrip(GalleryItem item, MediaRuntime runtime) =>
        MediaSurfaces.Materialize(new MediaSurface.Image(item.Key, item.Thumb, Stretch.UniformToFill), runtime);

    public static DialogIntent Lightbox(GalleryState state) =>
        new DialogIntent.Layer(OverlayShape.Editor, LightboxTemplate, new GalleryViewModel(state), new LayerAnchor.Bound());

    public static ZoomBorder Zoom(Control content) =>
        new() {
            Child = content,
            Stretch = StretchMode.Uniform,
            EnableConstrains = true,
            AutoCalculateMinZoom = true,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomToFit,
            EnableGestures = true,
            EnableGestureZoom = true,
            EnableKeyboardNavigation = true,
        };

    public static Seq<ControlIntent> Strip(GalleryState state, VirtualWindowSpec window) =>
        Seq<ControlIntent>(
            new ControlIntent.Select("gallery.items", SelectPosture.Closed,
                new OptionSource.Inline(state.Items.Map(static item =>
                    new OptionRow(item.Key, item.Caption, Some(item.Source.Key), None))),
                window, IntentBinding.Of(PaintRole.Panel)),
            new ControlIntent.Segmented("gallery.step", SegmentPosture.Command,
                Seq(new OptionRow("previous", LocaleStrings.Key(nameof(GallerySurface), "previous"), None, None),
                    new OptionRow("next", LocaleStrings.Key(nameof(GallerySurface), "next"), None, None)),
                IntentBinding.Of(PaintRole.Panel)));

    public const string LightboxTemplate = "gallery.lightbox";
}

// --- [COMPOSITION] ---------------------------------------------------------------------

[Mapper(
    RequiredMappingStrategy = RequiredMappingStrategy.Target,
    EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class GalleryIntake {
    [MapPropertyFromSource(nameof(GalleryItem.Source), Use = nameof(Sealed))]
    [MapPropertyFromSource(nameof(GalleryItem.Thumb), Use = nameof(Small))]
    [MapPropertyFromSource(nameof(GalleryItem.Full), Use = nameof(Retina))]
    [MapProperty(nameof(ThumbnailRow.Key), nameof(GalleryItem.Caption))]
    public static partial GalleryItem ToItem(ThumbnailRow row);

    [MapPropertyFromSource(nameof(GalleryItem.Source), Use = nameof(Grabbed))]
    [MapProperty(nameof(MediaSurface.Source), nameof(GalleryItem.Thumb))]
    [MapProperty(nameof(MediaSurface.Source), nameof(GalleryItem.Full))]
    [MapPropertyFromSource(nameof(GalleryItem.Caption), Use = nameof(Named))]
    public static partial GalleryItem ToItem(MediaSurface still);

    public static GalleryItem ToItem(string path) =>
        new(path, GallerySource.Upload, path, path, Path.GetFileName(path));

    // --- [CONVERTERS]
    [UserMapping] private static GallerySource Sealed(ThumbnailRow row) => GallerySource.Capture;
    [UserMapping] private static string Small(ThumbnailRow row) => row.BlobKey(ThumbnailVariant.Gallery);
    [UserMapping] private static string Retina(ThumbnailRow row) => row.BlobKey(ThumbnailVariant.GalleryRetina);
    [UserMapping] private static GallerySource Grabbed(MediaSurface still) => GallerySource.Grab;
    [UserMapping] private static string Named(MediaSurface still) => Path.GetFileName(still.Source);
}
```

## [08]-[DIFF_SEAT]

- Owner: `DiffPane` the mounted editor capsule per layout seat, keyed by the pane key the surface itself mints; `DiffReading` the seat's cursor-and-extent readout; `PropertyDiffRow` the structured property change; `DiffSeating` the composition-bound seating context both entry points read; `DiffSeat` the mounted pane roster over one surface value; `DiffSeats` the mount, the layout toggle, the collapse reveal, and the cursor moves; `DiffFolds` the generated region-to-fold mapper.
- Entry: `public static Fin<DiffSeat> Mount(DiffSurface surface, DiffSeating seating)` — seats the surface's cuts into as many panes as the layout declares, keys each pane to the surface's own `PaneKey`, attaches the bands and the gutter margin over the reveal arrow, and folds every collapsed region; `public static Fin<DiffSeat> Relayout(DiffSeat seat, DiffLayout layout, DiffSeating seating)` — the one presentation toggle re-seating the SAME hunk sequence; `public static Fin<DiffSeat> Reveal(DiffSeat seat, int region)` — the in-place expansion; `public static Fin<DiffSeat> Focus(DiffSeat seat, int hunk)` and `public static Fin<DiffSeat> Walk(DiffSeat seat, int delta)` — the absolute and relative cursor seats, one scroll fold under both; `public Option<DiffPane> Pane(string key)` and `public DiffReading Reading` on `DiffSeat`; `public static Seq<TableColumnRow<PropertyDiffRow>> Columns()` and `public static Seq<PropertyDiffRow> Properties(Seq<(string Key, Option<string> Baseline, Option<string> Current)> cells)` — the property leg.
- Auto: the seat MOUNTS the `Collab/compare#COMPARE_SESSION` `DiffSurface` value and mints none of it — the hunks are `ThreeWay.Diff`'s, the cuts and their line spans are the surface's, the regions are its own retained-context collapse, and the cursor is its modular walk, so a compare opened from the version history and one opened from an option render identically and this seat carries no geometry arrow of its own. Layout is a ROW read: the surface's `DiffLayout` declares how many panes to seat and `DiffLayout.Side(pane)` answers which `ConflictSide` each holds, so side-by-side and inline are two seat geometries over one hunk sequence and toggling re-seats without re-diffing. Every pane read is PANE-ADDRESSED off that same geometry — the cut text, the per-hunk line span the bands measure, and the collapsed region set the resync folds. Each mounted pane carries the surface's own `PaneKey(ordinal)` beside that ordinal, so the editor a seat mounted and the intent row the surface's body seats address one pane while every pane-addressed read resolves without parsing a key. Bands and the gutter margin come from the ONE `Editing/conflict#HUNK_CHROME` `HunkBands.Attach` mount under `HunkPosture.Navigating`, and the mount's published `Lane` is the pane's own change-lane arrow the code pane opens with. Collapsed regions cross onto the code pane's own `FoldRegion` through one generated mapper and ride its whole-set `Fold` resync. The property leg pairs baseline against current per key and renders as a table over the `Editing/tables#GRID_SUBSTRATE` column rows.
- Packages: Avalonia.AvaloniaEdit, Avalonia, Riok.Mapperly, Rasm (project — `Custody`, `Op`, `CapabilitySet`, `ColumnTrait`), LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new presentation is one `DiffLayout` row at its own owner reaching this seat with zero rows here, because every pane read is already addressed by ordinal and a third pane is a third fold step; a new structured leg is one projection onto the settled column rows; a new readout is one `DiffReading` column; a new seating capability is one `DiffSeating` column no entry point's arity sees; zero new surface, zero second differ.
- Boundary: this seat renders and never computes — a seat-local differ, a seat-local hunk model, a seat-local collapse list, a seat-local band renderer, and a seat-local line-span arrow are the five deleted forms, because each already exists at an owner and a copy here would diverge on the first fix; the span arrow's removal is what makes the claim structural rather than stated, since a caller-supplied geometry is a second authority over where a hunk sits. The pane's SIDE is the layout row's answer and never a seat derivation: `DiffLayout.Side(pane)` seats the baseline in the first pane of a two-pane geometry and the take in the second, so the derivation that renders the changed cut in both panes — passing every shape check while showing a reviewer nothing — is unspellable here. A pane holds its WHOLE cut, because the bands, the regions, and the cursor all address that cut's own line numbering: a document built from the changed runs alone leaves the text in one line space and every decoration measuring another, and each consequence is silent — segments drop past the document end, the overview lane publishes nothing, and the collapse regions fold nothing. Panes are SUPPLIED rather than constructed, so the host decides where the editors live and this fold decides only what goes in them; a surface that closed no hunk supplies none, because that is the state the surface's own body renders as unchanged. THE SEATING CONTEXT IS ONE VALUE: the pane factory, the reveal arrow, the registry, the language, and the resolved theme travel together on every entry point, so a sixth capability is a column rather than two signatures re-spelled in parallel. Pane mounts carry CUSTODY through the kernel owner: the acquire chain rolls back LIFO on refusal — `Custody.Rollback`, because a successful mount TRANSFERS custody into the returned seat and a bracket would dispose the panes the seat just accepted — and a refused relayout leaves the standing seat intact, so no partial geometry ever holds a band renderer, a gutter margin, a segment collection, or a grammar installation over a document nothing shows. The seat is READ-ONLY on both legs, and structurally so: the pane opens with NO `PaneAffordance.Editable` grant, and the gutter takes `HunkPosture.Navigating` — one `ConflictSide.Base` marker over every hunk, bound to a `reveal` arrow that SEATS THE CURSOR at the named hunk — so no `ConflictSide` resolution channel reaches a surface whose inverse is the time-travel owner's intent path. The folding manager is the SESSION's: the pane opens with the `Folding` grant, which installs one manager and uninstalls it with the session, so a second install would seat two fold margins on one editor. CHROME is the surface's: `DiffSurface.Body` seats the transport toolbar, the pane geometry, and the no-differences empty state, and the intent keys the deck raises are its constants — this seat answers those raises with a new seat and mints no toolbar, no empty state, and no intent key of its own. Every mount is disposed with the seat, because a segment collection left attached keeps moving offsets for a document nothing shows.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

public sealed record DiffPane(
    string Key, int Ordinal, TextEditor Editor, ConflictSide Side, FoldingManager Folding, IDisposable Mounts) : IDisposable {
    public void Dispose() => Mounts.Dispose();
}

public readonly record struct DiffReading(int Cursor, int Hunks, int Regions, int Collapsed) {
    public bool Unchanged => Hunks == 0;
}

public readonly record struct PropertyDiffRow(string Key, Option<string> Baseline, Option<string> Current) {
    public string BaselineText => Baseline.IfNone(string.Empty);
    public string CurrentText => Current.IfNone(string.Empty);
    public bool Added => Baseline.IsNone && Current.IsSome;

    public bool Removed => Baseline.IsSome && Current.IsNone;

    public bool Changed => (Baseline, Current)
        .Apply(static (before, after) => !string.Equals(before, after, StringComparison.Ordinal))
        .IfNone(false);
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed record DiffSeating(
    Func<DiffLayout, Seq<TextEditor>> Panes,
    Action<int> Reveal,
    RasmRegistry Registry,
    string Language,
    ResolvedTheme Resolved);

public sealed record DiffSeat(DiffSurface Surface, Seq<DiffPane> Panes) : IDisposable {
    public Option<DiffPane> Pane(string key) => Panes.Find(pane => string.Equals(pane.Key, key, StringComparison.Ordinal));

    public DiffReading Reading =>
        Panes.Bind(pane => Surface.Regions(pane.Ordinal)) switch {
            var folds => new DiffReading(
                Surface.Cursor, Surface.Hunks.Count, folds.Count,
                folds.Filter(static region => region.Collapsed).Count),
        };

    public void Dispose() => Panes.Iter(static pane => pane.Dispose());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class DiffSeats {
    static readonly Op Seating = Op.Of(name: "appui.diff.seat");

    public static Fin<DiffSeat> Mount(DiffSurface surface, DiffSeating seating) =>
        surface.Hunks.IsEmpty
            ? Fin.Succ(new DiffSeat(surface, Seq<DiffPane>()))
            : seating.Panes(surface.Layout) switch {
                var editors when editors.Count != surface.Layout.Panes =>
                    Fin.Fail<DiffSeat>(new ContentFault.UnresolvedRole(
                        $"diff/pane-count: {surface.Layout.Key} seats {surface.Layout.Panes}, host supplied {editors.Count}")),
                var editors => editors
                    .Map(static (editor, ordinal) => (Editor: editor, Ordinal: ordinal))
                    .Fold(Fin.Succ(Seq<DiffPane>()), (acc, row) => acc.Bind(held =>
                        Seated(surface, row.Editor, row.Ordinal, seating)
                            .Map(held.Add)
                            .Rollback([.. held])))
                    .Map(mounted => new DiffSeat(surface, mounted)),
            };

    public static Fin<DiffSeat> Relayout(DiffSeat seat, DiffLayout layout, DiffSeating seating) =>
        Mount(seat.Surface with { Layout = layout }, seating)
            .Bind(reseated => Seating.Catch(() => { seat.Dispose(); return Fin.Succ(reseated); }));

    public static Fin<DiffSeat> Reveal(DiffSeat seat, int region) =>
        region >= 0 && seat.Panes.Exists(pane => region < seat.Surface.Regions(pane.Ordinal).Count)
            ? seat.Surface.Reveal(region) switch {
                var revealed => Seating.Catch(() => {
                    seat.Panes.Iter(pane => ignore(Refold(pane, revealed)));
                    return Fin.Succ(seat with { Surface = revealed });
                }),
            }
            : Fin.Fail<DiffSeat>(new ContentFault.UnresolvedRole($"diff/reveal: {region} names no collapsed run on this seat"));

    public static Fin<DiffSeat> Walk(DiffSeat seat, int delta) =>
        Scrolled(seat, seat.Surface.Walk(delta), "walk");

    public static Fin<DiffSeat> Focus(DiffSeat seat, int hunk) =>
        hunk >= 0 && hunk < seat.Surface.Hunks.Count
            ? Scrolled(seat, seat.Surface with { Cursor = hunk }, "focus")
            : Fin.Fail<DiffSeat>(new ContentFault.UnresolvedRole($"diff/focus: {hunk} outside {seat.Surface.Hunks.Count} hunks"));

    static Fin<DiffSeat> Scrolled(DiffSeat seat, DiffSurface moved, string verb) =>
        moved.Hunks.IsEmpty
            ? Fin.Succ(seat with { Surface = moved })
            : Seating.Catch(() => {
                seat.Panes.Iter(pane => pane.Editor.ScrollToLine(moved.Span(pane.Ordinal, moved.Cursor).First));
                return Fin.Succ(seat with { Surface = moved });
            });

    public static Seq<TableColumnRow<PropertyDiffRow>> Columns() =>
        Seq(Column("property", nameof(PropertyDiffRow.Key), "diff.column.property", static row => row.Key),
            Column("baseline", nameof(PropertyDiffRow.BaselineText), "diff.column.baseline", static row => row.BaselineText),
            Column("current", nameof(PropertyDiffRow.CurrentText), "diff.column.current", static row => row.CurrentText));

    public static Seq<PropertyDiffRow> Properties(Seq<(string Key, Option<string> Baseline, Option<string> Current)> cells) =>
        cells.Map(static cell => new PropertyDiffRow(cell.Key, cell.Baseline, cell.Current))
            .Filter(static row => row.Added || row.Removed || row.Changed);

    static Fin<DiffPane> Seated(DiffSurface surface, TextEditor editor, int ordinal, DiffSeating seating) =>
        Seating.Catch(() => {
                editor.Document = new TextDocument(surface.Text(ordinal));
                return Fin.Succ(editor);
            })
            .Bind(seated => HunkBands.Attach(
                    seated, surface.Hunks, hunk => surface.Span(ordinal, hunk),
                    HunkPosture.Navigating, (hunk, _) => seating.Reveal(hunk)) switch {
                var mount => Pane(surface, seated, ordinal, mount, seating).Rollback(mount),
            });

    static Fin<DiffPane> Pane(
        DiffSurface surface, TextEditor seated, int ordinal, HunkMount mount, DiffSeating seating) =>
        new CodePane(
                CapabilitySet<PaneAffordance>.Of(PaneAffordance.LineNumbers, PaneAffordance.Folding),
                EditorOptionsRow.Default, CompletionPolicy.Default)
            .Open(seated, seating.Registry, seating.Language, seating.Resolved, mount.Lane)
            .Bind(session => session.Folding
                .ToFin(new ContentFault.GrammarAbsent($"diff/pane-folding: {surface.PaneKey(ordinal)} opened without a manager"))
                .Map(manager => {
                    DiffPane pane = new(
                        surface.PaneKey(ordinal), ordinal, seated, surface.Layout.Side(ordinal), manager,
                        new CompositeDisposable(session, mount));
                    ignore(Refold(pane, surface));
                    return pane;
                }));

    static Unit Refold(DiffPane pane, DiffSurface surface) =>
        CodePane.Fold(pane.Folding, pane.Editor.Document, surface.Regions(pane.Ordinal).Map(DiffFolds.ToFold));

    static TableColumnRow<PropertyDiffRow> Column(string key, string path, string header, Func<PropertyDiffRow, string> read) =>
        new(AggregateColumn.Create(key), header, TableCellKind.Text,
            new TableColumnAccess<PropertyDiffRow>.Plain(
                Cell: Some<BindingBase>(new Binding(path)), Export: read),
            new DataGridLength(1d, DataGridLengthUnitType.Star),
            CapabilitySet<ColumnTrait>.Of(ColumnTrait.Sortable));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

[Mapper(
    RequiredMappingStrategy = RequiredMappingStrategy.Target,
    EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class DiffFolds {
    [MapPropertyFromSource(nameof(FoldRegion.Title), Use = nameof(Title))]
    [MapProperty(nameof(DiffRegion.Collapsed), nameof(FoldRegion.Closed))]
    public static partial FoldRegion ToFold(DiffRegion region);

    [UserMapping]
    private static string Title(DiffRegion region) =>
        $"diff.collapsed.{(region.Last - region.First + 1).ToString(CultureInfo.InvariantCulture)}";
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Markdown materialization, media codec rows, transport, captions, gallery, and the diff seat
    accDescr: Typography markdown rows rendering into blocks with an outline and minted media rows, a media surface materializing one kernel-leased mount per codec case, a transport path consuming the settled transport verb grammar over a raised command channel and observing player state event-driven, a caption sidecar streamed as cues and joined back into the same player, a gallery composing thumbnails, the overlay canvas, and the pan-zoom owner, and a diff seat mounting the compare session's pane-addressed render into the code pane's band and fold machinery under one custody chain.
    MarkdownDocumentRows --> MarkdownRenderer
    MarkdownRenderer --> MarkdownRendered
    MarkdownRenderer -->|fence scope| RasmRegistry
    MarkdownRenderer -->|grid columns| TableColumnRow
    MarkdownRendered -->|Outline| SearchOpen
    MarkdownRendered -->|Media| MediaSurface
    MediaSurface -->|Materialize| MediaLease
    MediaLease -->|Lease, Custody| KernelCustody["Kernel Lease and Custody"]
    MediaLease -->|Surfaces.Mount| SurfaceSession
    MediaSurface -->|Image| AsyncImageLoader
    MediaSurface -->|Svg| SvgPipeline
    MediaSurface -->|Video/Audio| MpvContext
    TransportVerb["Animation TransportVerb"] --> MediaCommand
    MediaCommand -->|raised| CommandChannel["Channel of MediaCommand"]
    CommandChannel -->|ordered drain| PlaybackTransport
    PlaybackTransport --> MpvContext
    MpvContext -->|Changed events| MediaState
    MediaState --> TransportChrome
    MediaIntent --> MediaCommand
    MediaIntent --> TransportChrome
    CaptionTrack -->|cue stream| CaptionSidecar
    CaptionSidecar -->|SubAdd| MpvContext
    MpvContext -->|sub-text| MediaState
    ThumbnailRow --> GalleryIntake
    GalleryIntake --> GallerySurface
    GallerySurface -->|Editor| OverlayCanvas
    GallerySurface -->|Child| ZoomBorder
    DiffSurface["Sync CompareSession DiffSurface"] -->|Text, Span, Regions per pane| DiffSeats
    DiffSeats -->|HunkBands.Attach| CodePane
    DiffSeats -->|DiffFolds| FoldRegion
    DiffSeats --> DiffPane
    TransportChrome -->|Still| MediaSurface
    MediaSurface --> AppUiFactMedia["AppUiFact.Media"]
    AppUiFactMedia --> HookSet
```

## [09]-[RESEARCH]

(none)
