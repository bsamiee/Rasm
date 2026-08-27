# [APPUI_CHARTS_INK]

The chart plane's ink base: `ChartChrome` folds every chart chrome key from the resolved theme onto typed roles with its family and draw-layer columns, `ChromeInk` closes the stroke-fill-text-chip mint over one `StrokeForm` restyle body, `PaintFamily` binds one anchor role beside one `Colormap`, `ChartInk` is the one resolved paint set every chart, tile, and threshold reads with its atomic re-tint, `ChartComposition` is the one process-wide LiveCharts registration, and `ChartFault` carries each failure through a direct generated union case. Severity ink resolves through the folder `Severity` family (`Theme/tokens#TOKEN_CATALOG`), so a warn band, a warn alert, and a warn cell are one pigment at one rank. The severity ladder and the constraint profile seat beside the paints they route to — a crossing classifies once and every projection reads that one fold.

## [01]-[INDEX]

- [02]-[CHART_FAULTS]: The Charts fault family on the kernel `Fault` floor.
- [03]-[CHART_PAINTS]: Chrome role vocabulary; one paint resolver; composition registration; re-tint law.
- [04]-[THRESHOLD_FAMILY]: The ordered base-plus-steps list and its five projections.
- [05]-[CONSTRAINT_PROFILE]: The saved check set, the unit-safe comparison, and the pressure ranking.

## [02]-[CHART_FAULTS]

