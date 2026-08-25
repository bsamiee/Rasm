# [APPUI_CHARTS_BOARDS]

The board-scoped context and its survivals: one typed variable-and-range value every tile feed and title reads, one placement fold every layout derives from, one generation-sealed snapshot crossing the composition-seated wire, one deep-link query codec riding the router's grammar, and the board meter contribution. Tiles live at `Charts/tiles.md`, the brushed state at `Charts/boards.md`. Linked brushing is board-lifetime state, so the filter channel and its cross-filter index seat here beside the context every tile reads.

## [01]-[INDEX]

- [02]-[BOARD_CONTEXT]: Variables, the time range, the refresh tick, and the deep-link codec.
- [03]-[PLACEMENT_FOLD]: The per-tier grid and the one wrapping fold every layout derives from.
- [04]-[BOARD_STATE]: The sealed snapshot round-trip and the residue a refused parcel holds.
- [05]-[BOARD_TELEMETRY]: The board meter rows and their composition-bound projections.
- [06]-[FILTER_STATE]: The brushed state, the delta union, the lens value, and the polygon brush.
- [07]-[CROSS_FILTER]: The one push, the two projections, the bitmap index, and the pixel-to-data map.

## [02]-[BOARD_CONTEXT]

- Owner: `BoardVariable` — the typed bounded-vocabulary variable row with its `VariableArity` cardinality; `BoardRange` — the absolute-or-relative window union carrying its wire roster; `TimeRange` — that window under a shift; `BoardContext` — the board-scoped value; `BoardLink` — the deep-link query codec.
- Cases: `BoardRange` = Absolute | Relative; `VariableArity` = single | multi.
- Entry: `BoardContext.Admit(candidate)` — accumulating admission over key, refresh, variable domains, and window order; `TimeRange.Resolve(now)` — the one window resolution; `BoardContext.Window(now, tile)` — the board range under a tile's declared shift; `BoardContext.Ticks(scheduler)` — the board's re-query tick on the scheduler's own time; `BoardLink.Encode(context)` / `Decode(query, declared)` — the deep-link query half.
- Auto: one range change re-derives every tile's window, because a tile's feed reads the context rather than a window of its own — a per-tile override is a SHIFT column, never a second range owner; a variable's domain is its dropdown, so presentation and admission are one declaration and a board templates across projects by re-seeding domains.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, System.Reactive, BCL inbox
- Growth: a new board variable is one `BoardVariable` row; a new range posture is one `BoardRange` arm with its wire-roster row; zero new surface.
- Boundary: a variable is a BOUNDED vocabulary by construction — the row carries its domain and admission refuses a value outside it, so a free-text variable interpolated into a feed key is unspellable and a deep link cannot smuggle one in; cardinality is the `VariableArity` row, so a single-select holding two values refuses at the same gate that proves domain membership. `Relative` resolves against the injected time at read, so a board left open overnight re-resolves rather than pinning the window it opened with; `Absolute` pins deliberately; the two are union arms because a range with a back-duration AND two instants is a state no reader could rank. Refresh cadence is board POLICY, not a feed column — a board refreshing every minute over a feed sampling every quarter-second are two independent facts — and the tick's ONE clock is the surface scheduler, so a proof lane's virtual time advances a board's refresh deterministically and the wall clock enters nowhere on this page. The deep link is the QUERY half alone — scheme, verb, and route key stay the navigation owner's grammar — and a decoded variable outside its declared domain refuses the whole link, because a link that quietly renders a different board than it names is worse than one that refuses; a link carrying both an absolute pair and a back-duration is ambiguous by construction and refuses.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VariableArity {
    public static readonly VariableArity Single = new("single", static count => count <= 1);
    public static readonly VariableArity Multi = new("multi", static _ => true);

    [UseDelegateFromConstructor]
    public partial bool Admits(int count);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BoardVariable(string Key, string Label, Seq<string> Domain, Set<string> Current, VariableArity Arity) {
    public static Fin<BoardVariable> Admit(BoardVariable candidate) =>
        (Gate(!string.IsNullOrWhiteSpace(candidate.Key), $"variable/{candidate.Key}: blank key"),
         Gate(candidate.Domain.Count > 0 && candidate.Domain.ForAll(static value => !string.IsNullOrWhiteSpace(value)),
             $"variable/{candidate.Key}: degenerate domain"),
         Gate(candidate.Current.ForAll(candidate.Domain.Contains), $"variable/{candidate.Key}: value outside domain"),
         Gate(candidate.Arity.Admits(candidate.Current.Count), $"variable/{candidate.Key}: arity breach"))
            .Apply((_, _, _, _) => candidate).As().ToFin();

    public Fin<BoardVariable> With(Set<string> chosen) => Admit(this with { Current = chosen });

    static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? unit : (Validation<Error, Unit>)(Error)new ChartFault.ContextRejected(detail);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Absolute), "absolute")]
[JsonDerivedType(typeof(Relative), "relative")]
public abstract partial record BoardRange {
    private BoardRange() { }
    public sealed record Absolute(Instant From, Instant To) : BoardRange;
    public sealed record Relative(Duration Back) : BoardRange;
}

public sealed record TimeRange(BoardRange Window, Duration Shift) {
    public static readonly TimeRange LastHour = new(new BoardRange.Relative(Duration.FromHours(1)), Duration.Zero);

