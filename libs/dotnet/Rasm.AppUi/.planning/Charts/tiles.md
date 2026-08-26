# [APPUI_CHARTS_TILES]

The dashboard tile spine: one closed `DashboardTile` union over one `TileSource` axis, one `TileRender` product union beside one `TileState` lifecycle, one mount fold dispatching every case onto its admitted arm, and the watch rows that turn a board into a monitor over the same aggregate spine the tiles bind. Ink and faults arrive from `Charts/ink.md`, streams and reducers from `Charts/streams.md`, the compliance profile from `Charts/ink.md`, and the brush channel lives at `Charts/boards.md`.

## [01]-[INDEX]

- [02]-[SOURCE_AXIS]: The one source axis, the aggregate vocabulary, the drop ladder, and delta polarity.
- [03]-[TILE_SPINE]: Lifecycle, presentation postures, the scalar reading, placement, the tile union, and layout admission.
- [04]-[TILE_MOUNT]: The one bind fold, its products, the pace write, and the sparkline primitive.
- [05]-[WATCH_RULES]: Edge-triggered alert rows with hold, quiet, and staleness over the stat spine.
- [06]-[RESEARCH]: Open verification rows.

## [02]-[SOURCE_AXIS]

- Owner: `StatSample` — the weighted element every scalar fold reduces; `StatFold` — the aggregate vocabulary whose rows carry the DynamicData fold as delegate data; `TileSource` — the ONE closed source axis with its `SourceArm` classification column and its own admission; `TileDrop` — the element-drop ladder; `DeltaPolarity` — the tile's own reading of a delta's sign.
- Cases: `TileSource` = Folded | Derived | Streamed | Rows | Composed; `SourceArm` = scalar | series | rows | bundle; `TileDrop` = legend | data-labels | axis-labels | separators | title, dropped in that order; `DeltaPolarity` = higher-is-better | lower-is-better | neutral.
- Entry: `TileSource.Admit()` — the arm's own shape proof (a bundle is non-empty, slot-distinct, scalar-only, one level deep); `TileSource.Arm` — the classification every gate reads; `StatFold.Fold(source, value)` — the one aggregate bind; `TileDrop.Through(dropped)` — the dropped-element projection a cramped chrome reads.
- Auto: a `Folded` row crosses the livedata scalar-fold edge carrying the `StatFold` ROW itself, so a tile statistic is recoverable from its declaration and a bind-edge aggregate lambda is the deleted form; a multi-accumulator row folds every accumulator inside one `ForAggregation` scan, because a second subscription over the same feed publishes each accumulator against a different revision; every folded element carries its own population through `StatSample.Weight`, so a feed of pre-reduced rows binds `Weighted` and a feed of raw observations binds `Average` — an unweighted mean over bucket means answers the mean of buckets, and the tile would render that answer under the caption of the other.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, DynamicData, System.Reactive
- Growth: a new source class is one `TileSource` arm declaring its `SourceArm` side; a new statistic is one `StatFold` row; a new drop rank is one `TileDrop` row; zero new surface.
- Boundary: tile SOURCING is one axis — a scalar tile names a fold over a feed or a projection already reduced upstream (`Derived` carries the key naming its producer; the SLO burn rate is the standing instance), a custom tile names a feed with its transform rows, a table tile names a row source, and a CHART names none because its layers each carry one, so a tile's data is recoverable from its declaration on every arm; `TileSource.Rows(SourceKey)` IS the tables port vocabulary and its producer is the named `TableSourcePort`, so the table tile's source names a real owner at both ends; the `Composed` arm is the scalar-set bundle a compliance scorecard reads, each part bound through the same scalar arm a stat tile takes, so the bundle adds a shape and no second subscription path; classification is the `Arm` COLUMN and shape validity is the union's own `Admit`, so a sixth arm lands its side and its proof as one declaration and the retired `bool Scalar`/`bool Bundle` pair — one a derived flag, one an admission wearing a property's name — has no spelling left.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct StatSample(double Value, double Weight) {
    public static StatSample One(double value) => new(value, 1d);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StatFold {
    public static readonly StatFold Count = new("count", static (source, _) => source.Count().Select(static n => (double)n));
    public static readonly StatFold Sum = new("sum", static (source, value) => source.Sum(value));
    public static readonly StatFold Average = new("average", static (source, value) => source.Avg(value));
    public static readonly StatFold Weighted = new("weighted", static (source, value) =>
        source.ForAggregation()
            .Scan(
                (Numerator: 0d, Mass: 0d),
                (fold, changes) => changes.Aggregate(
                    fold,
                    (state, item) => item.Type == AggregateType.Add
                        ? (Numerator: state.Numerator + (value(item.Item) * item.Item.Weight), Mass: state.Mass + item.Item.Weight)
                        : (Numerator: state.Numerator - (value(item.Item) * item.Item.Weight), Mass: state.Mass - item.Item.Weight)))
            .Select(static fold => fold.Mass <= 0d ? 0d : fold.Numerator / fold.Mass));
    public static readonly StatFold Minimum = new("minimum", static (source, value) => source.Minimum(value));
    public static readonly StatFold Maximum = new("maximum", static (source, value) => source.Maximum(value));
    public static readonly StatFold Deviation = new("deviation", static (source, value) => source.StdDev(value, fallbackValue: 0d));

    [UseDelegateFromConstructor]
    public partial IObservable<double> Fold(IObservable<IChangeSet<StatSample, string>> source, Func<StatSample, double> value);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SourceArm {
    public static readonly SourceArm Scalar = new("scalar");
    public static readonly SourceArm Series = new("series");
    public static readonly SourceArm Rows = new("rows");
    public static readonly SourceArm Bundle = new("bundle");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileSource {
    private TileSource() { }
    public sealed record Folded(StatFold Fold, ChartStream Stream) : TileSource;
    public sealed record Derived(string Projection, IObservable<double> Values) : TileSource;
    public sealed record Streamed(ChartStream Stream, Seq<TransformRow> Transforms) : TileSource;
    public sealed record Rows(string SourceKey) : TileSource;
    public sealed record Composed(Seq<(string Slot, TileSource Part)> Parts) : TileSource;

    public SourceArm Arm => Switch(
        folded: static _ => SourceArm.Scalar, derived: static _ => SourceArm.Scalar,
        streamed: static _ => SourceArm.Series, rows: static _ => SourceArm.Rows,
        composed: static _ => SourceArm.Bundle);

    public Fin<TileSource> Admit(string tile) => Switch(
        state: (Self: this, Tile: tile),
        folded: static (s, _) => Fin.Succ(s.Self),
        derived: static (s, _) => Fin.Succ(s.Self),
        streamed: static (s, _) => Fin.Succ(s.Self),
        rows: static (s, _) => Fin.Succ(s.Self),
        composed: static (s, row) => !row.Parts.IsEmpty
            && row.Parts.ForAll(static part => part.Part.Arm == SourceArm.Scalar)
            && row.Parts.Map(static part => part.Slot).Distinct().Count == row.Parts.Count
            ? Fin.Succ(s.Self)
            : Fin.Fail<TileSource>(new ChartFault.SourceMismatch(s.Tile)));
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileDrop {
    public static readonly TileDrop Legend = new("legend", rank: 0);
    public static readonly TileDrop DataLabels = new("data-labels", rank: 1);
    public static readonly TileDrop AxisLabels = new("axis-labels", rank: 2);
    public static readonly TileDrop Separators = new("separators", rank: 3);
    public static readonly TileDrop Title = new("title", rank: 4);

    public int Rank { get; }

    public static Seq<TileDrop> Through(int dropped) => toSeq(Items).Filter(row => row.Rank < dropped);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeltaPolarity {
    public static readonly DeltaPolarity HigherIsBetter = new("higher-is-better",
        static delta => delta > 0d ? Severity.Nominal : delta < 0d ? Severity.Warning : Severity.Info);
    public static readonly DeltaPolarity LowerIsBetter = new("lower-is-better",
        static delta => delta < 0d ? Severity.Nominal : delta > 0d ? Severity.Warning : Severity.Info);
    public static readonly DeltaPolarity Neutral = new("neutral", static _ => Severity.Info);

    [UseDelegateFromConstructor]
    public partial Severity Reading(double delta);
}
```

## [03]-[TILE_SPINE]

- Owner: `TileState` — the per-tile lifecycle union; `TilePosture` — the presentation vocabulary each state projects onto, carrying opacity, veil, and interactivity as row columns; `TilePresentation` — the projected posture beside the drop depth and badge; `StatAnatomy` — the scalar tile's full reading; `TilePlacement` and `DashboardLayout` — the breakpoint-indexed board spine; `DashboardTile` — the closed tile union and its admission.
- Cases: `TileState` = Loading | Ready | Empty | Failed | Cramped; `TilePosture` = live | veiled | inert | faulted; `DashboardTile` = Chart | Stat | Gauge | Table | Scorecard | Custom.
- Entry: `TileState.Present` — the one presentation projection; `StatAnatomy.Folded(label, polarity, held, taus)` — the whole reading off ONE retained window; `DashboardLayout.Admit(key, placements, canvasState)` — accumulating admission with a per-tier sweep; `DashboardLayout.At(breakpoint)` — the widest-declared-tier resolution; `DashboardTile.Admit()` — the per-case source-arm proof.
- Auto: presentation is a projection of the state, so no tile decides its own opacity, veil, or interactivity — four postures resolve from five states and a sixth state lands as one arm; the scalar tile's headline, delta, spark, and declared percentiles are one retained window read four ways, so they cannot disagree; a gauge holds no window because a dial shows one reading and a trend under it states a second the dial cannot draw.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new lifecycle posture is one `TileState` case with its `Present` arm; a new tile kind is one `DashboardTile` case with its admission arm; zero new surface.
- Boundary: LOADING holds the prior reading on its own `Held` column so a refresh veils a live frame rather than blanking a board a viewer is reading — retention is the state's own payload, never a posture flag a presentation would re-derive; EMPTY states the gap in words rather than drawing a zero the feed never carried; FAILED carries the typed fault AND the kernel redrive `Verdict` the mount settled off the fault's own `Retriability`, so the chrome renders "retrying after the stated delay", "abandoned after the bound", or "terminal" from one typed answer and the retired bare `Option<Instant> Retry` — a retry instant with no policy and no retriability discriminant — is unspellable; CRAMPED carries the drop depth the placement fold resolved, and its producer is `DashboardSurface.Squeezed`, so the cramped arm has a real mint rather than a case nothing constructs. Layout admission accumulates every defect through `Validation` and proves tier overlap by a column-ordered sweep against the open placements — the quadratic every-pair fold re-materializing its tier per comparison is gone. The trend CAPTION is not a column on the reading: it is a phrase over `Delta` and `Polarity` spelled at the one render site holding a locale. `Percentiles` is the tile's own declared tau roster, because which quantiles matter is the tile's question — a latency tile states p95 and a count tile states none.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TilePosture {
    public static readonly TilePosture Live = new("live", opacity: 1d, veil: false, interactive: true);
    public static readonly TilePosture Veiled = new("veiled", opacity: 0.55d, veil: true, interactive: false);
    public static readonly TilePosture Inert = new("inert", opacity: 1d, veil: false, interactive: false);
    public static readonly TilePosture Faulted = new("faulted", opacity: 0.65d, veil: true, interactive: true);

    public double Opacity { get; }
    public bool Veil { get; }
    public bool Interactive { get; }
}

public readonly record struct TilePresentation(TilePosture Posture, int Dropped, Severity Badge);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileState {
    private TileState() { }

    public sealed record Loading(Option<StatAnatomy> Held, Instant Since) : TileState;
    public sealed record Ready(Instant At) : TileState;
    public sealed record Empty(string Reason) : TileState;
    public sealed record Failed(Error Fault, Verdict Redrive) : TileState;
    public sealed record Cramped(int Dropped) : TileState;

    public TilePresentation Present => Switch(
        loading: static _ => new TilePresentation(TilePosture.Veiled, 0, Severity.Info),
        ready: static _ => new TilePresentation(TilePosture.Live, 0, Severity.Nominal),
        empty: static _ => new TilePresentation(TilePosture.Inert, 0, Severity.Info),
        failed: static _ => new TilePresentation(TilePosture.Faulted, 0, Severity.Critical),
        cramped: static row => new TilePresentation(TilePosture.Live, row.Dropped, Severity.Nominal));
}

public sealed record StatAnatomy(
    string Label,
    double Value,
    Option<double> Delta,
    DeltaPolarity Polarity,
    Seq<double> Spark,
    Seq<(double Tau, double Value)> Percentiles) {
    public const int Window = 64;

    public static StatAnatomy Of(string label, double value, DeltaPolarity polarity) =>
        new(label, value, None, polarity, Seq(value), Seq<(double, double)>());

    public static StatAnatomy Folded(string label, DeltaPolarity polarity, Seq<double> held, Seq<double> taus) =>
        held.IsEmpty
            ? Of(label, 0d, polarity)
            : new(label, held.Last, Change(held.Last, Some(held.Head)), polarity, held,
                GroupSpread.Of(held, taus)
                    .Map(spread => taus.Map(tau => (Tau: tau, Value: ChartReducer.Quantile.Reduce(spread, tau).A)))
                    .IfFail(Seq<(double, double)>()));

    public Severity Reading => Delta.Match(Some: Polarity.Reading, None: static () => Severity.Info);

    public static Option<double> Change(double current, Option<double> prior) =>
        prior.Bind(before => Math.Abs(before) > EpsilonPolicy.ZeroTolerance ? Some((current - before) / Math.Abs(before)) : None);
}

public readonly record struct TilePlacement(string TileKey, BreakpointRow At, int Column, int Row, int ColumnSpan, int RowSpan) {
    public bool Overlaps(TilePlacement other) =>
        At == other.At
            && Column < (long)other.Column + other.ColumnSpan
            && (long)Column + ColumnSpan > other.Column
            && Row < (long)other.Row + other.RowSpan
            && (long)Row + RowSpan > other.Row;
}

public sealed record DashboardLayout(string Key, Seq<TilePlacement> Placements, Option<string> CanvasState) {
    public static Fin<DashboardLayout> Admit(string key, Seq<TilePlacement> placements, Option<string> canvasState = default) =>
        (Gate(!string.IsNullOrWhiteSpace(key), $"{key}: blank key"),
         Gate(placements.ForAll(static placement =>
                 !string.IsNullOrWhiteSpace(placement.TileKey)
                 && placement.Column >= 0 && placement.Row >= 0
                 && placement.ColumnSpan > 0 && placement.RowSpan > 0),
             $"{key}: degenerate placement"),
         toSeq(placements.GroupBy(static placement => placement.At))
             .Traverse(tier => Tier(key, toSeq(tier))).Map(static _ => unit).As())
            .Apply((_, _, _) => new DashboardLayout(key, placements, canvasState)).As().ToFin();

    static Validation<Error, Unit> Tier(string key, Seq<TilePlacement> tier) =>
        tier.Map(static placement => placement.TileKey).Distinct().Count == tier.Count
            && Swept(tier)
            ? unit
            : (Validation<Error, Unit>)(Error)new ChartFault.PlacementRejected(key);

    static bool Swept(Seq<TilePlacement> tier) {
        Seq<TilePlacement> ordered = toSeq(tier.OrderBy(static placement => placement.Column));
        Seq<TilePlacement> open = Seq<TilePlacement>();
        foreach (TilePlacement candidate in ordered) {
            open = open.Filter(held => held.Column + held.ColumnSpan > candidate.Column).Strict();
            if (open.Exists(candidate.Overlaps)) { return false; }
            open = open.Add(candidate);
        }
        return true;
    }

    public Seq<TilePlacement> At(BreakpointRow at) =>
        AdaptiveLayout.Rows.Filter(row => row.MinWidth <= at.MinWidth)
            .Fold(Seq<TilePlacement>(), (held, row) => Placements.Filter(placement => placement.At == row) switch {
                var tier => tier.IsEmpty ? held : tier,
            });

    static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? unit : (Validation<Error, Unit>)(Error)new ChartFault.PlacementRejected(detail);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DashboardTile {
    private DashboardTile() { }

    public sealed record Chart(string Key, ChartSpec Spec) : DashboardTile;

    public sealed record Stat(
        string Key, string Label, DeltaPolarity Polarity, TileSource Source, Seq<double> Percentiles = default) : DashboardTile;

    public sealed record Gauge(string Key, double Floor, double Ceiling, Option<ThresholdList> Steps, TileSource Source) : DashboardTile;

    public sealed record Table(string Key, TileSource Source) : DashboardTile;

    public sealed record Scorecard(string Key, ConstraintProfile Profile, TileSource Source) : DashboardTile;

    public sealed record Custom(string Key, CustomVisual Kind, TileSource Source) : DashboardTile;

    public string Key => Switch(
        chart: static row => row.Key, stat: static row => row.Key, gauge: static row => row.Key,
        table: static row => row.Key, scorecard: static row => row.Key, custom: static row => row.Key);

    public Fin<DashboardTile> Admit() => Switch(
        chart: static row => ChartSpec.Admit(row.Spec).Map(spec => (DashboardTile)(row with { Spec = spec })),
        stat: static row => Armed(row, row.Source, SourceArm.Scalar),
        gauge: static row => Armed(row, row.Source, SourceArm.Scalar),
        table: static row => Armed(row, row.Source, SourceArm.Rows),
        scorecard: static row => row.Source is TileSource.Composed bundle
            && row.Profile.Rows.ForAll(check => bundle.Parts.Exists(part => part.Slot == check.Metric))
            ? row.Source.Admit(row.Key)
                .Bind(_ => ConstraintProfile.Admit(row.Profile).Map(profile => (DashboardTile)(row with { Profile = profile })))
            : Refused(row.Key),
        custom: static row => Armed(row, row.Source, SourceArm.Series));

    static Fin<DashboardTile> Armed(DashboardTile tile, TileSource source, SourceArm needs) =>
        source.Arm == needs ? source.Admit(tile.Key).Map(_ => tile) : Refused(tile.Key);

    static Fin<DashboardTile> Refused(string key) => Fin.Fail<DashboardTile>(new ChartFault.SourceMismatch(key));
}
```

## [04]-[TILE_MOUNT]

- Owner: `TileMount` — the composition-bound tile context; `TileRender` — the ONE product union every bound tile publishes beside its lifecycle; `TableSourcePort` and `TableSourceBinding` — the tables port spelled as data; `DashboardSurface` — the one bind fold, the pace write, and the cramped producer; `BoardPace` — the hold posture; `Sparkline` — the axis-less primitive tiles and table cells share.
- Cases: `TileRender` = Scalar | Series | Rows | Card; `BoardPace` = live | held.
- Entry: `DashboardSurface.Resolve(layout, at, tiles)` — placement-to-tile resolution, `Fin` on the first unresolved key; `DashboardSurface.Mount(mount, tile)` — the ONE tile bind; `DashboardSurface.Pace(chart, ink, pace)` — the hold write on the package's own gating members; `DashboardSurface.Squeezed(width, height, theme)` — the cramped-state producer off the drop ladder; `Sparkline.Render(values, ink, stroke, info)` — the offscreen chrome-suppressed chart `Editing/tables.md` cells and stat cards share.
- Auto: every arm publishes its own `TileRender` product beside its lifecycle through the mount's two sinks, so a bound tile that delivered nothing but `Ready` is unspellable; a chart's mount opens ONE stream per non-pinned expanded layer and combines them into one frame, so a carpet layer's calendar fold and a ghost layer's alignment shift actually run; the scalar arm retains its window per BIND, so the reading dies with the subscription that opened it.
- Packages: LiveChartsCore.SkiaSharpView.Avalonia, SkiaSharp, LanguageExt.Core, DynamicData, System.Reactive, NodaTime
- Growth: a new tile product is one `TileRender` arm the bind edge dispatches on; a new pace posture is one `BoardPace` row; zero new surface.
- Boundary: the mount's two sinks take the tile's product KEYED by the tile that made it and its lifecycle — a sink per product class is the shape that let a tile publish a state and swallow its data. Per-card state lives in the CARD's own fold — the scorecard's readings cell mints per card inside its bind, exactly as the stat ring mints per subscription — because a mount-held buffer is shared by every tile the mount binds and two scorecards sharing a slot name would collide (the retired mount dictionary was that defect and the reason the ring's own law names per-subscription state). A refusal is a tile STATE carrying the settled redrive verdict, not a silent no-op subscription, so a mis-declared source is visible on the board that carries it. Pause and hold ride the package's own gating members — `AutoUpdateEnabled` stops the redraw pass, `Paint.IsPaused` freezes animations mid-transition — so a held tile keeps its last frame and a board-local render-suppression flag is the deleted form. The one clock a mount reads is the capsule's own scheduler, so a proof lane's virtual time paces `Ready` stamps and the wall clock enters nowhere. A `Gauge` fold lands on the materialized `XamlGaugeSeries.GaugeValue` with `Invalidate` refreshing the series — never a re-created series per sample. The SPARKLINE is the package's own offscreen cartesian chart with every chrome slot suppressed as one projection over the axis-chrome write set — a custom-plane Skia sparkline is the deleted form, because the chart plane's admission law rejects a bespoke Skia surface drawing chart semantics. Board capture projects to `SKImage` and hands off to the offscreen encode rows; each named board's headless render hash returns `VisualArtifact`, which `BoardTelemetry` measures (`Charts/boards.md`).

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
public sealed record TileMount(
    BindingCapsule Capsule,
    ChartInk Ink,
    BoardContext Context,
    CalendarPolicy Calendar,
    ResolvedLocale Locale,
    Func<ChartStream, IObservable<IChangeSet<ChartDatum, string>>> Feed,
    TableSourcePort Tables,
    RedrivePolicy Redrive,
    Action<string, TileRender> Render,
    Action<TileState> State);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileRender {
    private TileRender() { }

    public sealed record Scalar(StatAnatomy Reading) : TileRender;
    public sealed record Series(Seq<(string Layer, Seq<ChartDatum> Rows)> Layers) : TileRender;
    public sealed record Rows(TableSourceBinding Binding) : TileRender;
    public sealed record Card(Seq<ConstraintVerdict> Verdicts) : TileRender;
}

public sealed record TableSourcePort(Func<string, Fin<TableSourceBinding>> Resolve);

public sealed record TableSourceBinding(string SourceKey, IObservable<IChangeSet<object, string>> Rows, Seq<string> ColumnKeys);

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoardPace {
    public static readonly BoardPace Live = new("live", paused: false);
    public static readonly BoardPace Held = new("held", paused: true);

    public bool Paused { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DashboardSurface {
    public static Fin<Seq<(TilePlacement Placement, DashboardTile Tile)>> Resolve(
        DashboardLayout layout, BreakpointRow at, HashMap<string, DashboardTile> tiles) =>
        layout.At(at)
            .TraverseM(placement => tiles.Find(placement.TileKey) is { IsSome: true, Case: DashboardTile tile }
                ? tile.Admit().Map(admitted => (Placement: placement, Tile: admitted))
                : Fin.Fail<(TilePlacement Placement, DashboardTile Tile)>(new ChartFault.MissingTile(placement.TileKey)))
            .As();

    public static IDisposable Mount(TileMount mount, DashboardTile tile) =>
        tile.Switch(
            state: mount,
            chart: static (s, row) => Layered(s, row.Key, Some(row.Spec), None),
            stat: static (s, row) => Ring(StatAnatomy.Window) switch {
                var retained => Scalar(s, row.Source, row.Key, value => s.Render(row.Key,
                    new TileRender.Scalar(StatAnatomy.Folded(row.Label, row.Polarity, retained(value), row.Percentiles)))),
            },
            gauge: static (s, row) => Scalar(s, row.Source, row.Key, value => s.Render(row.Key,
                new TileRender.Scalar(StatAnatomy.Of(row.Key, Math.Clamp(value, row.Floor, row.Ceiling), DeltaPolarity.Neutral)))),
            table: static (s, row) => row.Source is TileSource.Rows rows
                ? s.Tables.Resolve(rows.SourceKey).Match(
                    Succ: binding => binding.Rows.ObserveOn(s.Capsule.Ui).Subscribe(
                        _ => { s.Render(row.Key, new TileRender.Rows(binding)); s.State(Ready(s)); },
                        raw => s.State(Failed(s, Error.New(raw.Message, raw)))),
                    Fail: error => Refused(s, error))
                : Refused(s, new ChartFault.SourceMismatch(row.Key)),
            scorecard: static (s, row) => Card(s, row),
            custom: static (s, row) => Layered(s, row.Key, None, Some(row.Source)));

    static IDisposable Card(TileMount mount, DashboardTile.Scorecard tile) {
        if (tile.Source is not TileSource.Composed bundle) { return Refused(mount, new ChartFault.SourceMismatch(tile.Key)); }
        Atom<HashMap<string, double>> readings = Atom(HashMap<string, double>());
        return new CompositeDisposable(bundle.Parts.Map(part => Scalar(mount, part.Part, $"{tile.Key}/{part.Slot}", value => {
            HashMap<string, double> held = readings.Swap(current => current.AddOrUpdate(part.Slot, value));
            if (tile.Profile.Rows.ForAll(check => held.ContainsKey(check.Metric))) {
                mount.Render(tile.Key, new TileRender.Card(tile.Profile.Pressure(
                    tile.Profile.Rows.Map(check => check.Read(held[check.Metric], tile.Profile.Grade)), tile.Profile.Rows.Count)));
                mount.State(Ready(mount));
            }
        })));
    }

    static IDisposable Layered(TileMount mount, string key, Option<ChartSpec> spec, Option<TileSource> source) =>
        Streams(mount, key, spec, source).Match(
            Succ: streams => streams.IsEmpty
                ? Pinned(mount, key)
                : Observable.CombineLatest(streams.Map(static row => row.Rows))
                    .ObserveOn(mount.Capsule.Ui)
                    .Subscribe(
                        frame => {
                            mount.Render(key, new TileRender.Series(streams.Map(static row => row.Layer).Zip(toSeq(frame))));
                            mount.State(Ready(mount));
                        },
                        raw => mount.State(Failed(mount, Error.New(raw.Message, raw)))),
            Fail: error => Refused(mount, error));

    static Fin<Seq<(string Layer, IObservable<Seq<ChartDatum>> Rows)>> Streams(
        TileMount mount, string key, Option<ChartSpec> spec, Option<TileSource> source) =>
        spec.Match(
            Some: declared => declared.Expand(mount.Locale).Map(expanded => expanded.Layers
                .Filter(static layer => !layer.Literal)
                .Map(layer => (layer.Name, Piped(mount, layer.Stream, layer.Transforms)))),
            None: () => source.Case is TileSource.Streamed streamed
                ? Fin.Succ(Seq((key, Piped(mount, streamed.Stream, streamed.Transforms))))
                : Fin.Fail<Seq<(string, IObservable<Seq<ChartDatum>>)>>(new ChartFault.SourceMismatch(key)));

    static IObservable<Seq<ChartDatum>> Piped(TileMount mount, ChartStream stream, Seq<TransformRow> rows) =>
        ChartFolds.Snapshots(stream, ChartFolds.Shape(stream, mount.Feed(stream)), rows, mount.Calendar);

    static IDisposable Pinned(TileMount mount, string key) {
        mount.Render(key, new TileRender.Series(Seq<(string, Seq<ChartDatum>)>()));
        mount.State(Ready(mount));
        return Disposable.Empty;
    }

    static TileState Ready(TileMount mount) => new TileState.Ready(Instant.FromDateTimeOffset(mount.Capsule.Ui.Now));

    static TileState Failed(TileMount mount, Error error) =>
        new TileState.Failed(error, Redrive.Settle(mount.Redrive, error, attempt: 0));

    static IDisposable Scalar(TileMount mount, TileSource source, string key, Action<double> render) =>
        source.Switch(
            state: (Mount: mount, Key: key, Render: render),
            folded: static (s, f) => s.Mount.Capsule.Scalar(
                s.Mount.Feed(f.Stream).Transform(static datum => datum.Sample), f.Fold, static sample => sample.Value, s.Render),
            derived: static (s, d) => d.Values
                .ObserveOn(s.Mount.Capsule.Ui)
                .Subscribe(s.Render, raw => s.Mount.Capsule.Fault(Error.New(raw.Message, raw))),
            streamed: static (s, _) => Refused(s.Mount, new ChartFault.SourceMismatch(s.Key)),
            rows: static (s, _) => Refused(s.Mount, new ChartFault.SourceMismatch(s.Key)),
            composed: static (s, _) => Refused(s.Mount, new ChartFault.SourceMismatch(s.Key)));

    static Func<double, Seq<double>> Ring(int window) {
        Seq<double> retained = Seq<double>();
        return value => retained = (retained.Count >= window ? retained.Tail : retained).Add(value);
    }

    static IDisposable Refused(TileMount mount, Error error) {
        mount.State(Failed(mount, error));
        return Disposable.Empty;
    }

    public static Unit Pace(SourceGenChart chart, ChartInk ink, BoardPace pace) {
        chart.AutoUpdateEnabled = !pace.Paused;
        toSeq(ChartChrome.Items).Iter(chrome => ink.Paint(chrome).IsPaused = pace.Paused);
        return unit;
    }

    public static Option<TileState> Squeezed(double width, double height, ResolvedTheme theme) =>
        theme.Metric(MetricFamily.Extent, 4).Bind(unit =>
            toSeq(TileDrop.Items).Count(row => width < unit * (row.Rank + 2) || height < unit * (row.Rank + 1)) switch {
                0 => Option<TileState>.None,
                var dropped => Some<TileState>(new TileState.Cramped(dropped)),
            });
}

public static class Sparkline {
    public static Fin<SKImage> Render(Seq<double> values, ChartInk ink, ChartChrome stroke, SKImageInfo info) =>
        values.Count < 2
            ? Fin.Fail<SKImage>(new ChartFault.VisualEmpty("sparkline"))
            : Op.Of(name: "appui.chart.sparkline").Catch(() => Fin.Succ(new SKCartesianChart {
                Width = info.Width,
                Height = info.Height,
                Background = SKColors.Transparent,
                DrawMarginFrame = null,
                Series = [new LineSeries<ChartDatum> {
                    Values = values.Map(static (value, index) => ChartDatum.Point(index, value)).ToList(),
                    Mapping = static (datum, _) => ChartEncoding.Xy.Of(datum),
                    Stroke = ink.Paint(stroke),
                    Fill = null,
                    GeometrySize = 0d,
                    EnableNullSplitting = true,
                    IsHoverable = false,
                }],
                XAxes = [Suppressed()],
                YAxes = [Suppressed()],
            }.GetImage()));

    static Axis Suppressed() {
        Axis bare = new() { IsVisible = false, ShowSeparatorLines = false };
        AxisChrome.Suppress(bare);
        return bare;
    }
}
```

## [05]-[WATCH_RULES]

- Owner: `WatchBound` and `WatchSample` — the bound pair and the value-with-age carrier every comparator reads; `WatchComparator` — the breach vocabulary; `WatchRule` — one alert row; `WatchCrossing` — the raised fact; `WatchFold` — the probed sample stream and the armed subscription; `FeedHealth` and `FeedFreshness` — the consumer end of the feed-freshness port.
- Cases: `WatchComparator` = above | below | outside | stale; `FeedHealth` = live | degraded | reconnecting | stalled.
- Entry: `WatchFold.Samples(stat, probe, scheduler)` — the probed stream that advances age without the feed; `WatchFold.Arm(rule, stat, scheduler, raise, fault)` — one armed subscription per rule; `WatchFold.Worst(live)` — the badge fold over the ranked family.
- Auto: a crossing is a breach-state EDGE that must HOLD through `PendingFor` before it raises and is then suppressed for `Quiet`, so a flapping aggregate raises once rather than per oscillation; the crossing raises the rule's `ToastIntent` through the CommandRow table, so `DeckOutcome` carries the durable command result and the alert vocabulary remains rule DATA over the one aggregate spine.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, System.Reactive, NodaTime
- Growth: a new breach posture is one `WatchComparator` row; a new alert is one `WatchRule` value; warn and critical on one tile are two rows of one family ordered by rank; zero new surface.
- Boundary: the STALE comparator closes the silent-stall hole a withheld tick leaves — a feed that stops emitting produces no breach on any value comparator, so the sample stream is probed on the rule's own cadence and age advances without the feed; a feed that never delivered is `TileState.Empty`, never a staleness alert, since an alert about a series that never existed names a breach of nothing. `Probe` is rule policy bounded below by the watched feed's own cadence — a probe finer than the feed alerts on jitter the feed cannot answer — and that floor is stated here because the rule addresses its tile by key and cannot reach the feed row to derive it. The one time authority in this fold is the injected scheduler: hold, quiet, and age all read `scheduler.Now`, so a proof lane's virtual scheduler drives every window deterministically — a kernel `MonotonicTimeline` beside it would seat a second clock inside a spine whose operators (`Throttle`, `Interval`) already run on the first, and the two would disagree under virtual time by construction. `FeedHealth` grades through `Severity` rows, so a degraded feed reads the same on a tile, a banner, and the connection strip; this page consumes the freshness projection and derives none of it.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WatchBound(double Floor, double Ceiling);

public readonly record struct WatchSample(double Value, Duration Age);

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WatchComparator {
    public static readonly WatchComparator Above = new("above", static (sample, bound) => sample.Value > bound.Ceiling);
    public static readonly WatchComparator Below = new("below", static (sample, bound) => sample.Value < bound.Floor);
    public static readonly WatchComparator Outside = new("outside", static (sample, bound) => sample.Value < bound.Floor || sample.Value > bound.Ceiling);
    public static readonly WatchComparator Stale = new("stale", static (sample, bound) => sample.Age.TotalSeconds > bound.Ceiling);

    [UseDelegateFromConstructor]
    public partial bool Breached(WatchSample sample, WatchBound bound);
}

public sealed record WatchRule(
    string Key,
    string TileKey,
    WatchComparator Comparator,
    WatchBound Bound,
    Severity Severity,
    Duration PendingFor,
    Duration Quiet,
    Duration Probe,
    string ToastIntent);

public readonly record struct WatchCrossing(string RuleKey, string TileKey, Severity Severity, double Value, Duration Age, string ToastIntent);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class WatchFold {
    public static IObservable<WatchSample> Samples(IObservable<double> stat, Duration probe, IScheduler scheduler) =>
        Observable.Merge(
                stat.Select(static value => Some(value)),
                Observable.Interval(probe.ToTimeSpan(), scheduler).Select(static _ => Option<double>.None))
            .Scan(
                (Value: 0d, Since: scheduler.Now, Seen: false),
                (held, step) => step.Match(
                    Some: value => (Value: value, Since: scheduler.Now, Seen: true),
                    None: () => held))
            .Where(static held => held.Seen)
            .Select(held => new WatchSample(held.Value, Duration.FromTimeSpan(scheduler.Now - held.Since)));

    public static IDisposable Arm(
        WatchRule rule, IObservable<double> stat, IScheduler scheduler, Action<WatchCrossing> raise, Action<Error> fault) =>
        Samples(stat, rule.Probe, scheduler)
            .Select(sample => (Sample: sample, Breached: rule.Comparator.Breached(sample, rule.Bound)))
            .DistinctUntilChanged(static step => step.Breached)
            .Throttle(rule.PendingFor.ToTimeSpan(), scheduler)
            .Where(static step => step.Breached)
            .Scan(
                (Last: Option<DateTimeOffset>.None, Emit: Option<WatchCrossing>.None),
                (gate, step) => scheduler.Now switch {
                    var now when gate.Last.Exists(last => now - last < rule.Quiet.ToTimeSpan()) => (gate.Last, Option<WatchCrossing>.None),
                    var now => (Some(now), Some(new WatchCrossing(rule.Key, rule.TileKey, rule.Severity, step.Sample.Value, step.Sample.Age, rule.ToastIntent))),
                })
            .Choose(static gate => gate.Emit)
            .Subscribe(raise, raw => fault(Error.New(raw.Message, raw)));

    public static Option<WatchCrossing> Worst(Seq<WatchCrossing> live) =>
        live.IsEmpty ? None : Some(live.Fold(live.Head, static (worst, row) => row.Severity.Rank > worst.Severity.Rank ? row : worst));
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FeedHealth {
    public static readonly FeedHealth Live = new("live", Severity.Nominal);
    public static readonly FeedHealth Degraded = new("degraded", Severity.Info);
    public static readonly FeedHealth Reconnecting = new("reconnecting", Severity.Warning);
    public static readonly FeedHealth Stalled = new("stalled", Severity.Critical);

    public Severity Severity { get; }
}

public readonly record struct FeedFreshness(string StreamKey, FeedHealth Health, Option<Instant> LastRefresh, Duration Age);
```

## [06]-[RESEARCH]

(none)