- Owner: `ChartFault` — the direct generated `[Union]` every Charts page raises, with one `[FaultCase]` leaf per semantic failure.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: `ChartFault` grows by one `[FaultCase]` leaf and its owning constructions.
- Boundary: `ChartFault` owns failures shared across the Charts sub-domain; each leaf carries only the payload its semantic case requires.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChartFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Chart;
    private ChartFault(string detail) { Detail = detail; }
    public string Detail { get; }
    public override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record DuplicateTile(string LayoutKey) : ChartFault($"chart/duplicate-tile: {LayoutKey}");
    [FaultCase(1)]
    public sealed partial record MissingTile(string TileKey) : ChartFault($"chart/missing-tile: {TileKey}");
    [FaultCase(2)]
    public sealed partial record VisualEmpty(string Reason) : ChartFault($"chart/visual-empty: {Reason}");
    [FaultCase(3)]
    public sealed partial record VisualDegenerate(string Reason) : ChartFault($"chart/visual-degenerate: {Reason}");
    [FaultCase(4)]
    public sealed partial record CrsUnresolved(string FeatureId, int Srid) : ChartFault($"chart/crs: {FeatureId} arrived in SRID {Srid}");
    [FaultCase(5)]
    public sealed partial record LayerRejected(string Layer) : ChartFault($"chart/layer: {Layer}");
    [FaultCase(6)]
    public sealed partial record PayloadMismatch(string Kind, string Payload) : ChartFault($"chart/payload: {Kind} rejected {Payload}");
    [FaultCase(7)]
    public sealed partial record PlacementRejected(string LayoutKey) : ChartFault($"chart/placement: {LayoutKey} is invalid");
    [FaultCase(8)]
    public sealed partial record FilterRejected() : ChartFault("chart/filter: state is invalid");
    [FaultCase(9)]
    public sealed partial record RecordOversize(string Kind, int Bytes, int Ceiling) : ChartFault($"chart/record: {Kind} sealed {Bytes} retained bytes over the {Ceiling} ceiling");
    [FaultCase(10)]
    public sealed partial record PaintUnresolved(string Chrome) : ChartFault($"chart/paint: {Chrome} resolves no generated rung");
    [FaultCase(11)]
    public sealed partial record TransformRejected(string Reason) : ChartFault($"chart/transform: {Reason}");
    [FaultCase(12)]
    public sealed partial record SpecRejected(string Reason) : ChartFault($"chart/spec: {Reason}");
    [FaultCase(13)]
    public sealed partial record ContextRejected(string Reason) : ChartFault($"chart/context: {Reason}");
    [FaultCase(14)]
    public sealed partial record SourceMismatch(string TileKey) : ChartFault($"chart/source: {TileKey} binds a source its case cannot read");
    [FaultCase(15)]
    public sealed partial record ThresholdRejected(string Reason) : ChartFault($"chart/threshold: {Reason}");
    [FaultCase(16)]
    public sealed partial record ProfileRejected(string Reason) : ChartFault($"chart/profile: {Reason}");
    [FaultCase(17)]
    public sealed partial record LegendRejected(string Reason) : ChartFault($"chart/legend: {Reason}");
    [FaultCase(18)]
    public sealed partial record BrushRejected(string Reason) : ChartFault($"chart/brush: {Reason}");
}
```

## [03]-[CHART_PAINTS]

- Owner: `ChartChrome` `[SmartEnum<string>]` the chart chrome role vocabulary, each row addressing a generated `PaintRole` rung by key and carrying its `ChromeFamily` group; `StrokeForm` the closed restyle-shape vocabulary; `ChromeInk` the stroke-fill-text-chip mint column over one restyle body; `PaintFamily` the series-palette family binding one anchor role beside one `Colormap`; `ChartInk` the one resolved paint set; `ChartComposition` the one process-wide registration.
- Cases: `StrokeForm` = fill | stroke | dashed-stroke; `ChromeInk` = Stroke | Fill | Text | Chip | Dashed — the chip arm exists because `ICartesianAxis.CrosshairLabelsBackground` takes an `LvcColor` and never a `Paint`; `PaintFamily` = accent | neutral | magnitude | divergent | cyclic; `ChromeFamily` = grid | tick | crosshair | axis-text | frame | overlay | mark | band | posture | annotation | legend.
- Entry: `ChartInk.Of(ResolvedTheme theme, PaintFamily family, int arity)` — the whole chrome roster plus the series ramp resolved once per board; `ChartInk.Retint(ResolvedTheme next)` — the in-place swap write returning the re-derived ramp beside the same paint instances; `ChartComposition.Register(ChartInk ink, MotionPlan motion, TypographyRole label, SKTypeface typeface)` — the ONE `LiveCharts.Configure` call; `ChartComposition.Reapply(…, Seq<IChartView> mounted)` — the swap half over the already-attached charts; `ChartChrome.Apply(IPlane plane, ChartInk ink, TypographyRole label)` — the one axis-chrome fold every axis mint and the theme's axis rule share.
- Auto: every axis, section, tooltip, legend, label, error bar, and threshold band reads its paint off one `ChartChrome` row, so a chart surface carrying an unresolved colour is unspellable and a new chrome surface is one row; the series ramp derives from the family's `Colormap` through `HeatMap`, so the palette, the heat ramp, and the CVD candidate pairs are three reads of one generation; the CVD candidates pair every adjacent and every non-adjacent ramp entry, so a palette whose neighbours collapse under a deficiency fails the accessibility sweep on the same path the token ladder does.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, SkiaSharp, Avalonia.Skia, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new chart chrome surface is one `ChartChrome` row naming its role, rung, ink, stroke step, alpha, layer, and family; a new palette posture is one `PaintFamily` row; a new restyle shape is one `StrokeForm` row; zero new surface.
- Boundary: `ChartChrome` mints NO token — every row addresses a rung the `Theme/tokens` generation already produces through `PaintRole.At(rung)`, so a chart chrome colour and a control chrome colour are one value and a chart-local paint token is the deleted form; stroke widths read `MetricFamily.Stroke` steps, so the high-contrast projection's stroke gain widens every chart hairline with no chart edit. `ChartInk` holds `SolidColorPaint` instances rather than colours: a `Paint` is a live draw task, so the swap re-runs the row's own restyle over the held instance and every mounted chart re-tints on its next frame with NO re-mount — the `Rematerialize.ChartPaint` roster row names this rebuild, and a chart holding a resolved colour outside this set is the defect that roster's law names. The process `Theme` is a VALUE the package reads at series attach, so the swap re-runs `Register` AND folds `Theme.ApplyStyleToSeries` over every attached series — `GetSeriesColor` indexes `Colors` by `SeriesId`, so the re-apply is deterministic and idempotent; `GetDefaultTooltip`/`GetDefaultLegend` are FACTORIES read at mount, so the swap re-assigns both as property writes; offscreen `SKCharts` twins and sealed captures are PRODUCTS re-rendered rather than re-tinted. `LiveChartsSettings.HasTheme` REPLACES the whole `Theme` and the last `Add*Theme` call wins, so registration is exactly one `LiveCharts.Configure` at the composition root; a second `Configure` from a board, tile, or screen is the deleted form, and a per-control `ChartTheme` override exists only for the offscreen proof twin whose gamut is pinned. A DASH is a `StrokeForm` row rather than a column beside the pigment: the dashed arm writes `SkiaPaint.PathEffect` inside the one restyle both the mint and the swap run, the intervals are the custom plane's `StrokeStyle.Dashed.Intervals(width)` CITED at the resolved width, so a comparison ghost on a chart and the same series drawn on that plane dash identically and a chrome row carrying its own interval roster is the deleted form. Severity ink is the folder `Severity` family's (`Theme/tokens`): `ChartInk.Ink(Severity)`/`Shade(Severity)` resolve severity pigment through the same rung read chrome takes, so a threshold band, a watch badge, and a status chip are one pigment. The three package-colour repacks (`AsLvc`/`AsSk`/`AsColor`) are BYTE transposes between foreign colour structs, never colour-space math — kernel `PerceptualColor` enters only where perception does (the CVD candidate pairs feed the token sweep's perceptual model); routing a byte repack through the perceptual model would compand a value that never left sRGB bytes.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StrokeForm {
    public static readonly StrokeForm Fill = new("fill", stroked: false, dashed: false);
    public static readonly StrokeForm Stroke = new("stroke", stroked: true, dashed: false);
    public static readonly StrokeForm DashedStroke = new("dashed-stroke", stroked: true, dashed: true);

    public bool Stroked { get; }

    public bool Dashed { get; }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChromeInk {
    public static readonly ChromeInk Stroke = new("stroke", StrokeForm.Stroke);
    public static readonly ChromeInk Fill = new("fill", StrokeForm.Fill);
    public static readonly ChromeInk Text = new("text", StrokeForm.Fill);
    public static readonly ChromeInk Chip = new("chip", StrokeForm.Fill);
    public static readonly ChromeInk Dashed = new("dashed", StrokeForm.DashedStroke);

    public StrokeForm Form { get; }

    public SolidColorPaint Mint(SKColor color, float width) => Restyle(new SolidColorPaint(), color, width);

    public SolidColorPaint Restyle(SolidColorPaint paint, SKColor color, float width) {
        paint.Color = color;
        paint.IsStroke = Form.Stroked;
        paint.StrokeThickness = Form.Stroked ? width : 0f;
        paint.PathEffect = Form.Dashed ? new DashEffect(StrokeStyle.Dashed.Intervals(width), 0f) : null;
        return paint;
    }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChromeFamily {
    public static readonly ChromeFamily Grid = new("grid");
    public static readonly ChromeFamily Tick = new("tick");
    public static readonly ChromeFamily Crosshair = new("crosshair");
    public static readonly ChromeFamily AxisText = new("axis-text");
    public static readonly ChromeFamily Frame = new("frame");
    public static readonly ChromeFamily Overlay = new("overlay");
    public static readonly ChromeFamily Mark = new("mark");
    public static readonly ChromeFamily Band = new("band");
    public static readonly ChromeFamily Posture = new("posture");
    public static readonly ChromeFamily Annotation = new("annotation");
    public static readonly ChromeFamily Legend = new("legend");
}

public readonly record struct PaintFacet(PaintRole Role, int Rung, ChromeInk Ink, int Stroke, UnitInterval Alpha, int Layer);

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartChrome {
    public static readonly ChartChrome Separator = new("separator", PaintRole.Separator, rung: 0, ChromeInk.Stroke, stroke: 0, alpha: 1d, layer: 0, ChromeFamily.Grid);
    public static readonly ChartChrome Subseparator = new("subseparator", PaintRole.Separator, rung: 1, ChromeInk.Stroke, stroke: 0, alpha: 0.55d, layer: 0, ChromeFamily.Grid);
    public static readonly ChartChrome Tick = new("tick", PaintRole.Border, rung: 0, ChromeInk.Stroke, stroke: 0, alpha: 1d, layer: 1, ChromeFamily.Tick);
    public static readonly ChartChrome Subtick = new("subtick", PaintRole.Border, rung: 1, ChromeInk.Stroke, stroke: 0, alpha: 0.65d, layer: 1, ChromeFamily.Tick);
    public static readonly ChartChrome Zero = new("zero", PaintRole.Border, rung: 0, ChromeInk.Stroke, stroke: 1, alpha: 1d, layer: 1, ChromeFamily.Tick);
    public static readonly ChartChrome FrameStroke = new("frame-stroke", PaintRole.Border, rung: 0, ChromeInk.Stroke, stroke: 0, alpha: 1d, layer: 0, ChromeFamily.Frame);
    public static readonly ChartChrome FrameFill = new("frame-fill", PaintRole.Panel, rung: 0, ChromeInk.Fill, stroke: 0, alpha: 1d, layer: -1, ChromeFamily.Frame);
    public static readonly ChartChrome AxisName = new("axis-name", PaintRole.TextMuted, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 2, ChromeFamily.AxisText);
    public static readonly ChartChrome AxisLabel = new("axis-label", PaintRole.TextFaint, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 2, ChromeFamily.AxisText);
    public static readonly ChartChrome Crosshair = new("crosshair", PaintRole.Accent, rung: 0, ChromeInk.Stroke, stroke: 0, alpha: 0.85d, layer: 3, ChromeFamily.Crosshair);
    public static readonly ChartChrome CrosshairLabel = new("crosshair-label", PaintRole.AccentText, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 4, ChromeFamily.Crosshair);
    public static readonly ChartChrome CrosshairChip = new("crosshair-chip", PaintRole.Accent, rung: 0, ChromeInk.Chip, stroke: 0, alpha: 1d, layer: 4, ChromeFamily.Crosshair);
    public static readonly ChartChrome TooltipText = new("tooltip-text", PaintRole.Text, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 5, ChromeFamily.Overlay);
    public static readonly ChartChrome TooltipBack = new("tooltip-back", PaintRole.Overlay, rung: 2, ChromeInk.Fill, stroke: 0, alpha: 1d, layer: 5, ChromeFamily.Overlay);
    public static readonly ChartChrome LegendText = new("legend-text", PaintRole.TextMuted, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 5, ChromeFamily.Overlay);
    public static readonly ChartChrome LegendBack = new("legend-back", PaintRole.Panel, rung: 0, ChromeInk.Fill, stroke: 0, alpha: 1d, layer: 5, ChromeFamily.Overlay);
    public static readonly ChartChrome LegendTitle = new("legend-title", PaintRole.Text, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 5, ChromeFamily.Legend);
    public static readonly ChartChrome LegendValue = new("legend-value", PaintRole.TextFaint, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 5, ChromeFamily.Legend);
    public static readonly ChartChrome LegendFrame = new("legend-frame", PaintRole.Border, rung: 1, ChromeInk.Stroke, stroke: 0, alpha: 1d, layer: 5, ChromeFamily.Legend);
    public static readonly ChartChrome DataLabel = new("data-label", PaintRole.TextMuted, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 3, ChromeFamily.Mark);
    public static readonly ChartChrome ErrorBar = new("error-bar", PaintRole.TextFaint, rung: 0, ChromeInk.Stroke, stroke: 0, alpha: 1d, layer: 2, ChromeFamily.Mark);
    public static readonly ChartChrome SectionFill = new("section-fill", PaintRole.Accent, rung: 0, ChromeInk.Fill, stroke: 0, alpha: 0.12d, layer: -1, ChromeFamily.Band);
    public static readonly ChartChrome SectionStroke = new("section-stroke", PaintRole.Border, rung: 1, ChromeInk.Stroke, stroke: 0, alpha: 0.60d, layer: 0, ChromeFamily.Band);
    public static readonly ChartChrome SectionLabel = new("section-label", PaintRole.TextMuted, rung: 0, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 2, ChromeFamily.Band);
    public static readonly ChartChrome Ghost = new("ghost", PaintRole.TextFaint, rung: 0, ChromeInk.Stroke, stroke: 0, alpha: 0.45d, layer: -1, ChromeFamily.Posture);
    public static readonly ChartChrome GhostDash = new("ghost-dash", PaintRole.TextFaint, rung: 0, ChromeInk.Dashed, stroke: 0, alpha: 0.45d, layer: -1, ChromeFamily.Posture);
    public static readonly ChartChrome Annotation = new("annotation", PaintRole.Warning, rung: 0, ChromeInk.Stroke, stroke: 0, alpha: 1d, layer: 3, ChromeFamily.Annotation);
    public static readonly ChartChrome AnnotationLabel = new("annotation-label", PaintRole.Warning, rung: 1, ChromeInk.Text, stroke: 0, alpha: 1d, layer: 4, ChromeFamily.Annotation);
    public static readonly ChartChrome Held = new("held", PaintRole.Surface, rung: 0, ChromeInk.Fill, stroke: 0, alpha: 0.55d, layer: 6, ChromeFamily.Posture);

    public PaintRole Role { get; }

    public int Rung { get; }

    public ChromeInk Ink { get; }

    public int Stroke { get; }

    public UnitInterval Alpha { get; }

    public int Layer { get; }

    public ChromeFamily Family { get; }

    public PaintFacet Facet => new(Role, Rung, Ink, Stroke, Alpha, Layer);

    public static Unit Apply(IPlane plane, ChartInk ink, TypographyRole label) {
        plane.NamePaint = ink.Paint(AxisName);
        plane.LabelsPaint = ink.Paint(AxisLabel);
        plane.SeparatorsPaint = ink.Paint(Separator);
        plane.ShowSeparatorLines = true;
        plane.TextSize = label.Size;
        plane.NameTextSize = label.Size;
        return plane is ICartesianAxis axis ? Cartesian(axis, ink) : unit;
    }

    static Unit Cartesian(ICartesianAxis axis, ChartInk ink) {
        axis.SubseparatorsPaint = ink.Paint(Subseparator);
        axis.SubseparatorsCount = 3;
        axis.DrawTicksPath = true;
        axis.TicksPaint = ink.Paint(Tick);
        axis.SubticksPaint = ink.Paint(Subtick);
        axis.ZeroPaint = ink.Paint(Zero);
        axis.CrosshairPaint = ink.Paint(Crosshair);
        axis.CrosshairLabelsPaint = ink.Paint(CrosshairLabel);
        axis.CrosshairLabelsBackground = ink.Tint(CrosshairChip);
        axis.CrosshairSnapEnabled = true;
        return unit;
    }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaintFamily {
    public static readonly PaintFamily Accent = new("accent", PaintRole.Accent, Colormap.Tableau);
    public static readonly PaintFamily Neutral = new("neutral", PaintRole.TextMuted, Colormap.Tableau);
    public static readonly PaintFamily Magnitude = new("magnitude", PaintRole.Accent, Colormap.Viridis);
    public static readonly PaintFamily Divergent = new("divergent", PaintRole.Accent, Colormap.Coolwarm);
    public static readonly PaintFamily Cyclic = new("cyclic", PaintRole.Accent, Colormap.Twilight);

    public PaintRole Anchor { get; }

    public Colormap Series { get; }
}
```

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed record ChartInk(
    ResolvedTheme Theme,
    PaintFamily Family,
    FrozenDictionary<ChartChrome, SolidColorPaint> Chrome,
    FrozenDictionary<Severity, SolidColorPaint> SeverityInk,
    LvcColor[] Palette,
    SKColor[] Ramp) {
    public static Fin<ChartInk> Of(ResolvedTheme theme, PaintFamily family, int arity) =>
        toSeq(ChartChrome.Items).Traverse(chrome => Mint(theme, chrome.Key, chrome.Facet).Map(paint => (Key: chrome, Paint: paint))).As()
            .Bind(chrome => toSeq(Severity.Items)
                .Traverse(row => Mint(theme, $"{ChartChrome.SectionFill.Key}/{row.Key}", ChartChrome.SectionFill.Facet with { Role = row.Role })
                    .Map(paint => (Key: row, Paint: paint))).As()
                .Map(severity => (Chrome: chrome, Severity: severity)))
            .Bind(sets => family.Series.HeatMap(int.Max(arity, 2), static color => color)
                .Map(stops => (sets.Chrome, sets.Severity, Stops: stops)))
            .Map(resolved => new ChartInk(
                theme,
                family,
                resolved.Chrome.ToFrozenDictionary(static row => row.Key, static row => row.Paint),
                resolved.Severity.ToFrozenDictionary(static row => row.Key, static row => row.Paint),
                resolved.Stops.Map(ChartPigment.AsLvc).ToArray(),
                resolved.Stops.Map(ChartPigment.AsSk).ToArray()));

    public Paint Paint(ChartChrome chrome) => Chrome[chrome];

    public Paint Ink(Severity severity) => SeverityInk[severity];

    public LvcColor Tint(ChartChrome chrome) => Chrome[chrome].Color.AsLvcColor();

    public SKColor Shade(Severity severity) => SeverityInk[severity].Color;

    public Fin<ChartInk> Retint(ResolvedTheme next) =>
        toSeq(ChartChrome.Items).Traverse(chrome => Write(next, chrome.Key, chrome.Facet, Chrome[chrome])).As()
            .Bind(_ => toSeq(Severity.Items)
                .Traverse(row => Write(next, row.Key, ChartChrome.SectionFill.Facet with { Role = row.Role }, SeverityInk[row])).As())
            .Bind(_ => Family.Series.HeatMap(int.Max(Palette.Length, 2), static color => color))
            .Map(stops => this with {
                Theme = next,
                Palette = stops.Map(ChartPigment.AsLvc).ToArray(),
                Ramp = stops.Map(ChartPigment.AsSk).ToArray(),
            });

    public Seq<(Color A, Color B, Cvd Lens)> CvdCandidates =>
        toSeq(Palette) switch {
            var palette => Seq(Cvd.Protanopia, Cvd.Deuteranopia, Cvd.Tritanopia).Bind(lens =>
                palette.Map(static (left, index) => (Left: left, Index: index)).Bind(cell => palette.Skip(cell.Index + 1)
                    .Map(right => (ChartPigment.AsColor(cell.Left), ChartPigment.AsColor(right), lens)))),
        };

    static Fin<SolidColorPaint> Mint(ResolvedTheme theme, string label, PaintFacet facet) =>
        Resolve(theme, facet).Map(cell => Seated(facet.Ink.Mint(cell.Color, cell.Width), facet.Layer))
            .MapFail(_ => (Error)new ChartFault.PaintUnresolved(label));

    static Fin<Unit> Write(ResolvedTheme theme, string label, PaintFacet facet, SolidColorPaint paint) =>
        Resolve(theme, facet).Map(cell => ignore(Seated(facet.Ink.Restyle(paint, cell.Color, cell.Width), facet.Layer)))
            .MapFail(_ => (Error)new ChartFault.PaintUnresolved(label));

    static Fin<(SKColor Color, float Width)> Resolve(ResolvedTheme theme, PaintFacet facet) =>
        (theme.Paint(facet.Role, facet.Rung), theme.Metric(MetricFamily.Stroke, facet.Stroke)) switch {
            ({ IsSome: true, Case: Color pigment }, { IsSome: true, Case: double width }) => Fin.Succ((
                Color: Color.FromArgb((byte)Math.Round(facet.Alpha.Value * pigment.A), pigment.R, pigment.G, pigment.B).ToSKColor(),
                Width: (float)width)),
            _ => Fin.Fail<(SKColor, float)>(new ChartFault.PaintUnresolved($"{facet.Role.Key}+{facet.Rung}")),
        };

    static SolidColorPaint Seated(SolidColorPaint paint, int layer) { paint.ZIndex = layer; return paint; }
}