    public static Fin<TimeRange> Admit(TimeRange candidate) =>
        candidate.Window.Switch(
            state: candidate,
            absolute: static (row, window) => window.From < window.To
                ? Fin.Succ(row)
                : Fin.Fail<TimeRange>(new ChartFault.ContextRejected("range/inverted")),
            relative: static (row, window) => window.Back > Duration.Zero
                ? Fin.Succ(row)
                : Fin.Fail<TimeRange>(new ChartFault.ContextRejected("range/non-positive")));

    public (Instant From, Instant To) Resolve(Instant now) => Window.Switch(
        state: (Now: now, Shift: Shift),
        absolute: static (s, window) => (window.From - s.Shift, window.To - s.Shift),
        relative: static (s, window) => (s.Now - window.Back - s.Shift, s.Now - s.Shift));

    public TimeRange Shifted(Duration by) => this with { Shift = Shift + by };
}

public sealed record BoardContext(string Key, Seq<BoardVariable> Variables, TimeRange Range, Duration Refresh) {
    public static Fin<BoardContext> Admit(BoardContext candidate) =>
        (Gate(!string.IsNullOrWhiteSpace(candidate.Key), $"{candidate.Key}: blank key"),
         Gate(candidate.Refresh > Duration.Zero, $"{candidate.Key}: non-positive refresh"),
         Gate(candidate.Variables.Map(static row => row.Key).Distinct().Count == candidate.Variables.Count,
             $"{candidate.Key}: duplicate variable keys"),
         candidate.Variables.Traverse(row => BoardVariable.Admit(row).ToValidation()).Map(static _ => unit).As(),
         TimeRange.Admit(candidate.Range).ToValidation().Map(static _ => unit).As())
            .Apply((_, _, _, _, _) => candidate).As().ToFin();

    public Option<Set<string>> Value(string key) => Variables.Find(row => row.Key == key).Map(static row => row.Current);

    public (Instant From, Instant To) Window(Instant now, Option<TimeRange> tile) =>
        tile.IfNone(Range).Resolve(now);

    public IObservable<Instant> Ticks(SurfaceScheduler scheduler) =>
        Observable.Interval(Refresh.ToTimeSpan(), scheduler.Ui)
            .Select(_ => Instant.FromDateTimeOffset(scheduler.Ui.Now));

