# [APPUI_CHARTS_GRAMMAR]

The chart grammar: `ChartDatum` is the canonical point every feed reduces to over the allocation-free `ChartMagnitude` carrier, `ChartEncoding` the coordinate-arity vocabulary, `ChartLayer` one series binding inside a tile, `CompareOffset` the comparison-ghost posture owning its one expansion, `FacetSpec` the small-multiples partition owning its member fold, `ChartSpec` the whole per-tile declaration whose applicative admission proves canvas agreement, axis indices, mark names, and chain arity in one refusal, `ChartPolicy` the typed interaction posture, and `ChartSync` the one lock-and-pair mount. The kind catalog, the axis-and-mark plane, and the legend declaration seat HERE beside the spec that admits them — one declaration grammar, one page.

## [01]-[INDEX]

- [02]-[CANONICAL_DATUM]: The point, the magnitude carrier, encodings, label rows, anchors, navigation, finding.
- [03]-[LAYER_AND_SPEC]: Layers, ghosts, facets, the spec, admission, expansion, materialization.
- [04]-[SYNC_AND_POLICY]: The policy value, the group locks, the paired-axis and interaction writes.
- [05]-[SERIES_TABLE]: Seventeen kind rows; canvas dispatch; dress traits; render-hash baselines.
- [06]-[GEO_OVERLAY]: The live land-swap mount and its keyed reason fold.
- [07]-[AXES]: Scale shells; the axis declaration; orientation; band sections.
- [08]-[ANNOTATION_PLANE]: Mark classes; clustering; the two materializations.
- [09]-[LEGEND_VOCABULARY]: The domain union, the statistics column, the dock, and the declaration.
- [10]-[LEGEND_FOLD]: The one entry resolution, the arm dispatch, and the drag writes.

## [02]-[CANONICAL_DATUM]

- Owner: `ChartMagnitude` — five inline slots covering every coordinate arity the package's point model admits; `ChartDatum` — the one row shape between every feed and every series; `ChartEncoding` — coordinate arity as data; `DataLabel` — the label-source row family; `ChartAnchor` — the one anchor vocabulary carrying BOTH package positions as columns; `ChartNav` and `ChartFind` — navigation and hit-test postures as package-enum rows.
- Cases: `ChartEncoding` = xy | weighted | financial | summary | bounded; `DataLabel` = value | caption; `ChartAnchor` = hidden, top, bottom, left, right, auto; `ChartNav` = fixed | time-scroll | value-scroll | free; `ChartFind` = automatic | shared-x | shared-y | exact | nearest.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new label source is one `DataLabel` row; a new coordinate arity is one `ChartEncoding` row; a new navigation or hit-test posture is one row carrying the package value; zero new surface.
- Boundary: `ChartMagnitude.Of` takes ONE span — the populated slot count IS the arity, so five arity twins collapse into one entry and a sixth slot is a struct column, not an overload. `ChartAnchor` carries the package's `TooltipPosition` AND `LegendPosition` as row columns (the legend enum carries no `Auto`, so the auto row lands on the shipped side — the one place the two vocabularies genuinely differ in cardinality); the two five-arm ternary ladders that re-derived the mapping at the bind edge are row reads. A data LABEL is a source row rather than a switch: the row carries the projection AND the chrome its text takes, the value row renders through the locale's own numeric formatter off the settled numeric-axis `CompositeFormat` so a data label and the axis tick beside it print one decimal separator, and neither row touches `ChartPoint.AsDataLabel` — that property resolves through `GetDataLabelText`, which calls the very formatter being defined and recurses until the stack ends.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct ChartMagnitude(double A, double B, double C, double D, double E) {
    public static ChartMagnitude Of(ReadOnlySpan<double> slots) => new(
        slots.Length > 0 ? slots[0] : 0d,
        slots.Length > 1 ? slots[1] : 0d,
        slots.Length > 2 ? slots[2] : 0d,
        slots.Length > 3 ? slots[3] : 0d,
        slots.Length > 4 ? slots[4] : 0d);

    public double this[int slot] => slot switch { 0 => A, 1 => B, 2 => C, 3 => D, _ => E };
}