public static class ChartPigment {
    public static LvcColor AsLvc(Color color) => new(color.R, color.G, color.B, color.A);

    public static SKColor AsSk(Color color) => color.ToSKColor();

    public static Color AsColor(LvcColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class ChartComposition {
    public static Fin<Unit> Register(ChartInk ink, MotionPlan motion, TypographyRole label, SKTypeface typeface) =>
        Try.lift(() => {
            LiveCharts.Configure(settings => settings
                .AddSkiaSharp()
                .AddDefaultMappers()
                .HasGlobalSKTypeface(typeface)
                .WithAnimationsSpeed(motion.EnterToken.ChartSpeed)
                .WithEasingFunction(progress => (float)motion.EnterToken.Curve(progress))
                .WithUpdateThrottlingTimeout(motion.EnterToken.ChartSpeed)
                .WithZoomMode(ChartNav.TimeScroll.Mode)
                .AddDarkTheme(theme => Seed(theme, ink, label))
                .HasRenderingSettings(rendering => {
                    rendering.TryUseVSync = true;
                    rendering.ShowFPS = false;
                }));
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner);

    static void Seed(Theme theme, ChartInk ink, TypographyRole label) {
        theme.Colors = ink.Palette;
        theme.VirtualBackroundColor = ink.Tint(ChartChrome.FrameFill);
        theme.TooltipTextPaint = ink.Paint(ChartChrome.TooltipText);
        theme.TooltipBackgroundPaint = ink.Paint(ChartChrome.TooltipBack);
        theme.TooltipTextSize = label.Size;
        theme.LegendTextPaint = ink.Paint(ChartChrome.LegendText);
        theme.LegendBackgroundPaint = ink.Paint(ChartChrome.LegendBack);
        theme.LegendTextSize = label.Size;
        _ = theme
            .HasRuleForAxes(plane => ChartChrome.Apply(plane, ink, label))
            .HasRuleForDrawMarginFrame(
                static () => new CoreDrawMarginFrame(),
                frame => {
                    frame.Stroke = ink.Paint(ChartChrome.FrameStroke);
                    frame.Fill = ink.Paint(ChartChrome.FrameFill);
                })
            .HasRuleForAnySeries(series => {
                series.IsVisibleAtLegend = true;
                series.IsHoverable = true;
            })
            .HasDefaultTooltip(() => new SKDefaultTooltip { Easing = theme.EasingFunction, AnimationsSpeed = theme.AnimationsSpeed })
            .HasDefaultLegend(() => new SKDefaultLegend { Easing = theme.EasingFunction, AnimationsSpeed = theme.AnimationsSpeed });
    }

    public static Fin<Unit> Reapply(ChartInk ink, MotionPlan motion, TypographyRole label, SKTypeface typeface, Seq<IChartView> mounted) =>
        Register(ink, motion, label, typeface).Map(_ => {
            Theme theme = LiveCharts.DefaultSettings.GetTheme();
            mounted.Iter(chart => {
                toSeq(chart.Series).Iter(theme.ApplyStyleToSeries);
                chart.Tooltip = theme.GetDefaultTooltip();
                chart.Legend = theme.GetDefaultLegend();
            });
            return unit;
        });
}
```

## [04]-[THRESHOLD_FAMILY]

- Owner: `ThresholdList` — the ordered base-plus-steps family; `ThresholdStep` — one crossing and the severity above it; `ThresholdBasis` — absolute against percentage-of-range; `ThresholdMode` — the render surface a list projects onto, each row carrying its consumer as a column.
- Cases: `ThresholdBasis` = absolute | percentage; `ThresholdMode` = axis-band | gauge-fill | cell-background | state-region.
- Entry: `ThresholdList.Admit(Severity floor, Seq<ThresholdStep> steps, ThresholdBasis basis, ThresholdMode mode)` — ascending, finite, and in-range or refused; `At(double value, double floor, double ceiling)` — the one classification every renderer reads; `Bands(double floor, double ceiling, BandAxis axis, int scalesAt)` — the axis-band projection over the axes page's orientation row; `Fills(ChartInk ink, double floor, double ceiling)` — ordered gauge stops; `Cell(ChartInk ink, double value, double floor, double ceiling)` — the one cell colour; `Edges(double floor, double ceiling)` — the covering band set every projection and the stepped legend read.
- Auto: one list drives an axis band set, a gauge fill ramp, a table cell background, and an alert state region, so a value classified amber on a chart is amber in the table cell and amber in the alert badge by construction; percentage-basis lists re-derive their crossings from whatever range the surface carries, so one authored list serves a gauge scaled zero-to-one and an axis scaled in absolute units.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new crossing is one `ThresholdStep`; a new presentation is one `ThresholdMode` row naming its consumer and its projection member; zero new surface.
- Boundary: the list is ORDERED and its base is the severity BELOW the first crossing, so a value's severity is the last step it cleared and a gap between steps is unrepresentable — a per-panel threshold block re-authoring its own crossings is what a shared list value forecloses. A percentage-basis list carries crossings in the unit interval and resolves them against the surface's own floor and ceiling at read; an absolute list carries measured values and ignores the surface range, which is what a physical limit needs. Axis bands project through the `BandAxis` row, so a horizontal compliance band and a vertical phase band are one list at two orientations and each carries its severity's own edge caption — a colour with no stated meaning is unspellable, which is why the caption column is total rather than a knob. Severity ink resolves through `ChartInk.Shade` off the `Severity` row, so a threshold band, a watch badge, and a status chip are one pigment and a threshold-local colour column is the deleted form. The tables-side receiving boundary for `Cell` is the value-driven format column on `TableColumnRow`; the stepped legend reads `Edges` — both consumers are named on the mode row itself, so the projection-to-consumer correspondence is fence data rather than a hand table.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct ThresholdStep(double At, Severity Severity);

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThresholdBasis {
    public static readonly ThresholdBasis Absolute = new("absolute", static (at, _, _) => at);
    public static readonly ThresholdBasis Percentage = new("percentage", static (at, floor, ceiling) => floor + (at * (ceiling - floor)));

    [UseDelegateFromConstructor]
    public partial double Resolve(double at, double floor, double ceiling);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThresholdMode {
    public static readonly ThresholdMode AxisBand = new("axis-band", consumer: "ChartSpec.Sections at a named axis index");
    public static readonly ThresholdMode GaugeFill = new("gauge-fill", consumer: "gauge background items behind the value arc");
    public static readonly ThresholdMode CellBackground = new("cell-background", consumer: "TableColumnRow value-driven format column");
    public static readonly ThresholdMode StateRegion = new("state-region", consumer: "tile status badge and WatchRule severity");

    public string Consumer { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ThresholdList(
    Severity Floor,
    Seq<ThresholdStep> Steps,
    ThresholdBasis Basis,
    ThresholdMode Mode) {
    public static Fin<ThresholdList> Admit(Severity floor, Seq<ThresholdStep> steps, ThresholdBasis basis, ThresholdMode mode) =>
        steps.ForAll(static step => double.IsFinite(step.At))
            && steps.Zip(steps.Skip(1)).ForAll(static pair => pair.First.At < pair.Second.At)
            && (basis != ThresholdBasis.Percentage || steps.ForAll(static step => step.At is >= 0d and <= 1d))
            ? Fin.Succ(new ThresholdList(floor, steps, basis, mode))
            : Fin.Fail<ThresholdList>(new ChartFault.ThresholdRejected(mode.Key));

    public Severity At(double value, double floor, double ceiling) =>
        Steps.Fold(Floor, (held, step) => value >= Basis.Resolve(step.At, floor, ceiling) ? step.Severity : held);

    public Fin<Seq<ChartSection>> Bands(double floor, double ceiling, BandAxis axis, int scalesAt) =>
        Mode != ThresholdMode.AxisBand
            ? Fin.Fail<Seq<ChartSection>>(new ChartFault.ThresholdRejected($"{Mode.Key}: not a band mode"))
            : Fin.Succ(Edges(floor, ceiling).Map(edge =>
                axis.Section(edge.From, edge.To, ChartChrome.SectionFill, Tint(edge.Severity), Some(edge.Severity.Key), scalesAt)));

    public Fin<Seq<(double At, SKColor Fill)>> Fills(ChartInk ink, double floor, double ceiling) =>
        Mode != ThresholdMode.GaugeFill
            ? Fin.Fail<Seq<(double, SKColor)>>(new ChartFault.ThresholdRejected($"{Mode.Key}: not a fill mode"))
            : Fin.Succ(Edges(floor, ceiling).Map(edge => (edge.From, ink.Shade(edge.Severity))));

    public Fin<SKColor> Cell(ChartInk ink, double value, double floor, double ceiling) =>
        Mode != ThresholdMode.CellBackground
            ? Fin.Fail<SKColor>(new ChartFault.ThresholdRejected($"{Mode.Key}: not a cell mode"))
            : Fin.Succ(ink.Shade(At(value, floor, ceiling)));

    public Seq<(double From, double To, Severity Severity)> Edges(double floor, double ceiling) =>
        Steps.Fold(
            (From: floor, Severity: Floor, Acc: Seq<(double, double, Severity)>()),
            (state, step) => Basis.Resolve(step.At, floor, ceiling) switch {
                var at => (From: at, Severity: step.Severity, Acc: state.Acc.Add((state.From, at, state.Severity))),
            }) switch {
            var tail => tail.Acc.Add((tail.From, ceiling, tail.Severity)),
        };

    Option<Severity> Tint(Severity severity) => severity == Floor ? None : Some(severity);
}
```

## [05]-[CONSTRAINT_PROFILE]

- Owner: `BoundDirection` — the comparison vocabulary carrying predicate, margin, and driver as row data; `ConstraintRow` — one saved check; `ConstraintProfile` — the named, project-scoped, generation-sealed check set carrying its own grading list and the verdict folds; `ConstraintVerdict` — one row's reading.
- Cases: `BoundDirection` = at-most | at-least | within.
- Entry: `ConstraintRow.Admit(candidate)` — accumulating admission over key, metric, bound, ceiling, and source arm; `ConstraintRow.Read(measured, grade)` — the one comparison; `ConstraintRow.Chip(verdict, locale)` — the verdict text a scorecard cell and a table chip both print; `ConstraintProfile.Admit(candidate)` — row keys, grading basis, and per-row admissions accumulated; `ConstraintProfile.Reading(verdicts)` — the worst-of badge; `ConstraintProfile.Pressure(verdicts, keep)` — the kernel `Ranked.Top` pressure ordering a truncated card keeps; `ConstraintProfile.Seal` with `Save()` / `Open(blob)` — the sealed round-trip whose admission arrow is `Admit` itself.
- Auto: a constraint profile grades every row through its one percentage-basis list, so a zoning check, a code check, and a budget check read one severity ladder and one ink; every admission accumulates ALL defects through the `Validation` applicative, so a profile with three bad rows names three rows in one refusal.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new check is one `ConstraintRow`; a new comparison sense is one `BoundDirection` row with its predicate, margin, and driver columns; zero new surface.
- Boundary: the profile NEVER computes a metric — every row names a scalar `TileSource` arm and the scorecard subscribes it through the same live-data scalar-fold edge a stat tile takes, so a gross-floor-area check and the area readout beside it are one number. Comparison is unit-safe by CONSTRUCTION: bound and measured value both stand in the role's own metric unit — the canonical storage unit every measured feed writes — and the display unit is ELECTED at render exactly as an axis title's is, so a row carrying a transcribed unit abbreviation is unspellable. MARGIN is signed distance in that metric unit; RATIO is that distance as a fraction of the bound's magnitude and is an `Option` — a zero-magnitude bound admits no fraction, and the retired fallback that published the raw margin in the ratio column handed the one cross-metric-comparable column a value in the role's own unit (`RULINGS [02]` ratio law); the zero test reads `EpsilonPolicy.ZeroTolerance`, never `double.Epsilon`, which is the denormal floor and no tolerance at all. The DRIVER names which edge broke; a passing row's driver is empty, since naming a cause on a pass invents one. `Passes` is a DERIVATION of the margin's sign, never a stored column a mutation could desynchronize. Severity is the profile's ONE `ThresholdList` read at the shortfall under a percentage basis; a ratio-less breach grades at the ladder's deepest step because an unscalable breach cannot be ranked shallow, and a ratio-less pass grades at the floor. Ranking reads kernel `Ranked.Top` over ascending ratio so the closest-to-breach rows rise and a truncated card keeps exactly the rows a designer must see; ratio-less rows seat last. A profile is a SAVED artifact crossing the one composition-seated `EvidenceOps.Wire` through its own `Diagnostics/evidence#DURABLE_PARCEL` seal, whose ceiling and generation compare are the shared owner's; a parcel the generation refuses HOLDS its bytes beside the seeded profile, because a compliance set is authored and its rows stay readable by hand. NAMED LOSS: attribute-rename carry — a row column renamed across a generation reaches no reader under the next. `ConstraintVerdict` is DISTINCT from Fabrication's nesting `ConstraintVerdict` — this row grades a live metric against a saved compliance bound; the NFP row witnesses a geometric non-fit — the shared name carries no shared regime.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoundDirection {
    public static readonly BoundDirection AtMost = new("at-most",
        static (value, bound, ceiling) => bound - value,
        static (value, bound, ceiling) => value > bound ? "above" : string.Empty);
    public static readonly BoundDirection AtLeast = new("at-least",
        static (value, bound, ceiling) => value - bound,
        static (value, bound, ceiling) => value < bound ? "below" : string.Empty);
    public static readonly BoundDirection Within = new("within",
        static (value, bound, ceiling) => Math.Min(value - bound, ceiling - value),
        static (value, bound, ceiling) => value < bound ? "below" : value > ceiling ? "above" : string.Empty);

    [UseDelegateFromConstructor]
    public partial double Margin(double value, double bound, double ceiling);

    [UseDelegateFromConstructor]
    public partial string Driver(double value, double bound, double ceiling);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ConstraintRow(
    string Key,
    string Label,
    string Metric,
    BoundDirection Direction,
    double Bound,
    double Ceiling,
    MeasureRole Measure,
    TileSource Source) {
    public static Fin<ConstraintRow> Admit(ConstraintRow candidate) =>
        (Gate(!string.IsNullOrWhiteSpace(candidate.Key), $"row/{candidate.Key}: blank key"),
         Gate(!string.IsNullOrWhiteSpace(candidate.Metric), $"row/{candidate.Key}: blank metric"),
         Gate(double.IsFinite(candidate.Bound), $"row/{candidate.Key}: non-finite bound"),
         Gate(candidate.Direction != BoundDirection.Within || (double.IsFinite(candidate.Ceiling) && candidate.Ceiling > candidate.Bound),
             $"row/{candidate.Key}: within needs ceiling above bound"),
         Gate(candidate.Source.Arm == SourceArm.Scalar, $"row/{candidate.Key}: source answers no scalar"))
            .Apply((_, _, _, _, _) => candidate).As().ToFin();

    public ConstraintVerdict Read(double measured, ThresholdList grade) =>
        Direction.Margin(measured, Bound, Ceiling) switch {
            var margin => (Margin: margin,
                           Ratio: Math.Abs(Bound) > EpsilonPolicy.ZeroTolerance ? Some(margin / Math.Abs(Bound)) : Option<double>.None) switch {
                var read => new ConstraintVerdict(
                    Key,
                    Value: measured,
                    Margin: read.Margin,
                    Ratio: read.Ratio,
                    Severity: read.Ratio.Match(
                        Some: ratio => grade.At(-ratio, 0d, 1d),
                        None: () => read.Margin >= 0d
                            ? grade.Floor
                            : grade.Steps.LastOrNone().Map(static step => step.Severity).IfNone(grade.Floor)),
                    Driver: Direction.Driver(measured, Bound, Ceiling)),
            },
        };

    public Fin<string> Chip(ConstraintVerdict verdict, ResolvedLocale locale) =>
        Quantity.TryFrom(verdict.Margin, Measure.MetricUnit, out IQuantity? margin) && margin is not null
            ? locale.Quantity(margin, Measure).Bind(spelled => locale.Message(
                verdict.Passes ? PassStem : FailStem,
                ("label", Label), ("margin", spelled), ("driver", verdict.Driver)))
            : Fin.Fail<string>(new ChartFault.ProfileRejected($"row/{Key}: {Measure.Key} admits no quantity"));

    static string PassStem => LocaleStrings.Key(nameof(ConstraintVerdict), "pass");

    static string FailStem => LocaleStrings.Key(nameof(ConstraintVerdict), "fail");

    static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? unit : (Validation<Error, Unit>)(Error)new ChartFault.ProfileRejected(detail);
}

public readonly record struct ConstraintVerdict(
    string RowKey, double Value, double Margin, Option<double> Ratio, Severity Severity, string Driver) {
    public bool Passes => Margin >= 0d;
}

public sealed record ConstraintProfile(
    string Key,
    string Label,
    string Project,
    ThresholdList Grade,
    Seq<ConstraintRow> Rows) {
    public static readonly StateSeal Seal = StateSeal.Of("chart", "profile", generation: 1, StateResidue.Hold);

    public static Fin<ConstraintProfile> Admit(ConstraintProfile candidate) =>
        (Gate(!string.IsNullOrWhiteSpace(candidate.Key), $"{candidate.Key}: blank key"),
         Gate(!string.IsNullOrWhiteSpace(candidate.Project), $"{candidate.Key}: blank project"),
         Gate(!candidate.Rows.IsEmpty, $"{candidate.Key}: empty row set"),
         Gate(candidate.Rows.Map(static row => row.Key).Distinct().Count == candidate.Rows.Count, $"{candidate.Key}: duplicate row keys"),
         Gate(candidate.Grade.Basis == ThresholdBasis.Percentage, $"{candidate.Key}: grade must be percentage-basis"))
            .Apply((_, _, _, _, _) => candidate).As().ToFin()
            .Bind(static admitted => admitted.Rows.Traverse(ConstraintRow.Admit).As().Map(rows => admitted with { Rows = rows }));

    public Severity Reading(Seq<ConstraintVerdict> verdicts) =>
        Severity.Worst(verdicts, static verdict => verdict.Severity);

    public Seq<ConstraintVerdict> Pressure(Seq<ConstraintVerdict> verdicts, int keep) =>
        Rasm.Domain.Ranked.Top(verdicts, keep, static verdict => verdict.Ratio.IfNone(double.PositiveInfinity), ExtremumDirection.Minimum);

    public Fin<string> Save() => Seal.Write(this);

    public static Restored<ConstraintProfile> Open(string blob) => Seal.Read<ConstraintProfile>(blob, Admit);

    static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? unit : (Validation<Error, Unit>)(Error)new ChartFault.ProfileRejected(detail);
}
```

## [06]-[RESEARCH]

(none)