    static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? unit : (Validation<Error, Unit>)(Error)new ChartFault.ContextRejected(detail);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BoardLink {
    const string FromKey = "from";
    const string ToKey = "to";
    const string BackKey = "back";
    const string ShiftKey = "shift";
    const string RefreshKey = "refresh";
    const string VariablePrefix = "var.";

    public static string Encode(BoardContext context) =>
        string.Join('&', Pairs(context).Map(static pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));

    public static Fin<BoardContext> Decode(string query, BoardContext declared) =>
        Parsed(query) switch {
            var fields => Window(fields, declared.Range)
                .Bind(range => declared.Variables
                    .Traverse(row => fields.Find($"{VariablePrefix}{row.Key}")
                        .Match(
                            Some: raw => row.With(toSet(raw.Split(',', StringSplitOptions.RemoveEmptyEntries))),
                            None: () => Fin.Succ(row)))
                    .As()
                    .Bind(variables => Refresh(fields, declared.Refresh)
                        .Bind(refresh => BoardContext.Admit(declared with { Variables = variables, Range = range, Refresh = refresh })))),
        };

    static Seq<(string Key, string Value)> Pairs(BoardContext context) =>
        context.Range.Window.Switch(
            state: context,
            absolute: static (row, window) => Seq(
                (FromKey, window.From.ToString()),
                (ToKey, window.To.ToString())),
            relative: static (row, window) => Seq((BackKey, window.Back.ToString())))
        + Seq((ShiftKey, context.Range.Shift.ToString()), (RefreshKey, context.Refresh.ToString()))
        + context.Variables.Filter(static row => !row.Current.IsEmpty)
            .Map(static row => ($"{VariablePrefix}{row.Key}", string.Join(',', row.Current)));

    static HashMap<string, string> Parsed(string query) =>
        toHashMap(query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Choose(field => field.Split('=', 2) switch {
                [var key, var value] => Some((Uri.UnescapeDataString(key), Uri.UnescapeDataString(value))),
                _ => None,
            }));

    static Fin<TimeRange> Window(HashMap<string, string> fields, TimeRange declared) =>
        ((fields.Find(FromKey), fields.Find(ToKey), fields.Find(BackKey)) switch {
            ({ IsSome: true }, _, { IsSome: true }) or (_, { IsSome: true }, { IsSome: true }) =>
                Fin.Fail<TimeRange>(new ChartFault.ContextRejected("link/ambiguous-range")),
            ({ IsSome: true, Case: string from }, { IsSome: true, Case: string to }, _) =>
                (InstantPattern.ExtendedIso.Parse(from), InstantPattern.ExtendedIso.Parse(to)) switch {
                    ({ Success: true } lo, { Success: true } hi) =>
                        TimeRange.Admit(declared with { Window = new BoardRange.Absolute(lo.Value, hi.Value) }),
                    _ => Fin.Fail<TimeRange>(new ChartFault.ContextRejected("link/instant")),
                },
            (_, _, { IsSome: true, Case: string back }) => DurationPattern.Roundtrip.Parse(back) switch {
                { Success: true } parsed => TimeRange.Admit(declared with { Window = new BoardRange.Relative(parsed.Value) }),
                _ => Fin.Fail<TimeRange>(new ChartFault.ContextRejected("link/duration")),
            },
            _ => Fin.Succ(declared),
        })
        .Bind(range => fields.Find(ShiftKey).Match(
            Some: shift => DurationPattern.Roundtrip.Parse(shift) switch {
                { Success: true } parsed => TimeRange.Admit(range with { Shift = parsed.Value }),
                _ => Fin.Fail<TimeRange>(new ChartFault.ContextRejected("link/shift")),
            },
            None: () => Fin.Succ(range)));

    static Fin<Duration> Refresh(HashMap<string, string> fields, Duration declared) =>
        fields.Find(RefreshKey).Match(
            Some: raw => DurationPattern.Roundtrip.Parse(raw) switch {
                { Success: true } parsed when parsed.Value > Duration.Zero => Fin.Succ(parsed.Value),
                _ => Fin.Fail<Duration>(new ChartFault.ContextRejected("link/refresh")),
            },
            None: () => Fin.Succ(declared));
}
```

## [03]-[PLACEMENT_FOLD]

- Owner: `PlacementGrid` — the column count per breakpoint tier, mintable only through its frozen roster; `SpanPolicy` — equal-weight against fixed-span wrapping; `PlacementFlow` — the ONE placement fold every board layout derives from.
- Cases: `SpanPolicy` = Equal | Fixed(span).
- Entry: `PlacementGrid.For(at)` — the tier's grid row; `PlacementFlow.Flow(grid, keys, span, rowSpan, from)` — the one fold; `PlacementFlow.Layout(key, bands, canvasState)` — the whole-board derivation across every tier.
- Auto: every board layout derives its columns and spans from the grid row for the active breakpoint, so a literal column index, a literal span, and a literal wrap arithmetic are all unspellable; a FACET grid is the same fold at a tile-local grid whose column count is the facet's own declared width — a grid is a grid at every scale.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new responsive tier is one roster row over the settled breakpoint vocabulary; a new spanning posture is one `SpanPolicy` arm; zero new surface.
- Boundary: placement is BREAKPOINT-INDEXED over the settled `BreakpointRow` vocabulary rather than a second responsive axis, and the widest declared tier at or below the active one wins, so a board declaring one tier renders at every width; the grid's column count is positive BY CONSTRUCTION — the ctor is private and the frozen roster is the only mint — so the `int.Max` floor guards the retired public record needed are gone; `Equal` derives its span from the key count and falls through to single-column wrapping where the tier cannot hold one column per key, so two tiles are halves and four are quarters at whatever width the tier declares, and the retired `Band`/`Flow` twins — one fold reachable from the other at a derived span — are one fold under one policy row.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlacementGrid {
    private PlacementGrid(BreakpointRow at, int columns) { At = at; Columns = columns; }

    public BreakpointRow At { get; }
    public int Columns { get; }

    public static readonly Seq<PlacementGrid> Rows = Seq(
        new PlacementGrid(BreakpointRow.Compact, 4),
        new PlacementGrid(BreakpointRow.Medium, 8),
        new PlacementGrid(BreakpointRow.Expanded, 12),
        new PlacementGrid(BreakpointRow.Ultrawide, 12));

    public static PlacementGrid For(BreakpointRow at) => Rows.Find(row => row.At == at).IfNone(Rows[0]);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpanPolicy {
    private SpanPolicy() { }
    public sealed record Equal() : SpanPolicy;
    public sealed record Fixed(int Span) : SpanPolicy;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlacementFlow {
    public static (Seq<TilePlacement> Placements, int Next) Flow(
        PlacementGrid grid, Seq<string> keys, SpanPolicy policy, int rowSpan, int from) {
        if (keys.IsEmpty) { return (Seq<TilePlacement>(), from); }
        int span = policy switch {
            SpanPolicy.Fixed fixedSpan => int.Clamp(fixedSpan.Span, 1, grid.Columns),
            _ => int.Max(1, grid.Columns / keys.Count),
        };
        int perRow = grid.Columns / span;
        return (
            keys.Map((key, index) => new TilePlacement(
                key, grid.At, index % perRow * span, from + (index / perRow * rowSpan), span, rowSpan)),
            from + (((keys.Count + perRow - 1) / perRow) * rowSpan));
    }

    public static Fin<DashboardLayout> Layout(string key, Seq<(Seq<string> Keys, int RowSpan)> bands, Option<string> canvasState = default) =>
        DashboardLayout.Admit(key,
            toSeq(PlacementGrid.Rows).Bind(grid =>
                bands.Fold((Acc: Seq<TilePlacement>(), Row: 0), (state, band) =>
                    Flow(grid, band.Keys, new SpanPolicy.Equal(), band.RowSpan, state.Row) switch {
                        var laid => (Acc: state.Acc + laid.Placements, Row: laid.Next),
                    }).Acc),
            canvasState);
}
```

## [04]-[BOARD_STATE]

- Owner: `BoardState` — the whole restorable board as ONE sealed grain: its layout, its brushed state, and the context every tile reads.
- Entry: `BoardState.Seal` — the grain's one `Diagnostics/evidence#DURABLE_PARCEL` `StateSeal` row; `Capture(layout, filter, context)` — the admitted snapshot and the seal's own admission arrow; `Save()` / `Open(blob)` — the round-trip through that seal; `Reapply(crossFilter)` — the brush restore pushed as a delta.
- Auto: the arrangement a person built is what this grain holds, so the seal declares `StateResidue.Hold` and a parcel the generation refuses reaches the shell as raw bytes beside the seeded default — the board opens, the loss is visible, and the arrangement stays recoverable by hand.
- Packages: LanguageExt.Core
- Growth: a new restorable board column is one `BoardState` field under one `Generation` bump on the seal row; zero new surface.
- Boundary: a board's stored shape carries no forward ladder, no per-generation step, and no version ordinal in its key — the seal proves the generation inside the bytes and the board rebuilds from its declared default where they disagree, because a step table translating one authored arrangement into another is a second authority on what the arrangement meant and a build reading its output renders a board nobody arranged while every gate passes. NAMED LOSS: attribute-rename carry — a placement or context column renamed across a generation reaches no reader under the next, and the residue is where that arrangement survives. The snapshot rides `EvidenceOps.Wire` through the seal, the ONE composition-seated options every AppUi durable payload crosses: it carries the NodaTime registration and the `Option`/`Seq`/`Set`/`HashMap` converters this record cannot self-describe, while the generated owners round-trip on their own stamped converters — the dock serializer's package-internal options can carry none of those, which is why handing this record that rail was a silent default round-trip no decode refused. `Capture` IS the admission the seal runs, so a parcel that decoded into the shape still proves overlap, window order, and variable domains before a tile reads it. Restore is a DELTA like every other change, pushed under the board key, so a side door writing the brush subject directly cannot seat a state no brush could produce.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record BoardState(DashboardLayout Layout, FilterState Filter, BoardContext Context) {
    public static readonly StateSeal Seal = StateSeal.Of("chart", "board", generation: 2, StateResidue.Hold);

    public static Fin<BoardState> Capture(DashboardLayout layout, FilterState filter, BoardContext context) =>
        DashboardLayout.Admit(layout.Key, layout.Placements, layout.CanvasState)
            .Bind(admitted => FilterState.Admit(filter)
                .Bind(admittedFilter => BoardContext.Admit(context)
                    .Map(admittedContext => new BoardState(admitted, admittedFilter, admittedContext))));

    public Fin<string> Save() => Seal.Write(this);

    public static Restored<BoardState> Open(string blob) =>
        Seal.Read<BoardState>(blob, static state => Capture(state.Layout, state.Filter, state.Context));

    public IO<Fin<Unit>> Reapply(CrossFilter crossFilter) =>
        crossFilter.Push(Layout.Key, new FilterDelta.Snapshot(Filter));
}
```

## [05]-[BOARD_TELEMETRY]

- Owner: `BoardTelemetry` — the board meter rows and the composition-bound projections each producing fold reads.
- Entry: `TelemetryRow(version)` — the one contribution; four `Observe` overloads — the render receipt, the geo-overlay swap, the brush push, and the watch crossing, each bound at composition onto the fold that already holds the typed fact.
- Packages: LanguageExt.Core, NodaTime
- Growth: one instrument is one row and one `Observe` arm; zero new surface.
- Boundary: instrument declarations are `InstrumentSpec` ROWS and every write passes the row, so a write against an undeclared name has no spelling; tags cross as the kernel's stack-allocated `InstrumentSet.Tags` projection, so a brush push measured on every drag allocates no per-write array; the severity dimension is the row key on the crossing count, so warn volume and critical volume separate on one series; the named-board roster is `Charts/telemetry.md`'s tile registry and the render-hash proof lane's `RenderReceipt` seals through the message envelope — this owner contributes the meter rows and projects, and measures nothing itself.

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
public static class BoardTelemetry {
    public static readonly InstrumentSpec Render = InstrumentSpec.Create(
        "rasm.appui.chart.render.elapsed", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "board and chart render wall duration", Seq<string>(), Some(Buckets.InteractionSeconds), None, None);
    public static readonly InstrumentSpec FrameSize = InstrumentSpec.Create(
        "rasm.appui.chart.frame.size", InstrumentKind.Count, MeasureForm.Whole, "By",
        "encoded board-frame payload size", Seq<string>(), None, None, None);
    public static readonly InstrumentSpec OverlaySwaps = InstrumentSpec.Create(
        "rasm.appui.geo.overlay.swaps", InstrumentKind.Count, MeasureForm.Whole, "{swap}",
        "live geo-overlay land swaps", Seq<string>(), None, None, None);
    public static readonly InstrumentSpec OverlayLands = InstrumentSpec.Create(
        "rasm.appui.geo.overlay.lands", InstrumentKind.Count, MeasureForm.Whole, "{land}",
        "land records folded per overlay swap", Seq<string>(), None, None, None);
    public static readonly InstrumentSpec FilterApplies = InstrumentSpec.Create(
        "rasm.appui.filter.applies", InstrumentKind.Count, MeasureForm.Whole, "{brush}",
        "cross-filter brush applications by source tile", Seq(AppUiTelemetry.SourceSlot), None, None, None);
    public static readonly InstrumentSpec FilterTiles = InstrumentSpec.Create(
        "rasm.appui.filter.tiles", InstrumentKind.Count, MeasureForm.Whole, "{tile}",
        "tiles re-filtered per brush application", Seq<string>(), None, None, None);
    public static readonly InstrumentSpec WatchCrossings = InstrumentSpec.Create(
        "rasm.appui.watch.crossings", InstrumentKind.Count, MeasureForm.Whole, "{crossing}",
        "watch-rule crossings raised by severity", Seq(AppUiTelemetry.SeveritySlot), None, None, None);

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Render, FrameSize, OverlaySwaps, OverlayLands, FilterApplies, FilterTiles, WatchCrossings);

    public static Fin<Unit> Observe(InstrumentSet set, RenderReceipt receipt) =>
        set.Write(Render, receipt.Elapsed.TotalSeconds)
            .Bind(_ => set.Write(FrameSize, receipt.Bytes));

    public static Fin<Unit> Observe(InstrumentSet set, int landsFolded) =>
        set.Write(OverlaySwaps, 1d)
            .Bind(_ => set.Write(OverlayLands, landsFolded));

    public static Fin<Unit> Observe(InstrumentSet set, FilterState pushed, int tilesRefiltered) =>
        set.Write(FilterApplies, 1d,
                InstrumentSet.Tags((AppUiTelemetry.SourceSlot, pushed.Source.IfNone("none"))))
            .Bind(_ => set.Write(FilterTiles, tilesRefiltered));

    public static Fin<Unit> Observe(InstrumentSet set, WatchCrossing crossing) =>
        set.Write(WatchCrossings, 1d,
            InstrumentSet.Tags((AppUiTelemetry.SeveritySlot, crossing.Severity.Key)));
}
```

## [06]-[FILTER_STATE]

- Owner: `FilterState` — the whole brushed state of a board; `FilterDelta` — the ONE mutation vocabulary whose arms carry their own state fold; `BrushLens<TRow>` — one tile's projector set as a value, carrying its optional bitmap index; `PolygonBrush` — the lasso ring over the admitted geometry engine's indexed locator.
- Cases: `FilterDelta` = Time | Tags | Dimension | Region | Highlight | Snapshot | Cleared.
- Entry: `FilterState.Admit(candidate)` — accumulating admission over window order, blank keys, ring arity, and finite ring points; `FilterDelta.Apply(held, source)` — the one place a `FilterState` column is written; `PolygonBrush.Contains(x, y)` — the indexed point-in-area read.
- Auto: the source stamp rides the delta fold, so no arm can forget it and the self-exclusion the predicate depends on is structural; a highlight does NOT stamp the source, because the surface a pointer is over must stay lit while a filter must exclude the tile that raised it; `Snapshot` and `Cleared` make restore and reset deltas like every other change rather than two side doors.
- Packages: LanguageExt.Core, NodaTime, NetTopologySuite, Thinktecture.Runtime.Extensions
- Growth: a new brushed concern is one `FilterDelta` arm carrying its own fold and one `FilterState` column; zero new surface.
- Boundary: filter and highlight are ONE question at two intensities — remove the non-matching, or dim it — so `Highlight` sits beside the filter columns and a second subject would let a hovered category and a brushed category disagree about which rows they mean; equality over the state is the collections' own — `Set`, `HashMap`, and `Seq` are value-semantic carriers whose `Equals` is structural, so a distinct-until-changed consumer reads real state identity with no generated comparer; the lens is one VALUE per tile rather than four optional delegate parameters threaded through every call, so a tile's brushable axes are stated once and every reader sees the same set — `Key` is the identity the highlight set names, so a highlight published by a table row and consumed by a chart mark resolve one vocabulary; containment rides the ADMITTED geometry engine's indexed locator built once per brush — a lasso over ten thousand scatter points costs one interval-tree query per point, boundary points classify by the engine's own `Location` vocabulary (a lasso drawn through a point selected it), and a page-local even-odd ray cast beside an admitted robust locator is the deleted form; the ring closes at this owner because the geometry factory refuses an unclosed linear ring, so no caller repeats the first coordinate.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record FilterState(
    Option<Instant> From,
    Option<Instant> To,
    Set<string> Tags,
    HashMap<string, Set<string>> Dimensions,
    Option<PolygonBrush> Region,
    Set<string> Highlight,
    Option<string> Source) {
    public static readonly FilterState Empty =
        new(None, None, Set<string>(), HashMap<string, Set<string>>(), None, Set<string>(), None);

    public static Fin<FilterState> Admit(FilterState candidate) =>
        (Gate(candidate.From.Match(Some: lo => candidate.To.ForAll(hi => lo <= hi), None: static () => true), "window inverted"),
         Gate(candidate.Tags.ForAll(static tag => !string.IsNullOrWhiteSpace(tag)), "blank tag"),
         Gate(candidate.Highlight.ForAll(static key => !string.IsNullOrWhiteSpace(key)), "blank highlight key"),
         Gate(candidate.Region.ForAll(static region => !string.IsNullOrWhiteSpace(region.DimensionKey)
                 && region.Ring.Count >= 3
                 && region.Ring.ForAll(static point => double.IsFinite(point.X) && double.IsFinite(point.Y))), "degenerate region"),
         Gate(candidate.Dimensions.ForAll(static entry => !string.IsNullOrWhiteSpace(entry.Key)
                 && entry.Value.ForAll(static value => !string.IsNullOrWhiteSpace(value))), "blank dimension member"),
         Gate(candidate.Source.ForAll(static source => !string.IsNullOrWhiteSpace(source)), "blank source"))
            .Apply((_, _, _, _, _, _) => candidate).As().ToFin();

    public bool Admits(Instant at, Set<string> rowTags) =>
        From.Map(lo => at >= lo).IfNone(true)
            && To.Map(hi => at <= hi).IfNone(true)
            && (Tags.IsEmpty || Tags.Exists(rowTags.Contains));

    static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? unit : (Validation<Error, Unit>)(Error)new ChartFault.BrushRejected(detail);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FilterDelta {
    private FilterDelta() { }

    public sealed record Time(Option<Instant> From, Option<Instant> To) : FilterDelta;
    public sealed record Tags(Set<string> Values) : FilterDelta;
    public sealed record Dimension(string Key, Set<string> Values) : FilterDelta;
    public sealed record Region(Option<PolygonBrush> Brush) : FilterDelta;
    public sealed record Highlight(Set<string> Keys) : FilterDelta;
    public sealed record Snapshot(FilterState State) : FilterDelta;
    public sealed record Cleared() : FilterDelta;

    public FilterState Apply(FilterState held, string source) => Switch(
        state: (Held: held, Source: source),
        time: static (s, row) => s.Held with { From = row.From, To = row.To, Source = Some(s.Source) },
        tags: static (s, row) => s.Held with { Tags = row.Values, Source = Some(s.Source) },
        dimension: static (s, row) => s.Held with {
            Dimensions = s.Held.Dimensions.AddOrUpdate(row.Key, row.Values),
            Source = Some(s.Source),
        },
        region: static (s, row) => s.Held with { Region = row.Brush, Source = Some(s.Source) },
        highlight: static (s, row) => s.Held with { Highlight = row.Keys },
        snapshot: static (_, row) => row.State,
        cleared: static (_, _) => FilterState.Empty);
}

public sealed record BrushLens<TRow>(
    Func<TRow, string> Key,
    Func<TRow, Instant> At,
    Func<TRow, Set<string>> Tags,
    Option<Func<TRow, string, Option<string>>> Dimension,
    Option<Func<TRow, (double X, double Y)>> Point,
    Option<DimensionIndex<TRow, string>> Index = default);

public sealed record PolygonBrush(string DimensionKey, Seq<(double X, double Y)> Ring) {
    private readonly Lazy<Option<IPointOnGeometryLocator>> locator =
        new(() => Locate(Ring), LazyThreadSafetyMode.ExecutionAndPublication);

    public bool Contains(double x, double y) =>
        locator.Value.Match(
            Some: found => found.Locate(new Coordinate(x, y)) is Location.Interior or Location.Boundary,
            None: static () => false);

    static Option<IPointOnGeometryLocator> Locate(Seq<(double X, double Y)> ring) =>
        ring.Count < 3
            ? None
            : Optional<IPointOnGeometryLocator>(new IndexedPointInAreaLocator(
                GeometryFactory.Default.CreatePolygon(GeometryFactory.Default.CreateLinearRing(
                    ring.Append(ring.Head).Map(static point => new Coordinate(point.X, point.Y)).ToArray()))));
}
```

## [07]-[CROSS_FILTER]

- Owner: `CrossFilter` — the one push and the two projections over one lens; `DimensionIndex<TRow, TKey>` — the word-aligned categorical bitmap the lens carries; `ChartBrush` — the pixel-to-data mapping the gesture edge binds.
- Entry: `Push(source, FilterDelta)` — the ONE brush mutation, answering the admission's typed verdict; `Predicate<TRow>(tile, lens)` — the dynamic predicate the DynamicData `Filter(IObservable<Func<TRow,bool>>)` overload takes; `Emphasis<TRow>(tile, lens)` — the per-row opacity off the identical state; `DimensionIndex.Ingest(row)` / `Drop(key)` / `Selected(predicate)` — the bitmap maintenance and the AND-of-unions read; `ChartBrush.From(chart, from, to, scalesXAt, scalesYAt, axis, dimension)` — the rectangle-drag decode at the chart's own scale.
- Auto: the predicate composes inside the chart `SyncContext` lock on the one `Connect()` spine the multi-series feeds already share, so a brush is an incremental change-set re-filter, never a feed re-subscribe; each brush push and its re-filtered tile count fold onto the one meter through `BoardTelemetry.Observe` (`Charts/boards.md`); the server-side filtered re-query against the analytical lane is Persistence-owned — the brush pushes the same `(time, tags, dimensions, region)` shape across the seam and AppUi never builds the SQL predicate.
- Packages: LanguageExt.Core, System.Reactive, DynamicData, CommunityToolkit.HighPerformance, NetTopologySuite, NodaTime
- Growth: a new cross-tile brush dimension is one `FilterState.Dimensions` map key; a tile earning the bitmap accelerator is one `Index` column on its lens; zero new surface.
- Boundary: consumption is ONE projection per question — `Predicate` removes and `Emphasis` dims off the identical state, so a scene ghosting to a hovered category, a chart dimming non-matching marks, and a table bolding its hovered row are three readers of one channel; the source tile is excluded from its own brush by the `FilterState.Source` key so a self-filter loop is structurally impossible, and it is NOT excluded from its own highlight. `Push` answers the ADMISSION's verdict on the rail — a rejected delta names its defect and writes nothing, so the subject's value is admitted state by construction; the Rx `BehaviorSubject` stays the carrier because the board's whole spine is the declared Rx/DynamicData fabric and its consumers subscribe the state as an observable — the discriminant over a kernel `Atom` is that the CELL here IS the subscription surface, not a guarded transition whose verdict a contender re-reads. The categorical fold is RESOLVED ONCE per state change: a lens carrying its `DimensionIndex` folds the brushed dimension members into one survivor set through the bitmap AND — `O(words)` per push — and the per-row read is one frozen-set membership test, so no brush path performs an `O(rows)` re-scan; a lens without the index takes the per-row linear predicate, which is the honest cost for a tile too small to earn a bitmap. The index's package census is settled and the verdict is composition: `BitHelper` carries every per-bit read and write at both machine widths but owns no bitmap, no set algebra, and no iteration; `BitArray` allocates per element and cannot walk set bits; nothing else admitted reaches further — so the ordinal registry, the per-cell bitmaps, and the word loops mutate in place as the NAMED statement exemption, narrowed to exactly those members plus the two growth allocations (`Union`'s live-set clone on the empty-value arm and `Grow`'s resize), while every bit operation rides the admitted helper and the survivor walk rides the in-box `BitOperations.TrailingZeroCount`. An EMPTY brushed value set constrains nothing and unions the live set — the same sense the predicate fold gives it, so bitmap-indexed and predicate-filtered tiles answer one brush identically. PIXEL-TO-DATA mapping reads the chart's own `ScalePixelsToData` at the layer's declared axis indices for both corners — a board-local pixel-per-unit reconstruction would silently disagree after any pan — and the gesture edge that raises it is the chart pointer bind at the composing surface, which hands the decoded delta to `Push` like every other arm; corners order after conversion because an inverted axis maps a top-left drag onto a bottom-right domain rectangle. `CrossFilter.Dispose` completes and disposes the subject at the board activation boundary. The brushed-dimension VALUE vocabulary is the Element seam's: a brush a query lane must replay crosses as the seam predicate keyed by `PredicateKey.Key`, so a stored delegate filter — unreplayable, unhashable, receipt-less — has no spelling on this channel.

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
public sealed class CrossFilter : IDisposable {
    private readonly BehaviorSubject<FilterState> state = new(FilterState.Empty);

    public IObservable<FilterState> State => state;

    public FilterState Current => state.Value;

    public IO<Fin<Unit>> Push(string source, FilterDelta delta) => IO.lift(() =>
        FilterState.Admit(delta.Apply(state.Value, source)).Map(admitted => fun(() => state.OnNext(admitted))()));

    public IObservable<Func<TRow, bool>> Predicate<TRow>(string tile, BrushLens<TRow> lens) =>
        state.Select(filter => {
            Option<FrozenSet<string>> survivors = lens.Index
                .Filter(_ => !filter.Dimensions.IsEmpty)
                .Map(index => index.Selected(filter.Dimensions).ToFrozenSet(StringComparer.Ordinal));
            return (Func<TRow, bool>)(row =>
                filter.Source == Some(tile)
                    || (filter.Admits(lens.At(row), lens.Tags(row))
                        && survivors.Match(
                            Some: chosen => chosen.Contains(lens.Key(row)),
                            None: () => DimensionsAdmit(filter, row, lens))
                        && RegionAdmits(filter, row, lens)));
        });

    public IObservable<Func<TRow, double>> Emphasis<TRow>(string tile, BrushLens<TRow> lens) =>
        state.Select(filter => (Func<TRow, double>)(row =>
            filter.Highlight.IsEmpty || filter.Highlight.Contains(lens.Key(row)) ? 1d : Dimmed));

    public const double Dimmed = 0.25d;

    private static bool DimensionsAdmit<TRow>(FilterState filter, TRow row, BrushLens<TRow> lens) =>
        lens.Dimension.Match(
            Some: project => filter.Dimensions.ForAll(entry => project(row, entry.Key).Match(
                Some: value => entry.Value.IsEmpty || entry.Value.Contains(value),
                None: () => entry.Value.IsEmpty)),
            None: static () => true);

    private static bool RegionAdmits<TRow>(FilterState filter, TRow row, BrushLens<TRow> lens) =>
        lens.Point.Match(
            Some: project => filter.Region.Match(
                Some: brush => project(row) switch { var at => brush.Contains(at.X, at.Y) },
                None: static () => true),
            None: static () => true);

    public void Dispose() {
        state.OnCompleted();
        state.Dispose();
    }
}

public sealed class DimensionIndex<TRow, TKey> where TKey : notnull {
    private readonly Func<TRow, TKey> key;
    private readonly FrozenDictionary<string, Func<TRow, string>> dimensions;
    private readonly Dictionary<TKey, int> ordinals = new();
    private readonly List<TKey> keys = [];
    private readonly Dictionary<string, Dictionary<string, ulong[]>> words = new(StringComparer.Ordinal);
    private ulong[] live = new ulong[1];
    private int capacityWords = 1;

    public DimensionIndex(Func<TRow, TKey> key, FrozenDictionary<string, Func<TRow, string>> dimensions) {
        this.key = key;
        this.dimensions = dimensions;
        foreach (string dimension in dimensions.Keys) { words[dimension] = new Dictionary<string, ulong[]>(StringComparer.Ordinal); }
    }

    public Unit Ingest(TRow row) {
        TKey k = key(row);
        if (!ordinals.TryGetValue(k, out int ordinal)) {
            ordinal = keys.Count;
            ordinals[k] = ordinal;
            keys.Add(k);
            Grow(ordinal);
        }
        Clear(ordinal);
        BitHelper.SetFlag(ref live[ordinal >> 6], ordinal & 63, true);
        foreach ((string dimension, Func<TRow, string> project) in dimensions) {
            Dictionary<string, ulong[]> bucket = words[dimension];
            string value = project(row);
            if (!bucket.TryGetValue(value, out ulong[]? bitmap)) { bitmap = new ulong[capacityWords]; bucket[value] = bitmap; }
            BitHelper.SetFlag(ref bitmap[ordinal >> 6], ordinal & 63, true);
        }
        return unit;
    }

    public Unit Drop(TKey k) {
        if (!ordinals.TryGetValue(k, out int ordinal)) { return unit; }
        Clear(ordinal);
        BitHelper.SetFlag(ref live[ordinal >> 6], ordinal & 63, false);
        return unit;
    }

    public Seq<TKey> Selected(HashMap<string, Set<string>> predicate) =>
        predicate.IsEmpty
            ? Materialize(Some(live))
            : Materialize(predicate.Fold(
                Option<ulong[]>.None,
                (acc, entry) => acc.Match(
                    Some: held => Some(And(held, Union(entry.Key, entry.Value))),
                    None: () => Some(Union(entry.Key, entry.Value)))));

    private ulong[] Union(string dimension, Set<string> values) {
        if (values.IsEmpty) { return (ulong[])live.Clone(); }
        ulong[] result = new ulong[capacityWords];
        if (!words.TryGetValue(dimension, out Dictionary<string, ulong[]>? bucket)) { return result; }
        foreach (string value in values) {
            if (bucket.TryGetValue(value, out ulong[]? bitmap)) {
                for (int word = 0; word < capacityWords; word++) { result[word] |= bitmap[word]; }
            }
        }
        return result;
    }

    private static ulong[] And(ulong[] left, ulong[] right) {
        ulong[] result = new ulong[left.Length];
        for (int word = 0; word < left.Length; word++) { result[word] = left[word] & right[word]; }
        return result;
    }

    private Seq<TKey> Materialize(Option<ulong[]> bits) =>
        bits.Match(
            Some: chosen => {
                List<TKey> hits = [];
                for (int word = 0; word < chosen.Length; word++) {
                    ulong bucket = chosen[word];
                    while (bucket != 0UL) {
                        hits.Add(keys[(word << 6) + BitOperations.TrailingZeroCount(bucket)]);
                        bucket &= bucket - 1;
                    }
                }
                return toSeq(hits);
            },
            None: () => Seq<TKey>());

    private Unit Clear(int ordinal) {
        foreach (Dictionary<string, ulong[]> bucket in words.Values) {
            foreach (ulong[] bitmap in bucket.Values) { BitHelper.SetFlag(ref bitmap[ordinal >> 6], ordinal & 63, false); }
        }
        return unit;
    }

    private void Grow(int ordinal) {
        int need = (ordinal >> 6) + 1;
        if (need <= capacityWords) { return; }
        capacityWords = need;
        Array.Resize(ref live, capacityWords);
        foreach (Dictionary<string, ulong[]> bucket in words.Values) {
            foreach (string value in bucket.Keys.ToArray()) { Array.Resize(ref CollectionsMarshal.GetValueRefOrNullRef(bucket, value), capacityWords); }
        }
    }
}

public static class ChartBrush {
    public static Fin<FilterDelta> From(
        SourceGenCartesianChart chart, LvcPointD from, LvcPointD to, int scalesXAt, int scalesYAt,
        ChartAxisKind axis, string dimension) =>
        (chart.ScalePixelsToData(from, scalesXAt, scalesYAt), chart.ScalePixelsToData(to, scalesXAt, scalesYAt)) switch {
            var (a, b) when !double.IsFinite(a.X) || !double.IsFinite(b.X) =>
                Fin.Fail<FilterDelta>(new ChartFault.BrushRejected($"{dimension}: drag maps to no finite domain")),
            var (a, b) => (Low: Math.Min(a.X, b.X), High: Math.Max(a.X, b.X)) switch {
                var span when span.High <= span.Low =>
                    Fin.Fail<FilterDelta>(new ChartFault.BrushRejected($"{dimension}: degenerate drag")),
                var span => axis == ChartAxisKind.Instant
                    ? Fin.Succ<FilterDelta>(new FilterDelta.Time(
                        Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(span.Low.AsDate(), DateTimeKind.Utc))),
                        Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(span.High.AsDate(), DateTimeKind.Utc)))))
                    : Fin.Succ<FilterDelta>(new FilterDelta.Dimension(dimension, toSet(
                        Seq(span.Low.ToString(CultureInfo.InvariantCulture), span.High.ToString(CultureInfo.InvariantCulture))))),
            },
        };
}
```

## [08]-[RESEARCH]

(none)