public readonly record struct ChartDatum(
    double X,
    ChartMagnitude Value,
    int Arity,
    double Weight,
    string Group,
    Option<Instant> Stamp) {
    public static ChartDatum Point(double x, double y, string group = "", Option<Instant> stamp = default) =>
        new(x, ChartMagnitude.Of([y]), 1, 1d, group, stamp);

    public static ChartDatum Of(double x, ChartMagnitude value, int arity, double weight, string group, Option<Instant> stamp) =>
        new(x, value, arity, weight, group, stamp);

    public StatSample Sample => new(Value.A, Weight);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartEncoding {
    public static readonly ChartEncoding Xy = new("xy", arity: 1, static (x, v) => new Coordinate(x, v.A));
    public static readonly ChartEncoding Weighted = new("weighted", arity: 2, static (x, v) => new Coordinate(x, v.A, v.B));
    public static readonly ChartEncoding Financial = new("financial", arity: 4, static (x, v) => new Coordinate(x, v.A, v.B, v.C, v.D));
    public static readonly ChartEncoding Summary = new("summary", arity: 5, static (x, v) => new Coordinate(x, v.A, v.B, v.C, v.D, v.E));
    public static readonly ChartEncoding Bounded = new("bounded", arity: 3,
        static (x, v) => new Coordinate(v.A, x, 0d, 0d, 0d, 0d, new Error(0d, 0d, v.B, v.C)));

    public int Arity { get; }

    [UseDelegateFromConstructor]
    public partial Coordinate Point(double x, ChartMagnitude value);

    public Coordinate Of(ChartDatum datum) => datum.Arity >= Arity ? Point(datum.X, datum.Value) : Coordinate.Empty;
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DataLabel {
    public static readonly DataLabel Value = new("value", ChartChrome.DataLabel,
        static (point, locale) => locale.Text(ChartAxisKind.Numeric.Format, point.Coordinate.PrimaryValue));
    public static readonly DataLabel Caption = new("caption", ChartChrome.AnnotationLabel,
        static (point, _) => point.Context.DataSource is ChartDatum datum ? datum.Group : string.Empty);

    public ChartChrome Ink { get; }

    [UseDelegateFromConstructor]
    public partial string Text(ChartPoint point, ResolvedLocale locale);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartAnchor {
    public static readonly ChartAnchor Hidden = new("hidden", TooltipPosition.Hidden, LegendPosition.Hidden);
    public static readonly ChartAnchor Top = new("top", TooltipPosition.Top, LegendPosition.Top);
    public static readonly ChartAnchor Bottom = new("bottom", TooltipPosition.Bottom, LegendPosition.Bottom);
    public static readonly ChartAnchor Left = new("left", TooltipPosition.Left, LegendPosition.Left);
    public static readonly ChartAnchor Right = new("right", TooltipPosition.Right, LegendPosition.Right);
    public static readonly ChartAnchor Auto = new("auto", TooltipPosition.Auto, LegendPosition.Right);

    public TooltipPosition Tooltip { get; }

    public LegendPosition Legend { get; }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartNav {
    public static readonly ChartNav Fixed = new("fixed", ZoomAndPanMode.None);
    public static readonly ChartNav TimeScroll = new("time-scroll", ZoomAndPanMode.X);
    public static readonly ChartNav ValueScroll = new("value-scroll", ZoomAndPanMode.Y);
    public static readonly ChartNav Free = new("free", ZoomAndPanMode.Both);

    public ZoomAndPanMode Mode { get; }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartFind {
    public static readonly ChartFind Automatic = new("automatic", FindingStrategy.Automatic);
    public static readonly ChartFind SharedX = new("shared-x", FindingStrategy.CompareOnlyXTakeClosest);
    public static readonly ChartFind SharedY = new("shared-y", FindingStrategy.CompareOnlyYTakeClosest);
    public static readonly ChartFind Exact = new("exact", FindingStrategy.ExactMatch);
    public static readonly ChartFind Nearest = new("nearest", FindingStrategy.CompareAllTakeClosest);

    public FindingStrategy Strategy { get; }
}
```

## [03]-[LAYER_AND_SPEC]

- Owner: `LayerTrait` — the layer capability vocabulary; `ChartLayer` — one series binding; `CompareOffset` — the comparison posture owning its expansion; `FacetSpec` with `FacetAxis` — the partition declaration owning its member fold, placement, and cursor sync; `ChartSpec` — the whole per-tile chart with its applicative admission, its one expansion, and its one materialization.
- Cases: `LayerTrait` = errors | toggleable; `CompareOffset` = Period | Ordinal | Scenario; `FacetAxis` = Grouped | Calendar.
- Entry: `ChartSpec.Admit(ChartSpec candidate)` — the whole-spec shape law under one applicative, so a spec wrong on five gates names all five; `ChartSpec.Expand(ResolvedLocale locale)` — comparison ghosts and annotation marks folded into the layer list once, ahead of every materialization; `ChartSpec.Materialize(ChartInk ink, TypographyRole label, ResolvedLocale locale)` — the series mint over the expanded list; `FacetSpec.Partition(ChartSpec spec, Seq<ChartDatum> rows, CalendarPolicy calendar, ResolvedLocale locale)` — the member split with its cap and honest overflow.
- Auto: one datum shape feeds every kind, so a stacked mix, a dual-axis overlay, a categorical bar beside a line, and a ghost comparison are four layer rows on one spec; the encoding arity is checked ONCE at admission against the chain's declared output, so a per-point arity branch never runs; a comparison declaration mints its own aligned ghost and a facet declaration replicates the whole layer list per member.
- Packages: PanAndZoom, LiveChartsCore.SkiaSharpView.Avalonia, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new visualization on a tile is one `ChartLayer` row; a new comparison posture is one `CompareOffset` arm; a new partition is one `FacetAxis` arm; a new layer capability is one `LayerTrait` row read at the dress fold; zero new surface.
- Boundary: COMPARISON is a posture on the layer it shadows, never a view — `Compare` mints a ghost of that same layer on the same axes, the ALIGNMENT is a `TransformRow.Shift` appended to the ghost's own chain (a declared reshape the evaluator already runs, replayable offscreen with no live feed), and the offset is an EXPRESSION, never a wall-clock instant: `Period` shifts by a duration `TimeRange.Shifted` also understands, `Ordinal` by index for members that are positions, `Scenario` re-binds the stream under a named board-variable member. Ghost presentation is `ChartChrome.GhostDash` at reduced alpha one layer below its host — alpha alone loses which run a reader is on exactly at the overlap, which is the one place a comparison is actually read — and the ghost is untoggleable on its host's axes, because a shadow a viewer can leave visible while its subject is hidden reads as data. FACETING replicates the whole layer list rather than minting a chart kind: each member takes a COPY of the spec under a member-suffixed key carrying the parent policy's `ScaleGroup`, so `ChartSync.Pair` shares one min-max across the grid and the legend stays the PARENT tile's single declaration — a shared facet DECLARES its group on the policy (one authority; the auto-minted private group is gone, and its loss is exactly that a shared-scale facet now names its group where the fold once invented one). Cursor sync rides `InvalidateCrosshair`/`ClearCrosshair` over the member set, because a shared range pairs the SCALE and never the pointer. The CAP is a rendering bound with an honest overflow: members past it fold into ONE residual member whose caption carries the count through the locale — twelve cells and a thirteenth reading the rest, where a silent truncation renders twelve cells and a lie. Facet placement is the SAME `PlacementFlow.Flow` fold the board runs over a tile-local `PlacementGrid`. `ChartSpec.AnnotationTolerance` is a bare axis-space scalar BY DECLARATION — the tolerance lives in the measure the axis renders, not in model space, so it composes no kernel tolerance lane and the discriminant is stated here.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerTrait : ICapability<LayerTrait> {
    public static readonly LayerTrait Errors = new(key: "errors");
    public static readonly LayerTrait Toggleable = new(key: "toggleable");
}

public sealed record ChartLayer(
    string Name,
    ChartSeriesKind Kind,
    ChartStream Stream,
    Seq<TransformRow> Transforms,
    int ScalesXAt,
    int ScalesYAt,
    Option<PaintFamily> Family,
    Option<ChartChrome> Ink,
    Option<DataLabel> Labels,
    Option<CompareOffset> Compare,
    Seq<ChartDatum> Pinned,
    CapabilitySet<LayerTrait> Traits,
    int Layer) {
    public static ChartLayer Of(string name, ChartSeriesKind kind, ChartStream stream, params TransformRow[] transforms) =>
        new(name, kind, stream, toSeq(transforms), 0, 0, None, None, None, None, Seq<ChartDatum>(),
            CapabilitySet<LayerTrait>.Of(LayerTrait.Toggleable), 0);

    public bool Literal => !Pinned.IsEmpty;

    public Fin<ChartShape> Shape() =>
        Literal ? Fin.Succ(ChartShape.Series) : TransformChain.Admit(Stream.Shape + Transforms, ChartShape.Series);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CompareOffset {
    private CompareOffset() { }
    public sealed record Period(Duration Back) : CompareOffset;
    public sealed record Ordinal(int Steps) : CompareOffset;
    public sealed record Scenario(string VariableKey, string Member) : CompareOffset;

    public Option<TransformRow> Alignment => Switch(
        period: static row => Some<TransformRow>(new TransformRow.Shift(row.Back, 0)),
        ordinal: static row => Some<TransformRow>(new TransformRow.Shift(Duration.Zero, row.Steps)),
        scenario: static _ => Option<TransformRow>.None);

    public string Suffix => Switch(
        period: static row => $"−{row.Back}",
        ordinal: static row => $"−{row.Steps}",
        scenario: static row => row.Member);

    public Seq<ChartLayer> Expand(ChartLayer host) =>
        Seq(host, host with {
            Name = $"{host.Name}:{Suffix}",
            Transforms = host.Transforms + toSeq(Alignment),
            Ink = Some(ChartChrome.GhostDash),
            Labels = None,
            Compare = None,
            Traits = host.Traits.Admits(LayerTrait.Errors) ? CapabilitySet<LayerTrait>.Of(LayerTrait.Errors) : CapabilitySet<LayerTrait>.None,
            Layer = host.Layer - 1,
            Stream = this is Scenario scenario
                ? host.Stream with { Key = $"{host.Stream.Key}:{scenario.VariableKey}={scenario.Member}" }
                : host.Stream,
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FacetAxis {
    private FacetAxis() { }
    public sealed record Grouped() : FacetAxis;
    public sealed record Calendar(CalendarAxis Axis) : FacetAxis;

    public Option<string> Member(ChartDatum datum, CalendarPolicy calendar) => Switch(
        state: (Datum: datum, Calendar: calendar),
        grouped: static (s, _) => string.IsNullOrEmpty(s.Datum.Group) ? None : Some(s.Datum.Group),
        calendar: static (s, row) => s.Datum.Stamp.Map(stamp => row.Axis.Group(s.Calendar.Civil(stamp))));
}

public sealed record FacetSpec(FacetAxis On, int Columns, int Cap, int RowSpan) {
    public static Fin<FacetSpec> Admit(FacetSpec candidate) =>
        candidate.Columns > 0 && candidate.Cap > 0 && candidate.RowSpan > 0
            ? Fin.Succ(candidate)
            : Fin.Fail<FacetSpec>(new ChartFault.SpecRejected("facet/bounds"));

    public static string OverflowStem => LocaleStrings.Key(nameof(FacetSpec), "overflow");

    public static Fin<Seq<(string Member, ChartSpec Spec, Seq<ChartDatum> Rows)>> Partition(
        ChartSpec spec, Seq<ChartDatum> rows, CalendarPolicy calendar, ResolvedLocale locale) =>
        spec.Facet.Match(
            Some: facet => Admit(facet).Bind(admitted => Members(admitted, rows, calendar, locale)
                .Map(members => members.Map(member => (member.Member, Member(spec, member.Member), member.Rows)))),
            None: () => Fin.Succ(Seq((spec.Key, spec, rows))));

    static Fin<Seq<(string Member, Seq<ChartDatum> Rows)>> Members(
        FacetSpec facet, Seq<ChartDatum> rows, CalendarPolicy calendar, ResolvedLocale locale) =>
        rows.Choose(datum => facet.On.Member(datum, calendar).Map(member => (Member: member, Datum: datum))) switch {
            var keyed => toSeq(keyed.GroupBy(static row => row.Member, StringComparer.Ordinal))
                .Map(group => (Member: group.Key, Rows: toSeq(group).Map(static row => row.Datum))) switch {
                var members when members.Count <= facet.Cap => Fin.Succ(members),
                var members => locale.Message(OverflowStem, ("count", members.Count - facet.Cap))
                    .Map(caption => members.Take(facet.Cap)
                        .Add((Member: caption, Rows: members.Skip(facet.Cap).Bind(static row => row.Rows)))),
            },
        };

    static ChartSpec Member(ChartSpec spec, string member) =>
        spec with {
            Key = $"{spec.Key}:{member}",
            Facet = None,
            Legend = None,
            Policy = spec.Policy with { Legend = ChartAnchor.Hidden },
        };

    public Seq<TilePlacement> Place(BreakpointRow at, Seq<string> members) =>
        PlacementFlow.Flow(new PlacementGrid(at, Columns), members, span: 1, rowSpan: RowSpan, from: 0).Placements;

    public static Unit Cursor(Seq<(SourceGenCartesianChart Chart, ICartesianAxis Axis)> members, Option<LvcPoint> at) =>
        members.Fold(unit, (_, member) => at.Match(
            Some: point => fun(() => member.Axis.InvalidateCrosshair(member.Chart.CoreChart, point))(),
            None: () => fun(() => member.Axis.ClearCrosshair(member.Chart.CoreChart))()));
}

public sealed record ChartSpec(
    string Key,
    Seq<ChartLayer> Layers,
    Seq<ChartAxis> XAxes,
    Seq<ChartAxis> YAxes,
    Seq<ChartSection> Sections,
    Seq<ChartAnnotation> Annotations,
    Option<ThresholdList> Thresholds,
    Option<FacetSpec> Facet,
    Option<LegendSpec> Legend,
    double AnnotationTolerance,
    ChartPolicy Policy) {
    public static ChartSpec Of(string key, ChartPolicy policy, params ChartLayer[] layers) =>
        new(toSeq(layers), Seq(ChartAxis.Time), Seq(ChartAxis.Value),
            Seq<ChartSection>(), Seq<ChartAnnotation>(), None, None, None, 0d, policy);

    public ChartCanvas Canvas => Layers[0].Kind.Canvas;

    public static Fin<ChartSpec> Admit(ChartSpec candidate) =>
        candidate.Layers.IsEmpty
            ? Fin.Fail<ChartSpec>(new ChartFault.SpecRejected($"{candidate.Key}: no layers"))
            : (Slot(candidate.Layers.ForAll(layer => layer.Kind.Canvas == candidate.Canvas), $"{candidate.Key}: canvas"),
               Slot(candidate.XAxes.Count > 0 && candidate.YAxes.Count > 0, $"{candidate.Key}: axis rosters"),
               Slot(candidate.Layers.ForAll(layer => layer.ScalesXAt >= 0 && layer.ScalesXAt < candidate.XAxes.Count
                   && layer.ScalesYAt >= 0 && layer.ScalesYAt < candidate.YAxes.Count), $"{candidate.Key}: axis index"),
               Slot(candidate.Layers.Map(static layer => layer.Name).Distinct().Count == candidate.Layers.Count, $"{candidate.Key}: layer names"),
               Slot(candidate.Annotations.ForAll(mark => candidate.Layers.Exists(layer => layer.Name == mark.Layer)), $"{candidate.Key}: annotation layer"),
               Slot(candidate.AnnotationTolerance >= 0d, $"{candidate.Key}: tolerance"))
                .Apply(static (_, _, _, _, _, _) => unit).As().ToFin()
                .Bind(_ => candidate.XAxes.Append(candidate.YAxes).Traverse(ChartAxis.Admit).As())
                .Bind(_ => candidate.Layers.Traverse(Arity).As())
                .Bind(_ => candidate.Facet.TraverseM(FacetSpec.Admit).As())
                .Bind(_ => candidate.Legend.TraverseM(LegendSpec.Admit).As())
                .Map(_ => candidate);

    static Validation<Error, Unit> Slot(bool holds, string reason) =>
        holds ? Validation<Error, Unit>.Success(unit) : Validation<Error, Unit>.Fail((Error)new ChartFault.SpecRejected(reason));

    public Fin<ChartSpec> Expand(ResolvedLocale locale) =>
        Admit(this).Bind(spec => ChartAnnotation.Project(
                ChartAnnotation.Cluster(spec.Annotations, spec.AnnotationTolerance), locale, spec.Layers)
            .Map(marks => spec with {
                Layers = spec.Layers.Bind(layer => layer.Compare.Match(Some: offset => offset.Expand(layer), None: () => Seq(layer))) + marks.Marks,
                Sections = spec.Sections + marks.Sections,
                Annotations = Seq<ChartAnnotation>(),
            }));

    static Fin<Unit> Arity(ChartLayer layer) =>
        layer.Shape().Bind(shape => shape.Arity >= layer.Kind.Encoding.Arity
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ChartFault.SpecRejected(
                $"layer/{layer.Name}: {shape.Key} carries {shape.Arity} magnitudes, {layer.Kind.Encoding.Key} needs {layer.Kind.Encoding.Arity}")));

    public Fin<Seq<XamlSeries>> Materialize(ChartInk ink, TypographyRole label, ResolvedLocale locale) =>
        Expand(locale).Bind(spec => spec.Layers.Traverse(layer => Mint(layer, ink, label, locale)).As());

    static Fin<XamlSeries> Mint(ChartLayer layer, ChartInk ink, TypographyRole label, ResolvedLocale locale) =>
        layer.Kind.Mint.Switch(
            state: (Layer: layer, Ink: ink, Label: label, Locale: locale),
            factory: static (s, arm) => Fin.Succ(Dressed(arm.Shell(), s.Layer, s.Ink, s.Label, s.Locale)),
            geoAsset: static (s, _) => Fin.Fail<XamlSeries>(new ChartFault.SpecRejected(
                $"layer/{s.Layer.Name}: {s.Layer.Kind.Key} mounts through GeoSeries.Mount, not the series mint")));

    static XamlSeries Dressed(XamlSeries series, ChartLayer layer, ChartInk ink, TypographyRole label, ResolvedLocale locale) {
        series.SeriesName = layer.Name;
        series.ZIndex = layer.Layer;
        series.IsVisibleAtLegend = layer.Traits.Admits(LayerTrait.Toggleable);
        series.ShowDataLabels = layer.Labels.IsSome;
        series.DataLabelsSize = label.Size;
        layer.Labels.Iter(row => {
            series.DataLabelsPaint = ink.Paint(row.Ink);
            series.DataLabelsFormatter = point => row.Text(point, locale);
        });
        if (layer.Literal) { series.Values = layer.Pinned.ToList(); }
        layer.Ink.Iter(chrome => series.Stroke = ink.Paint(chrome));
        if (layer.Kind.Traits.Admits(SeriesTrait.Cartesian) && series is ICartesianSeries cartesian) {
            cartesian.ScalesXAt = layer.ScalesXAt;
            cartesian.ScalesYAt = layer.ScalesYAt;
            cartesian.ShowError = layer.Traits.Admits(LayerTrait.Errors);
            cartesian.ErrorPaint = layer.Traits.Admits(LayerTrait.Errors) ? ink.Paint(ChartChrome.ErrorBar) : null;
        }
        if (layer.Kind.Traits.Admits(SeriesTrait.NullSplitting) && series is ILineSeries line) { line.EnableNullSplitting = true; }
        return series;
    }
}
```

## [04]-[SYNC_AND_POLICY]

- Owner: `ChartPolicy` — the typed interaction posture; `ChartSyncGroups` — the frozen per-`ScaleGroup` lock table; `ChartSync` — the one mount-time lock resolution, the paired-axis write, and the interaction write.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, LanguageExt.Core
- Growth: a new interaction posture is one `ChartPolicy` value row; a new overlay verb is one CommandRow table row the chart raises by key; zero new surface.
- Boundary: every key that was a bare string is TYPED — `MotionPlan`, `TypographyRole`, `ChartChrome`, `PaintFamily` — so a policy value naming nothing is a compile error rather than a resolve that quietly returns the shipped default. `LegendToggle` makes a legend entry a series switch by writing `ISeries.IsVisible` from the legend hit; `AnimationsSpeed` and the easing delegate derive from the `MotionPlan` row through the motion page's reduced-motion projection, so an active reduction reaches chart animation with no chart edit. `VisualElements` overlays route `VisualElementsPointerDown` through the `PointerIntent` CommandRow key, never a local handler. `ScaleGroup` is the axis-pairing key AND the lock key — a second grouping vocabulary beside it is the deleted form; a NAMED group whose lock is absent is a composition defect the board refuses, because minting a fresh object on the miss hands each paired chart a private lock under the name of a shared one, silently; an ungrouped tile keeps its own instance minted ONCE at mount, because a lock re-created per read is never held by two readers and locks nothing. `ChartSync.Mount` is the one `SyncContext` write and the one `ChartSyncGroups.For` caller — every fold that mutates a chart's bound collection (the geo land swap, the brush re-filter) takes the object the LiveCharts update pass itself takes.

```csharp
// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed record ChartPolicy(
    ChartNav Nav,
    ChartFind Find,
    ChartAnchor Tooltip,
    ChartAnchor Legend,
    bool LegendToggle,
    Option<string> ScaleGroup,
    Option<string> PointerIntent,
    MotionPlan Motion,
    TypographyRole LabelRole,
    ChartChrome GridRole,
    PaintFamily Family) {
    public static readonly ChartPolicy Dashboard = new(
        Nav: ChartNav.TimeScroll,
        Find: ChartFind.SharedX,
        Tooltip: ChartAnchor.Auto,
        Legend: ChartAnchor.Hidden,
        LegendToggle: true,
        ScaleGroup: None,
        PointerIntent: None,
        Motion: MotionPlan.Page,
        LabelRole: TypographyRole.Caption,
        GridRole: ChartChrome.Separator,
        Family: PaintFamily.Accent);
}

public sealed record ChartSyncGroups(FrozenDictionary<string, object> Locks) {
    public static ChartSyncGroups Of(Seq<ChartPolicy> policies) =>
        new(policies.Choose(static policy => policy.ScaleGroup).Distinct().ToFrozenDictionary(identity, static _ => new object()));

    public Fin<object> For(Option<string> group) => group.Match(
        Some: key => Locks.TryGetValue(out object? shared)
            ? Fin.Succ(shared)
            : Fin.Fail<object>(new ChartFault.SpecRejected($"sync-group/{key}: no lock minted at board activation")),
        None: static () => Fin.Succ<object>(new object()));
}

public static class ChartSync {
    public static Fin<object> Mount(ChartSyncGroups groups, ChartPolicy policy, IChartView chart) =>
        groups.For(policy.ScaleGroup).Bind(shared =>
            Try.lift(() => {
                chart.SyncContext = shared;
                return Fin.Succ(shared);
            }).Run().Bind(static inner => inner));

    public static Fin<Unit> Pair(Seq<ICartesianAxis> group) =>
        group.Count < 2
            ? Fin.Succ(unit)
            : Try.lift(() => {
                Unit paired = group.Fold(unit, (_, axis) => {
                    axis.SharedWith = group.Filter(peer => !ReferenceEquals(peer, axis));
                    return unit;
                });
                return Fin.Succ(paired);
            }).Run().Bind(static inner => inner);

    public static Fin<Unit> Apply(SourceGenCartesianChart chart, ChartPolicy policy, ChartInk ink) =>
        Try.lift(() => {
            chart.ZoomMode = policy.Nav.Mode;
            chart.FindingStrategy = policy.Find.Strategy;
            chart.TooltipPosition = policy.Tooltip.Tooltip;
            chart.LegendPosition = policy.Legend.Legend;
            chart.TooltipTextPaint = ink.Paint(ChartChrome.TooltipText);
            chart.TooltipBackgroundPaint = ink.Paint(ChartChrome.TooltipBack);
            chart.TooltipTextSize = policy.LabelRole.Size;
            chart.LegendTextPaint = ink.Paint(ChartChrome.LegendText);
            chart.LegendBackgroundPaint = ink.Paint(ChartChrome.LegendBack);
            chart.LegendTextSize = policy.LabelRole.Size;
            chart.AnimationsSpeed = policy.Motion.EnterToken.ChartSpeed;
            chart.EasingFunction = progress => (float)policy.Motion.EnterToken.Curve(progress);
            chart.UpdaterThrottler = policy.Motion.EnterToken.ChartSpeed;
            chart.DrawMarginFrame = new FrameExtension {
                Stroke = ink.Paint(ChartChrome.FrameStroke),
                Fill = ink.Paint(ChartChrome.FrameFill),
            }.Value;
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner);
}
```

## [05]-[SERIES_TABLE]

- Owner: `ChartSeriesKind` — the frozen series-KIND catalog; `ChartCanvas` — the four control families; `SeriesMint` — the construction arm a row carries; `SeriesTrait` — the dress capability vocabulary the grammar's one dress fold reads.
- Cases: line, step-line, scatter, column, row, stacked-area, stacked-step-area, stacked-column, stacked-row, heat, candlestick, box, pie, polar-line, gauge-angular, gauge-background, geo-map; canvas rows cartesian, pie, polar, map materialize as `CartesianChart`, `PieChart`, `PolarChart`, `GeoMap` control templates selected by the `ChartCanvas` key; `SeriesMint` = Factory | GeoAsset — a row constructs a closed `XamlSeries` shell or names the decoded world asset the map canvas boots from; `SeriesTrait` = cartesian | null-splitting — the slots the grammar's dress fold may write on this kind.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, Thinktecture.Runtime.Extensions, LanguageExt.Core, DynamicData
- Growth: a new visualization is one `ChartSeriesKind` row and a new chart family is one `ChartCanvas` row; an eighteenth row carries its render-hash baseline by construction of the same fold; a new dress slot is one `SeriesTrait` row and one arm in the grammar's dress fold; zero new surface.
- Boundary: this catalog is a frozen KIND vocabulary a `ChartLayer` names, never a per-tile seat — a comparison, a facet member, and a dual-axis overlay are all layer rows over the same kind rather than parallel spec objects. Every shell is constructed CLOSED over the canonical `ChartDatum`, so `Mapping` is reachable and typed — the erased shell binds `double?` collections and leaves the projection member unspellable, which is why the factory arms mint `XamlLineSeries<ChartDatum>` and its peers. The geo row's `SeriesMint.GeoAsset` carries `AssetDeclaration.GeoWorld.Asset` ITSELF, resolved through the asset rank fold — a transcribed key literal survives a rename at the assets owner and empties the row silently. Chart code never opens files; the decoded asset feeds `GeoMap` through `SourceGenMapChart`, and heat-land geometry projects from the Bim-owned `GeoFeature` GeoJSON projection delivered over the Persistence query lane. `HeatLandSeries<TLand>` over `CoreHeatLandSeries<TLand>` owns the heat series — `TModel : IWeigthedMapLand` (settable `Name`/`Value` under `INotifyPropertyChanged`), so `GeoLand` implements the interface and binds as the model directly; the fold's in-place `Value` write IS the render invalidation, and a shipped `HeatLand` projection would be a second collection the update pass never watched. `DrawnMap.FindLand(shortName, layerName)` answers null when absent, so the fold treats a missing land as an append rather than a fault. The Mapsui basemap-overlay leg is the disjoint tiled-basemap owner (`Charts/basemap.md`) composing the realized Bim MVT source. `AdditionalVisualStates` on the materialized series carries per-point hover and selection states resolved from `ChartInk`, so a per-point state is a series column, never a local overlay control; gauge accessories `XamlNeedle`/`XamlAngularTicks` ride the gauge rows as canvas children. Per-chart wrapper controls, hand-drawn chart code, and a second charting package are the deleted patterns.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartCanvas {
    public static readonly ChartCanvas Cartesian = new("cartesian");
    public static readonly ChartCanvas Pie = new("pie");
    public static readonly ChartCanvas Polar = new("polar");
    public static readonly ChartCanvas Map = new("map");
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeriesTrait : ICapability<SeriesTrait> {
    public static readonly SeriesTrait Cartesian = new(key: "cartesian");
    public static readonly SeriesTrait NullSplitting = new(key: "null-splitting");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SeriesMint {
    private SeriesMint() { }
    public sealed record Factory(Func<XamlSeries> Shell) : SeriesMint;
    public sealed record GeoAsset(AssetKey Key) : SeriesMint;
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartSeriesKind {
    static readonly CapabilitySet<SeriesTrait> Plot = CapabilitySet<SeriesTrait>.Of(SeriesTrait.Cartesian);
    static readonly CapabilitySet<SeriesTrait> Broken = CapabilitySet<SeriesTrait>.Of(SeriesTrait.Cartesian, SeriesTrait.NullSplitting);

    public static readonly ChartSeriesKind Line = new("line", ChartCanvas.Cartesian, ChartEncoding.Xy, Broken, new SeriesMint.Factory(static () => new XamlLineSeries<ChartDatum>()));
    public static readonly ChartSeriesKind StepLine = new("step-line", ChartCanvas.Cartesian, ChartEncoding.Xy, Broken, new SeriesMint.Factory(static () => new XamlStepLineSeries<ChartDatum>()));
    public static readonly ChartSeriesKind Scatter = new("scatter", ChartCanvas.Cartesian, ChartEncoding.Weighted, Plot, new SeriesMint.Factory(static () => new XamlScatterSeries<ChartDatum>()));
    public static readonly ChartSeriesKind Column = new("column", ChartCanvas.Cartesian, ChartEncoding.Xy, Plot, new SeriesMint.Factory(static () => new XamlColumnSeries<ChartDatum>()));
    public static readonly ChartSeriesKind Row = new("row", ChartCanvas.Cartesian, ChartEncoding.Xy, Plot, new SeriesMint.Factory(static () => new XamlRowSeries<ChartDatum>()));
    public static readonly ChartSeriesKind StackedArea = new("stacked-area", ChartCanvas.Cartesian, ChartEncoding.Xy, Broken, new SeriesMint.Factory(static () => new XamlStackedAreaSeries<ChartDatum>()));
    public static readonly ChartSeriesKind StackedStepArea = new("stacked-step-area", ChartCanvas.Cartesian, ChartEncoding.Xy, Broken, new SeriesMint.Factory(static () => new XamlStackedStepAreaSeries<ChartDatum>()));
    public static readonly ChartSeriesKind StackedColumn = new("stacked-column", ChartCanvas.Cartesian, ChartEncoding.Xy, Plot, new SeriesMint.Factory(static () => new XamlStackedColumnSeries<ChartDatum>()));
    public static readonly ChartSeriesKind StackedRow = new("stacked-row", ChartCanvas.Cartesian, ChartEncoding.Xy, Plot, new SeriesMint.Factory(static () => new XamlStackedRowSeries<ChartDatum>()));
    public static readonly ChartSeriesKind Heat = new("heat", ChartCanvas.Cartesian, ChartEncoding.Weighted, Plot, new SeriesMint.Factory(static () => new XamlHeatSeries<ChartDatum>()));
    public static readonly ChartSeriesKind Candlestick = new("candlestick", ChartCanvas.Cartesian, ChartEncoding.Financial, Plot, new SeriesMint.Factory(static () => new XamlCandlesticksSeries<ChartDatum>()));
    public static readonly ChartSeriesKind Box = new("box", ChartCanvas.Cartesian, ChartEncoding.Summary, Plot, new SeriesMint.Factory(static () => new XamlBoxSeries<ChartDatum>()));
    public static readonly ChartSeriesKind Pie = new("pie", ChartCanvas.Pie, ChartEncoding.Xy, CapabilitySet<SeriesTrait>.None, new SeriesMint.Factory(static () => new XamlPieSeries<ChartDatum>()));
    public static readonly ChartSeriesKind PolarLine = new("polar-line", ChartCanvas.Polar, ChartEncoding.Xy, CapabilitySet<SeriesTrait>.None, new SeriesMint.Factory(static () => new XamlPolarLineSeries<ChartDatum>()));
    public static readonly ChartSeriesKind GaugeAngular = new("gauge-angular", ChartCanvas.Pie, ChartEncoding.Xy, CapabilitySet<SeriesTrait>.None, new SeriesMint.Factory(static () => new XamlAngularGaugeSeries()));
    public static readonly ChartSeriesKind GaugeBackground = new("gauge-background", ChartCanvas.Pie, ChartEncoding.Xy, CapabilitySet<SeriesTrait>.None, new SeriesMint.Factory(static () => new XamlGaugeBackgroundSeries()));
    public static readonly ChartSeriesKind Geo = new("geo-map", ChartCanvas.Map, ChartEncoding.Weighted, CapabilitySet<SeriesTrait>.None, new SeriesMint.GeoAsset(AssetDeclaration.GeoWorld.Asset));

    public ChartCanvas Canvas { get; }

    public ChartEncoding Encoding { get; }

    public CapabilitySet<SeriesTrait> Traits { get; }

    public SeriesMint Mint { get; }

    public Fin<CaptureRow> Baseline((ThemeVariantRow Variant, DensityRow Density) cell, RenderHashLane lane,
        Func<ChartSeriesKind, (ThemeVariantRow, DensityRow), FrameGrab> grab) =>
        (lane with {
            Key = ContentHash.Of((Kind: Key, Variant: cell.Variant.Key, Density: cell.Density.Key),
                static (state, writer) => writer.String(state.Kind).String(state.Variant).String(state.Density))
                .ToString("x32", CultureInfo.InvariantCulture),
        }).Row(grab(this, cell));
}
```

## [06]-[GEO_OVERLAY]

- Owner: `GeoLand` — the bound series model realizing `IWeigthedMapLand`; `GeoSeries` — the geo mount owning the keyed reason fold and the land index.
- Entry: `GeoSeries.Mount(object sync, IObservable<IChangeSet<GeoLand, string>> diff, SurfaceScheduler scheduler, Action<int> observed, Action<Error> fault)` — mints the `HeatLandSeries<GeoLand>` and binds the land-swap fold under the resolved group lock, answering the series beside the feed's detach handle, so the geo kind's one consumer and the fold's one caller are the same expression and an unmounted fold cannot exist.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, DynamicData, LanguageExt.Core
- Growth: a new land delta reason is one `Apply` arm; zero new surface.
- Boundary: the fold OWNS the dispatch on `Change<GeoLand, string>.Reason` — `Add` appends, `Update`/`Refresh` replace by feature name with `Current` reassigning heat through the family ramp, `Remove` drops, `Moved` is a no-op on a keyed set carrying no ordinal, and an unadmitted reason REFUSES rather than passing as a silent no-op that quietly stops updating one land. The whole mutation runs under the resolved group lock `ChartSync.Mount` supplied — the lock is a parameter, so the law is enforced by the signature — and a land swap and a cross-filter re-filter cannot tear the bound set against each other or against the LiveCharts update pass. Feature name IS the key at every seat: the change set keys on it, the index maps it, and a second key projection beside it would let a rename desynchronize the two. In-place writes on the live bound collection are the declared mutation grain — an immutable rebuild per delta re-renders the whole layer the incremental fold exists to avoid — and the `Value` INPC write IS the render invalidation, where an element swap re-binds. The overlay counts each delivered change set once through the composition-bound `observed` edge because that is where the folded count exists; the change-set is the Persistence `SpatialDiff` change-detection fold projected to land records, so AppUi re-computes no diff, and the land records project from the Bim `GeoFeature` vocabulary Persistence serves — a choropleth arm on the Compute proto family is the rejected wire.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed class GeoLand(string name, double value) : LiveChartsCore.Geo.IWeigthedMapLand {
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name {
        get;
        set { field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
    } = name;

    public double Value {
        get;
        set { field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); }
    } = value;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeoSeries {
    public static (HeatLandSeries<GeoLand> Series, IDisposable Feed) Mount(
        object sync,
        IObservable<IChangeSet<GeoLand, string>> diff,
        SurfaceScheduler scheduler,
        Action<int> observed,
        Action<Error> fault) {
        HeatLandSeries<GeoLand> series = new([]);
        Dictionary<string, int> index = new(StringComparer.Ordinal);
        IDisposable feed = diff.ObserveOn(scheduler.Ui)
            .Subscribe(
                changes => {
                    lock (sync) {
                        toSeq(changes)
                            .Fold(Fin.Succ(0), (acc, change) =>
                                acc.Bind(folded => Apply(series.Lands, index, change).Map(touched => folded + touched)))
                            .Match(Succ: observed, Fail: fault);
                    }
                },
                raw => fault(Error.New(raw.Message, raw)));
        return (series, feed);
    }

    static Fin<int> Apply(IList<GeoLand> lands, Dictionary<string, int> index, Change<GeoLand, string> change) => change.Reason switch {
        ChangeReason.Add => Fin.Succ(Appended(lands, index, change.Current)),
        ChangeReason.Update or ChangeReason.Refresh => Fin.Succ(Replaced(lands, index, change.Key, change.Current)),
        ChangeReason.Remove => Fin.Succ(Dropped(lands, index, change.Key)),
        ChangeReason.Moved => Fin.Succ(0),
        var reason => Fin.Fail<int>(new ChartFault.LayerRejected($"geo-land: {reason} is not an admitted change reason")),
    };

    static int Appended(IList<GeoLand> lands, Dictionary<string, int> index, GeoLand current) {
        index[current.Name] = lands.Count;
        lands.Add(current);
        return 1;
    }

    static int Replaced(IList<GeoLand> lands, Dictionary<string, int> index, string key, GeoLand current) {
        if (!index.TryGetValue(out int at)) { return Appended(lands, index, current); }
        lands[at].Value = current.Value;
        return 1;
    }

    static int Dropped(IList<GeoLand> lands, Dictionary<string, int> index, string key) {
        if (!index.TryGetValue(out int at)) { return 0; }
        lands.RemoveAt(at);
        index.Remove();
        foreach ((string name, int seat) in index) {
            if (seat > at) { index[name] = seat - 1; }
        }
        return 1;
    }
}
```

## [07]-[AXES]

- Owner: `ChartAxisKind` — the scale-shell catalog carrying shell factory, format, and label projection per row; `AxisTrait` — the axis capability vocabulary; `ChartAxis` — the per-axis declaration; `BandAxis` — the orientation row with its coordinate-slot projection; `ChartSection` — the band value.
- Cases: `ChartAxisKind` = numeric, instant, duration, logarithmic, categorical, polar — mapping to `XamlAxis`, `XamlDateTimeAxis`, `XamlTimeSpanAxis`, `XamlLogarithmicAxis`, `XamlAxis` under a label roster, and `XamlPolarAxis`; `AxisTrait` = inverted | crosshair; `BandAxis` = X | Y — a band ALONG X is the vertical strip, along Y the horizontal one, and the row's seat projection is the one place that correspondence is spelled.
- Entry: `ChartAxis.Materialize(ChartInk ink, TypographyRole label, ResolvedLocale locale)` — the one axis mint; `ChartSection.Of(BandAxis axis, double from, double to, …)` — the one band constructor both orientations and the threshold projections take; `ChartSection.Materialize(ChartInk ink, TypographyRole label)` — the one band mint.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, NodaTime, UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new scale is one `ChartAxisKind` row; a categorical domain, a measure role, a second value axis, a trait, and a fixed range are each one column on a `ChartAxis` value; a new band is one `ChartSection` value; zero new surface.
- Boundary: DUAL axes are the axis LIST, not a second axis concept — a spec carries `Seq<ChartAxis>` per orientation and each layer names its index through `ScalesXAt`/`ScalesYAt`, so two named-unit rows and two layer indices are an energy plot and a layer naming a missing index refuses at `ChartSpec.Admit` rather than silently scaling against axis zero. A CATEGORICAL axis is the package's own `Labels` roster under `MinStep` of one and `ForceStepToMin`, so a category is an ordinal position with a label and the datum's `X` is that ordinal; a categorical row carrying no domain refuses. A MEASURED axis carries its `MeasureRole` and nothing else — display unit, conversion, precision, and grammar are the resolved measurement policy's, the title's unit token is the abbreviation the policy ELECTED (`MeasurePolicy.Abbreviation`, `Theme/locale#MEASUREMENT_FORMAT`), and an axis carrying a unit string is the deleted form because it prints the authoring machine's units to every viewer. Every tick label crosses `ResolvedLocale` — a measured axis through `Quantity` under its role, the temporal rows through the locale's own patterns via each row's OWN label projection, a bare numeric axis through the culture-bound `Text` over the row's `CompositeFormat` (parsed once per row) — so no arm reaches a default `ToString` and a comma decimal separator cannot silently become a point on the one surface a viewer reads numbers from. Instant ticks cross as `DateTime` ordinals through the package's own `AsDate` projection, so no page-local epoch arithmetic exists on either side. Axis ORDER never mirrors: `MirrorSubject.NumericAxis` is a never-flipping subject, so a right-to-left locale mirrors chrome and leaves value direction, category order, and time direction as the data carries them — `AxisTrait.Inverted` stays a declared trait a spec sets deliberately and never a locale consequence. `ChartPolicy.ScaleGroup` pairs axes across charts through `ICartesianAxis.SharedWith` under one shared min-max fold per group key, and the SAME key resolves the render lock at `ChartSync.Mount`. A `ChartSection` carries both-axis coordinates as four independent `Option<double>` bounds because the package extends a null bound to the draw margin, so a horizontal band, a vertical band, and a rectangular region are one value at three coordinate populations and a band family per orientation is unspellable — `BandAxis` owns which pair a linear band populates, so the threshold family's bands and the legend's dock read the same two rows instead of each carrying an orientation bool; the label rides the section because a caption that is a separate overlay drifts the moment either axis re-ranges. Every paint on both owners resolves from a `ChartChrome` row.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartAxisKind {
    public static readonly ChartAxisKind Numeric = new("numeric", "{0:G6}", categorical: false,
        static () => new XamlAxis(),
        static (kind, locale, value) => locale.Text(kind.Format, value));
    public static readonly ChartAxisKind Instant = new("instant", "{0:HH:mm:ss}", categorical: false,
        static () => new XamlDateTimeAxis(),
        static (_, locale, value) => locale.Stamp(NodaTime.Instant.FromDateTimeUtc(DateTime.SpecifyKind(value.AsDate(), DateTimeKind.Utc))));
    public static readonly ChartAxisKind Duration = new("duration", "{0:c}", categorical: false,
        static () => new XamlTimeSpanAxis(),
        static (_, locale, value) => locale.Span(NodaTime.Duration.FromTimeSpan(value.AsTimeSpan())));
    public static readonly ChartAxisKind Logarithmic = new("logarithmic", "{0:E2}", categorical: false,
        static () => new XamlLogarithmicAxis(),
        static (kind, locale, value) => locale.Text(kind.Format, value));
    public static readonly ChartAxisKind Categorical = new("categorical", "{0}", categorical: true,
        static () => new XamlAxis(),
        static (kind, locale, value) => locale.Text(kind.Format, value));
    public static readonly ChartAxisKind Polar = new("polar", "{0:G4}", categorical: false,
        static () => new XamlPolarAxis(),
        static (kind, locale, value) => locale.Text(kind.Format, value));

    public string LabelFormat { get; }

    public bool Categorical { get; }

    public CompositeFormat Format => field ??= CompositeFormat.Parse(LabelFormat);

    [UseDelegateFromConstructor]
    public partial IPlane Shell();

    [UseDelegateFromConstructor]
    public partial string Label(ChartAxisKind kind, ResolvedLocale locale, double value);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxisTrait : ICapability<AxisTrait> {
    public static readonly AxisTrait Inverted = new(key: "inverted");
    public static readonly AxisTrait Crosshair = new(key: "crosshair");
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BandAxis {
    public static readonly BandAxis X = new("x",
        static (from, to) => (Xi: Some(from), Xj: Some(to), Yi: Option<double>.None, Yj: Option<double>.None));
    public static readonly BandAxis Y = new("y",
        static (from, to) => (Xi: Option<double>.None, Xj: Option<double>.None, Yi: Some(from), Yj: Some(to)));

    [UseDelegateFromConstructor]
    public partial (Option<double> Xi, Option<double> Xj, Option<double> Yi, Option<double> Yj) Seat(double from, double to);
}

public sealed record ChartAxis(
    ChartAxisKind Kind,
    Option<string> NameKey,
    Option<MeasureRole> Measure,
    Option<Seq<string>> Categories,
    Option<(double Min, double Max)> Limits,
    Option<double> UnitWidth,
    CapabilitySet<AxisTrait> Traits,
    AxisPosition Position) {
    public static readonly ChartAxis Time = new(ChartAxisKind.Instant, None, None, None, None, None, CapabilitySet<AxisTrait>.Of(AxisTrait.Crosshair), AxisPosition.Start);
    public static readonly ChartAxis Value = new(ChartAxisKind.Numeric, None, None, None, None, None, CapabilitySet<AxisTrait>.Of(AxisTrait.Crosshair), AxisPosition.Start);

    public static Fin<ChartAxis> Admit(ChartAxis candidate) => (
        Slot(candidate.Kind.Categorical == candidate.Categories.IsSome
            && candidate.Categories.ForAll(static domain => domain.Count > 0 && domain.ForAll(static label => !string.IsNullOrWhiteSpace(label))),
            $"axis/{candidate.Kind.Key}: categorical domain"),
        Slot(candidate.Limits.ForAll(static range => double.IsFinite(range.Min) && double.IsFinite(range.Max) && range.Min < range.Max),
            $"axis/{candidate.Kind.Key}: limits"),
        Slot(candidate.UnitWidth.ForAll(static width => double.IsFinite(width) && width > 0d),
            $"axis/{candidate.Kind.Key}: unit width"))
        .Apply(static (_, _, _) => candidate).As().ToFin();

    static Validation<Error, Unit> Slot(bool holds, string reason) =>
        holds ? Validation<Error, Unit>.Success(unit) : Validation<Error, Unit>.Fail((Error)new ChartFault.SpecRejected(reason));

    public string Title(ResolvedLocale locale) =>
        (NameKey.Map(locale.Label), Measure.Map(role => locale.Measures.Abbreviation(role, locale.Formats))) switch {
            ({ IsSome: true, Case: string name }, { IsSome: true, Case: string unit }) => $"{name} [{unit}]",
            ({ IsSome: true, Case: string name }, _) => name,
            (_, { IsSome: true, Case: string unit }) => unit,
            _ => string.Empty,
        };

    public Fin<IPlane> Materialize(ChartInk ink, TypographyRole label, ResolvedLocale locale) =>
        Admit(this).Map(admitted => {
            IPlane plane = admitted.Kind.Shell();
            plane.Name = admitted.Title(locale);
            plane.Labeler = value => Text(admitted, locale, value);
            plane.IsInverted = admitted.Traits.Admits(AxisTrait.Inverted);
            admitted.Limits.Iter(range => { plane.MinLimit = range.Min; plane.MaxLimit = range.Max; });
            admitted.UnitWidth.Iter(width => plane.UnitWidth = width);
            admitted.Categories.Iter(domain => {
                plane.Labels = domain.ToList();
                plane.MinStep = 1d;
                plane.ForceStepToMin = true;
            });
            _ = ChartChrome.Apply(plane, ink, label);
            if (plane is ICartesianAxis axis) {
                axis.Position = admitted.Position;
                if (!admitted.Traits.Admits(AxisTrait.Crosshair)) {
                    axis.CrosshairPaint = null;
                    axis.CrosshairLabelsPaint = null;
                }
            }
            return plane;
        });

    static string Text(ChartAxis axis, ResolvedLocale locale, double value) =>
        axis.Categories.Bind(domain => domain.At((int)Math.Round(value))).Match(
            Some: identity,
            None: () => axis.Measure.Match(
                Some: role => Quantity.TryFrom(value, role.MetricUnit, out IQuantity? quantity) && quantity is not null
                    ? locale.Quantity(quantity, role).IfFail(_ => locale.Text(axis.Kind.Format, value))
                    : locale.Text(axis.Kind.Format, value),
                None: () => axis.Kind.Label(axis.Kind, locale, value)));
}

public readonly record struct ChartSection(
    Option<double> Xi,
    Option<double> Xj,
    Option<double> Yi,
    Option<double> Yj,
    Option<string> Label,
    ChartChrome Fill,
    ChartChrome Stroke,
    Option<Severity> Tint,
    int ScalesXAt,
    int ScalesYAt) {
    public static ChartSection Of(BandAxis axis, double from, double to, ChartChrome fill,
        Option<Severity> tint = default, Option<string> label = default, int scalesXAt = 0, int scalesYAt = 0) =>
        axis.Seat(from, to) switch {
            var seat => new(seat.Xi, seat.Xj, seat.Yi, seat.Yj, label, fill, ChartChrome.SectionStroke, tint, scalesXAt, scalesYAt),
        };

    public Fin<XamlRectangularSection> Materialize(ChartInk ink, TypographyRole label) =>
        (Xi, Xj, Yi, Yj) switch {
            (None, None, None, None) => Fin.Fail<XamlRectangularSection>(new ChartFault.SpecRejected("section/unbounded")),
            _ => Fin.Succ(new XamlRectangularSection {
                Xi = Xi.Match<double?>(Some: static value => value, None: static () => null),
                Xj = Xj.Match<double?>(Some: static value => value, None: static () => null),
                Yi = Yi.Match<double?>(Some: static value => value, None: static () => null),
                Yj = Yj.Match<double?>(Some: static value => value, None: static () => null),
                Fill = Tint.Match(Some: ink.Ink, None: () => ink.Paint(Fill)),
                Stroke = ink.Paint(Stroke),
                Label = Label.IfNone(string.Empty),
                LabelPaint = ink.Paint(ChartChrome.SectionLabel),
                LabelSize = label.Size,
                ScalesXAt = ScalesXAt,
                ScalesYAt = ScalesYAt,
            }),
        };
}
```

## [08]-[ANNOTATION_PLANE]

- Owner: `ChartAnnotation` — the point-mark, region, and event-line plane owning its clustering fold and its two materializations; `AnnotationCluster` — one collapsed mark plus what it stands for.
- Cases: Point | Region | Moment — the moment arm is the event line, named off the keyword the generated switch parameter cannot take.
- Entry: `ChartAnnotation.Cluster(Seq<ChartAnnotation> marks, double tolerance)` — the density fold; `ChartAnnotation.Project(Seq<AnnotationCluster> clusters, ResolvedLocale locale, Seq<ChartLayer> layers)` — the one mint onto the two data-anchored owners.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new mark class is one arm; zero new surface.
- Boundary: an ANNOTATION is a mark declared beside the layer it annotates rather than a free overlay — the arm names the layer, `ChartSpec.Admit` refuses a name the roster lacks, and the mark inherits that layer's axis indices and moves with its series when a spec is re-authored. Every arm materializes onto an owner the plane ALREADY has, because the package's two data-anchored planes are the section and the series: `XamlDrawnLabelVisual` carries pixels with no axis binding, so a label visual anchored to a datum is unreachable from this plane by construction. The REGION arm materializes as a `ChartSection` on its `BandAxis` row; the MOMENT arm is that same section at `From == To`, so a vertical instant mark is a degenerate band whose stroke draws the hairline and whose own `Label` draws the flag; the POINT arm is a one-datum scatter `ChartLayer` outside the legend — pinned to its literal coordinate, captioned through `DataLabel.Caption` so the mark prints the words the board wrote instead of the number its marker already sits at. Density is a CLUSTERING policy rather than a draw-order accident: marks inside the declared tolerance collapse into one cluster carrying its member count, the lead mark's caption renders through the locale's own cluster message stem under the viewer's plural rules, and the cluster's severity is the WORST member's — an overlapping stack of forty deploy flags renders as one flag reading forty rather than burying the critical one behind thirty-nine nominal ones. Instants cross OUTBOUND to chart space as raw `DateTime.Ticks`, the exact ordinal the instant axis reads back through `AsDate` — the package ships that conversion in one direction only, so both sides agree by construction. The tolerance is in AXIS units — a board declares it in the measure the axis renders, and no fold needs a pixel-per-unit reading it cannot have before the measure pass; the discriminant (axis-space, not model-space) is why it composes no kernel tolerance lane.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChartAnnotation {
    private ChartAnnotation() { }

    public sealed record Point(string Layer, double X, double Y, string Label, Severity Severity) : ChartAnnotation;
    public sealed record Region(string Layer, BandAxis Axis, double From, double To, string Label, Severity Severity) : ChartAnnotation;
    public sealed record Moment(string Layer, Instant At, string Flag, Severity Severity) : ChartAnnotation;

    public string Layer => Switch(
        point: static row => row.Layer, region: static row => row.Layer, moment: static row => row.Layer);

    public Severity Severity => Switch(
        point: static row => row.Severity, region: static row => row.Severity, moment: static row => row.Severity);

    public string Caption => Switch(
        point: static row => row.Label, region: static row => row.Label, moment: static row => row.Flag);

    public string Arm => Switch(
        point: static _ => "point", region: static _ => "region", moment: static _ => "moment");

    public double Ordinate => Switch(
        point: static row => row.X,
        region: static row => row.From,
        moment: static row => row.At.ToDateTimeUtc().Ticks);

    public static string ClusterStem => LocaleStrings.Key(nameof(ChartAnnotation), "cluster");

    public static Seq<AnnotationCluster> Cluster(Seq<ChartAnnotation> marks, double tolerance) =>
        toSeq(marks.GroupBy(static mark => (mark.Layer, mark.Arm)))
            .Bind(family => toSeq(family.OrderBy(static mark => mark.Ordinate))
                .Fold((Closed: Seq<AnnotationCluster>(), Open: Option<AnnotationCluster>.None), (held, mark) => held.Open.Match(
                    Some: open => Math.Abs(mark.Ordinate - open.Lead.Ordinate) > tolerance
                        ? (held.Closed.Add(open), Some(new AnnotationCluster(mark, 1, mark.Severity)))
                        : (held.Closed, Some(open with { Count = open.Count + 1, Severity = Severity.Worst(Seq(open.Severity, mark.Severity), identity) })),
                    None: () => (held.Closed, Some(new AnnotationCluster(mark, 1, mark.Severity)))))
                switch { var folded => folded.Closed + toSeq(folded.Open) });

    public static Fin<(Seq<ChartSection> Sections, Seq<ChartLayer> Marks)> Project(
        Seq<AnnotationCluster> clusters, ResolvedLocale locale, Seq<ChartLayer> layers) =>
        clusters.Traverse(cluster => Caption(cluster, locale).Map(caption => (Cluster: cluster, Caption: caption))).As()
            .Bind(captioned => captioned.Traverse(row => Seated(row.Cluster, row.Caption, layers)).As())
            .Map(seated => (
                Sections: seated.Choose(static row => row.Section),
                Marks: seated.Choose(static row => row.Mark)));

    static Fin<string> Caption(AnnotationCluster cluster, ResolvedLocale locale) =>
        cluster.Count > 1
            ? locale.Message(ClusterStem, ("count", cluster.Count), ("label", cluster.Lead.Caption))
            : Fin.Succ(cluster.Lead.Caption);

    static Fin<(Option<ChartSection> Section, Option<ChartLayer> Mark)> Seated(
        AnnotationCluster cluster, string caption, Seq<ChartLayer> layers) =>
        layers.Find(layer => layer.Name == cluster.Lead.Layer).Match(
            Some: layer => Fin.Succ(cluster.Lead.Switch(
                state: (Cluster: cluster, Caption: caption, Layer: layer),
                point: static (s, row) => (
                    Option<ChartSection>.None,
                    Some(Mark(s.Layer, row, s.Caption))),
                region: static (s, row) => (
                    Some(ChartSection.Of(row.Axis, row.From, row.To, ChartChrome.SectionFill,
                        Some(s.Cluster.Severity), Some(s.Caption), s.Layer.ScalesXAt, s.Layer.ScalesYAt)),
                    Option<ChartLayer>.None),
                moment: static (s, row) => (
                    Some(ChartSection.Of(BandAxis.X,
                        row.At.ToDateTimeUtc().Ticks, row.At.ToDateTimeUtc().Ticks,
                        ChartChrome.SectionStroke, Some(s.Cluster.Severity), Some(s.Caption), s.Layer.ScalesXAt, s.Layer.ScalesYAt)),
                    Option<ChartLayer>.None))),
            None: () => Fin.Fail<(Option<ChartSection>, Option<ChartLayer>)>(
                new ChartFault.SpecRejected($"annotation/{cluster.Lead.Layer}: names no layer on this spec")));

    static ChartLayer Mark(ChartLayer host, Point row, string caption) =>
        ChartLayer.Of($"{host.Name}:mark:{row.X}", ChartSeriesKind.Scatter, host.Stream) with {
            ScalesXAt = host.ScalesXAt,
            ScalesYAt = host.ScalesYAt,
            Ink = Some(ChartChrome.Annotation),
            Labels = Some(DataLabel.Caption),
            Traits = CapabilitySet<LayerTrait>.None,
            Layer = ChartChrome.Annotation.Layer,
            Pinned = Seq(ChartDatum.Point(row.X, row.Y, caption)),
        };
}

public readonly record struct AnnotationCluster(ChartAnnotation Lead, int Count, Severity Severity);
```

## [09]-[LEGEND_VOCABULARY]

- Owner: `LegendDomain` — the closed vocabulary of what a legend is a legend OF; `LegendColumn` — one per-series statistics column naming a settled reducer; `LegendDock` — the placement vocabulary whose orientation DERIVES from its side; `LegendArm` — the render-arm verdict; `LegendSpec` — the one declaration carrying that verdict as a derivation; `LegendEntry` — the resolved row every arm renders.
- Cases: `LegendDomain` = Series | Continuous | Stepped | Categorized | Ordinal; `LegendDock` = hidden, top, bottom, left, right, top-left, top-right, bottom-left, bottom-right; `LegendArm` = swatches | ramp | drawn.
- Entry: `LegendSpec.Admit(candidate)` — domain, segments, columns, and dock proved together, every defect named; `LegendSpec.Arm` — the ONE arm classification every dispatch reads.
- Auto: a chart legend, an analysis-mesh legend, and a false-colour viewport legend read one vocabulary, so a surface with a fixed unconfigurable legend is unspellable; every value a legend prints crosses the resolved locale under the spec's own measure role, so a ramp bound and a table statistic carry the unit the viewer's policy elected rather than the one an author typed.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, SkiaSharp, UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new legend kind is one `LegendDomain` arm with its entry projection and its `Arm` election; a new statistics column is one `LegendColumn` naming a settled `ChartReducer`; a new placement is one `LegendDock` row; zero new surface.
- Boundary: the domain arm is the WHOLE discriminant — a form column beside it would let a spec declare a ramp presentation over a series domain, a state no renderer can answer — and the statistics table is that same discriminant at a populated `Columns` roster, so a table legend is a swatch legend that also carries reductions. What each render arm can DRAW is stated law, because the package legends are far narrower than the declaration: the default legend builds one miniature plus one series name per visible entry and nothing else; the heat legend reads the FIRST visible heat series' own map and weight bounds and draws a gradient bar with exactly two labels. Both derive orientation from `LegendPosition`, so `Vertical` on the dock row is a DERIVATION of its side, never a column a spec could contradict. Every richer form — statistics columns, stepped bands, categorized members, ordinal dictionaries, and every corner dock, since `LegendPosition` spells four sides and no corner — renders on the custom plane through the `Drawn` arm, and that whole classification is `LegendSpec.Arm`, so `Admit` and the render dispatch read one verdict and neither re-derives it from type tests. The CLAMP on a continuous domain applies to the DATA through `TransformRow.Clamp`, because the heat legend prints the series' own measured bounds — a clamp declared on the legend alone would caption a range the ramp does not paint. The stepped domain's band count is `ThresholdList.Edges`' — a legend never invents a step set, so a stepped legend and the axis bands beside it cannot drift. The ORDINAL arm exists because a coded raster carries integer codes with no numeric meaning: interpolating between code three and code four is meaningless, so the dictionary renders discrete swatches keyed by code and a ramp over the same data refuses. Legend chrome resolves entirely from `ChartChrome` rows on generated rungs, so a legend colour and a control colour are one value.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LegendDomain {
    private LegendDomain() { }

    public sealed record Series() : LegendDomain;
    public sealed record Continuous(double Low, double High) : LegendDomain;
    public sealed record Stepped(ThresholdList List, double Low, double High) : LegendDomain;
    public sealed record Categorized(Seq<(string Label, double At)> Members) : LegendDomain;
    public sealed record Ordinal(HashMap<int, string> Dictionary) : LegendDomain;
}

public sealed record LegendColumn(string HeaderKey, ChartReducer Reducer, double Tau, Option<MeasureRole> Measure);

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LegendDock {
    public static readonly LegendDock Hidden = new("hidden", LegendPosition.Hidden, corner: false);
    public static readonly LegendDock Top = new("top", LegendPosition.Top, corner: false);
    public static readonly LegendDock Bottom = new("bottom", LegendPosition.Bottom, corner: false);
    public static readonly LegendDock Left = new("left", LegendPosition.Left, corner: false);
    public static readonly LegendDock Right = new("right", LegendPosition.Right, corner: false);
    public static readonly LegendDock TopLeft = new("top-left", LegendPosition.Left, corner: true);
    public static readonly LegendDock TopRight = new("top-right", LegendPosition.Right, corner: true);
    public static readonly LegendDock BottomLeft = new("bottom-left", LegendPosition.Left, corner: true);
    public static readonly LegendDock BottomRight = new("bottom-right", LegendPosition.Right, corner: true);

    public LegendPosition Side { get; }

    public bool Corner { get; }

    public bool Vertical => Side is LegendPosition.Left or LegendPosition.Right;
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LegendArm {
    public static readonly LegendArm Swatches = new("swatches");
    public static readonly LegendArm Ramp = new("ramp");
    public static readonly LegendArm Drawn = new("drawn");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LegendSpec(
    string Key,
    LegendDomain Domain,
    LegendDock Dock,
    Seq<LegendColumn> Columns,
    Option<MeasureRole> Measure,
    int Segments,
    Option<string> TitleKey,
    Option<(double X, double Y)> Offset) {
    public static readonly LegendSpec Swatches = new(
        "swatches", new LegendDomain.Series(), LegendDock.Right, Seq<LegendColumn>(), None, 0, None, None);

    public const string ResetIntent = "chart.legend.reset";

    public LegendArm Arm =>
        Dock == LegendDock.Hidden ? LegendArm.Swatches
        : Dock.Corner || !Columns.IsEmpty ? LegendArm.Drawn
        : Domain.Switch(
            series: static _ => LegendArm.Swatches,
            continuous: static _ => LegendArm.Ramp,
            stepped: static _ => LegendArm.Drawn,
            categorized: static _ => LegendArm.Drawn,
            ordinal: static _ => LegendArm.Drawn);

    public static Fin<LegendSpec> Admit(LegendSpec candidate) =>
        (Gate(!string.IsNullOrWhiteSpace(candidate.Key), $"{candidate.Key}: blank key"),
         Gate(candidate.Segments >= 0, $"{candidate.Key}: negative segments"),
         Gate(candidate.Columns.Map(static column => column.HeaderKey).Distinct().Count == candidate.Columns.Count,
             $"{candidate.Key}: duplicate column headers"),
         Gate(candidate.Arm != LegendArm.Ramp || candidate.Segments >= 2, $"{candidate.Key}: ramp needs two stops"),
         Gate(candidate.Columns.IsEmpty || candidate.Domain is LegendDomain.Series, $"{candidate.Key}: columns need a series domain"),
         Gate(candidate.Offset.ForAll(static at => double.IsFinite(at.X) && double.IsFinite(at.Y)), $"{candidate.Key}: non-finite offset"),
         Domain(candidate))
            .Apply((_, _, _, _, _, _, _) => candidate).As().ToFin();

    static Validation<Error, Unit> Domain(LegendSpec candidate) => candidate.Domain.Switch(
        state: candidate,
        series: static (spec, _) => Gate(true, spec.Key),
        continuous: static (spec, row) => Gate(row.High > row.Low, $"{spec.Key}: inverted ramp bounds"),
        stepped: static (spec, row) => Gate(row.High > row.Low && !row.List.Steps.IsEmpty, $"{spec.Key}: stepped domain carries no crossing"),
        categorized: static (spec, row) => Gate(!row.Members.IsEmpty
            && row.Members.ForAll(static member => !string.IsNullOrWhiteSpace(member.Label) && double.IsFinite(member.At)),
            $"{spec.Key}: empty categorized domain"),
        ordinal: static (spec, row) => Gate(!row.Dictionary.IsEmpty, $"{spec.Key}: empty ordinal dictionary"));

    static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? unit : (Validation<Error, Unit>)(Error)new ChartFault.LegendRejected(detail);
}

public readonly record struct LegendEntry(
    string Label, LvcColor Swatch, Option<double> At, Seq<(string Header, string Value)> Stats);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LegendRender {
    private LegendRender() { }
    public sealed record Package(IChartLegend Legend, LegendPosition At) : LegendRender;
    public sealed record Drawn(CustomVisualData Data, LegendDock Dock, Option<(double X, double Y)> Offset) : LegendRender;
}
```

## [10]-[LEGEND_FOLD]

- Owner: `LegendFold` — the one entry resolution, the printed-value projection, the arm dispatch, and the drag writes.
- Entry: `Entries(spec, chart, ink, rows, locale)` — the one entry resolution every arm reads; `Render(spec, entries, style, locale)` — the arm dispatch on `LegendSpec.Arm`; `Rendered(value, measure, locale)` — the one printed-value projection, public because the heat arm's `Formatter` IS this projection; `Dragged(spec, by)` / `Reset(spec)` — the drag accumulation and the board-wide clear under `LegendSpec.ResetIntent`.
- Auto: series entries take their swatch from the SAME ramp position the chart draws with, because the theme indexes its palette by series id and a legend that re-sampled the colormap would drift one slot on every re-tint; ramp entries are the sampled stops themselves, so the legend's swatches and the surface's ramp are one generation read twice; stepped entries read the covering band set every threshold projection already paints.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, SkiaSharp, UnitsNet, LanguageExt.Core
- Growth: a new domain arm's entries are one `Entries` arm; zero new surface.
- Boundary: statistics reduce the layer's OWN rows through the settled reducer over the one ordered substrate `Charts/streams.md` publishes, so a legend column and a chart transform answer one number, and a layer with no rows answers an empty column rather than a zero — a reduction of nothing is absence. The arm dispatch reads `LegendSpec.Arm` and nothing else, so the admission's verdict and the render's routing cannot disagree; a corner-docked ramp draws its sampled stops as the swatch ladder because no package arm reads a drag offset. The colour hops at this fold's two edges — the chart's byte swatch into an entry, an entry's swatch into the wide-gamut float the custom plane carries — are `ChartInk`'s own correspondence members, so the legend spells no channel arithmetic of its own and the quantization boundary stays at the one ink owner. The theme's OWN factory products cross: the default legend arrives through the registered theme so it inherits the easing, speed, and background the one `LiveCharts.Configure` chained, and the heat legend's `Formatter` is the one place the elected quantity lands — this arm cannot print a bound the series does not measure, which is why a clamp belongs on the data.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LegendFold {
    public static Fin<Seq<LegendEntry>> Entries(
        LegendSpec spec, ChartSpec chart, ChartInk ink, Seq<ChartDatum> rows, ResolvedLocale locale) =>
        spec.Domain.Switch(
            state: (Spec: spec, Chart: chart, Ink: ink, Rows: rows, Locale: locale),
            series: static (s, _) => s.Chart.Layers.Filter(static layer => layer.Toggleable)
                .Traverse((layer, index) => Statistics(s.Spec, layer, s.Rows, s.Locale)
                    .Map(stats => new LegendEntry(
                        layer.Name,
                        layer.Ink.Match(Some: s.Ink.Tint, None: () => s.Ink.Palette[index % s.Ink.Palette.Length]),
                        None,
                        stats)))
                .As(),
            continuous: static (s, row) => Sampled(s.Spec, row.Low, row.High, s.Ink, s.Locale),
            stepped: static (s, row) => row.List.Edges(row.Low, row.High)
                .Traverse(edge => Rendered(edge.From, s.Spec.Measure, s.Locale)
                    .Map(label => new LegendEntry(label, s.Ink.Lvc(s.Ink.Shade(edge.Severity)), Some(edge.From), Seq<(string, string)>())))
                .As(),
            categorized: static (s, row) => row.Members
                .Traverse((member, index) => Fin.Succ(new LegendEntry(
                    member.Label, s.Ink.Palette[index % s.Ink.Palette.Length], Some(member.At), Seq<(string, string)>())))
                .As(),
            ordinal: static (s, row) => toSeq(row.Dictionary.OrderBy(static entry => entry.Key))
                .Traverse((entry, index) => Fin.Succ(new LegendEntry(
                    entry.Value, s.Ink.Palette[index % s.Ink.Palette.Length], Some(entry.Key), Seq<(string, string)>())))
                .As());

    public static Fin<LegendRender> Render(
        LegendSpec spec, Seq<LegendEntry> entries, CustomVisualStyle style, ChartInk ink, ResolvedLocale locale) =>
        spec.Arm == LegendArm.Swatches
            ? Fin.Succ<LegendRender>(new LegendRender.Package(
                Swatches(), spec.Dock == LegendDock.Hidden ? LegendPosition.Hidden : spec.Dock.Side))
        : spec.Arm == LegendArm.Ramp
            ? Fin.Succ<LegendRender>(new LegendRender.Package(Ramp(spec, locale), spec.Dock.Side))
        : entries
                .Traverse(entry => entry.At.Match(
                        Some: at => Rendered(at, spec.Measure, locale).Map(Some),
                        None: static () => Fin.Succ(Option<string>.None))
                    .Map(spelled => (entry.Label, Swatch: ink.Wide(entry.Swatch), At: spelled, entry.Stats)))
                .As()
                .Map(rows => (LegendRender)new LegendRender.Drawn(
                    new CustomVisualData(
                        $"legend:{spec.Key}",
                        new VisualPayload.Legend(rows, Vertical: spec.Dock.Vertical),
                        style),
                    spec.Dock,
                    spec.Offset));

    static Fin<Seq<(string Header, string Value)>> Statistics(
        LegendSpec spec, ChartLayer layer, Seq<ChartDatum> rows, ResolvedLocale locale) =>
        rows.Filter(datum => datum.Group == layer.Name) switch {
            var owned when owned.IsEmpty => Fin.Succ(Seq<(string, string)>()),
            var owned => GroupSpread.Of(owned, spec.Columns.Map(static column => column.Tau))
                .Bind(spread => spec.Columns.Traverse(column =>
                    Rendered(column.Reducer.Reduce(spread, column.Tau).A, column.Measure, locale)
                        .Map(value => (Header: locale.Label(column.HeaderKey), Value: value))).As()),
        };

    static Fin<Seq<LegendEntry>> Sampled(LegendSpec spec, double low, double high, ChartInk ink, ResolvedLocale locale) =>
        toSeq(Range(0, spec.Segments))
            .Traverse(step => (low + ((high - low) * step / (spec.Segments - 1))) switch {
                var at => Rendered(at, spec.Measure, locale).Map(label =>
                    new LegendEntry(label, ink.Lvc(ink.Ramp[step * (ink.Ramp.Length - 1) / (spec.Segments - 1)]), Some(at), Seq<(string, string)>())),
            })
            .As();

    public static Fin<string> Rendered(double value, Option<MeasureRole> measure, ResolvedLocale locale) =>
        measure.Match(
            Some: role => Quantity.TryFrom(value, role.MetricUnit, out IQuantity? quantity) && quantity is not null
                ? locale.Quantity(quantity, role)
                : Fin.Succ(locale.Text(ChartAxisKind.Numeric.Format, value)),
            None: () => Fin.Succ(locale.Text(ChartAxisKind.Numeric.Format, value)));

    public static LegendSpec Dragged(LegendSpec spec, (double X, double Y) by) =>
        spec with { Offset = Some(spec.Offset.Match(Some: at => (at.X + by.X, at.Y + by.Y), None: () => by)) };

    public static LegendSpec Reset(LegendSpec spec) => spec with { Offset = None };

    static IChartLegend Swatches() => LiveCharts.DefaultSettings.GetTheme().GetDefaultLegend();

    static IChartLegend Ramp(LegendSpec spec, ResolvedLocale locale) => new SKHeatLegend {
        Formatter = value => Rendered(value, spec.Measure, locale).IfFail(_ => string.Empty),
    };
}
```

## [11]-[RESEARCH]

(none)
