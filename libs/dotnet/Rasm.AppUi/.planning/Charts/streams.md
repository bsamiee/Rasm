# [APPUI_CHARTS_STREAMS]

The stream-and-reshape plane: `ChartStream` is the typed feed roster with retention, cadence, and shape columns as row data, `TransformRow` declares every reshape between feed and series on one shape-checked chain, `ChartReducer` projects every order statistic off ONE kernel `Distribution` per group, `ChartFolds` carries retention and cadence onto the DynamicData operators plus the largest-triangle downsampler, and `PlanFeeds` folds the Bim `CostSchedule` and `ScheduleNetwork` planning receipts into chart rows — consumed as feed values, never re-solved.

## [01]-[INDEX]

- [02]-[SHAPE_VOCABULARY]: Pipeline shapes; reducers over the kernel spread; bins; the civil calendar.
- [03]-[TRANSFORM_CHAIN]: The declared reshape family, the shape check, and the one evaluator.
- [04]-[FEED_ROSTER]: The typed feed rows and the retention and cadence folds.
- [05]-[PLAN_FEEDS]: Cost and schedule receipts folded into chart rows.

## [02]-[SHAPE_VOCABULARY]

- Owner: `ChartShape` — the declared pipeline-shape vocabulary; `GroupSpread` — the one per-group statistics carrier every reducer row projects; `ChartReducer` — the reducer rows; `BinPolicy` — binning as a closed value; `CalendarAxis` and `CalendarPolicy` — the civil-calendar fold and its injected zone.
- Cases: `ChartShape` = series | grouped | matrix | summary | span; `ChartReducer` = count | sum | mean | weighted | median | quantile | minimum | maximum | deviation | five-number; `BinPolicy` = Buckets | Width | Extent; `CalendarAxis` = hour-by-day | month | season | weekday.
- Packages: Rasm, DynamicData, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new reducer is one `ChartReducer` row projecting the spread; a new civil axis is one `CalendarAxis` row; zero new surface.
- Boundary: every order statistic reads the kernel `Distribution<Scalar>` under `QuantileRule.NearestRank` — the declared percentile is an observation the population CONTAINS, so a p95 a viewer reads beside a measured maximum belongs to the same sample set, and min, max, mean, deviation, median, quartiles, and the requested quantile are ONE kernel fold per group rather than ten hand statistics over a privately sorted array; the weighted mean is the kernel `Stat<Scalar>.Of` weighted fold, the one reduction a stream of PRE-REDUCED rows admits without distortion, and a zero-mass window REFUSES on the kernel rail rather than reading zero. `GroupSpread.Of` is the one construction, so a row set reducing one group nine ways folds once. Calendar reshape rides NodaTime through one injected zone and calendar policy, so an hour-by-day matrix, a month rollup, and a season rollup resolve their civil fields through the same zone a time brush and an axis label resolve theirs through, and a UTC-offset literal anywhere in a reshape is the deleted form.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// `Arity` is the magnitude count rows of this shape carry — what the spec checks a layer's encoding against —
// and `Keyed` marks the shapes whose rows are grouped rather than ordered.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartShape {
    public static readonly ChartShape Series = new("series", arity: 1, keyed: false);
    public static readonly ChartShape Grouped = new("grouped", arity: 1, keyed: true);
    public static readonly ChartShape Matrix = new("matrix", arity: 2, keyed: true);
    public static readonly ChartShape Summary = new("summary", arity: 5, keyed: true);
    public static readonly ChartShape Span = new("span", arity: 2, keyed: true);

    public int Arity { get; }

    public bool Keyed { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// The one per-group statistics carrier: the kernel spread (exact order statistics under NearestRank), the
// plain sum, and the weighted mean with its mass. Every reducer row is a PROJECTION of this value, so one
// group reduces once however many rows read it.
public readonly record struct GroupSpread(Distribution<Scalar> Spread, double Sum, double WeightedMean, double Mass) {
    static readonly Op Key = Op.Of(name: "chart.reduce");

    // ONE construction, two admission shapes: a datum group carries weights, a bare sample run carries none —
    // absence stays absent (`Stat` folds unweighted) rather than fabricating unit weights the stream never spent.
    public static Fin<GroupSpread> Of(Seq<ChartDatum> group, Seq<double> taus) {
        // Pair-filtered admission: a non-finite sample drops WITH its weight, so the weighted mean never
        // pairs a surviving value with a rejected sample's population.
        Seq<(Scalar Value, double Weight)> admitted = group.Choose(static datum =>
            Scalar.From(datum.Value.A).ToOption().Map(value => (value, datum.Weight)));
        Seq<Scalar> values = admitted.Map(static pair => pair.Value);
        Seq<double> weights = admitted.Map(static pair => pair.Weight);
        return Distribution<Scalar>.Of(values, Ranks(taus), Key, rule: Some(QuantileRule.NearestRank))
            .Bind(spread => Stat<Scalar>.Of(values, Key, weights: Some(weights))
                .Map(weighted => new GroupSpread(
                    spread,
                    group.Fold(0d, static (sum, datum) => sum + datum.Value.A),
                    weighted.Mean,
                    weighted.Mass)));
    }

    public static Fin<GroupSpread> Of(Seq<double> samples, Seq<double> taus) {
        Seq<Scalar> values = samples.Choose(static sample => Scalar.From(sample).ToOption());
        return Distribution<Scalar>.Of(values, Ranks(taus), Key, rule: Some(QuantileRule.NearestRank))
            .Bind(spread => Stat<Scalar>.Of(values, Key)
                .Map(stat => new GroupSpread(spread, samples.Fold(0d, static (sum, sample) => sum + sample), stat.Mean, stat.Mass)));
    }

    static Seq<double> Ranks(Seq<double> taus) =>
        (Seq(25d, 75d) + taus.Map(static tau => Math.Clamp(tau * 100d, 0d, 100d))).Distinct();

    public double Percentile(double rank) =>
        Spread.Percentiles.Find(row => row.Percentile == rank).Map(static row => row.Value.To())
            .IfNone(Spread.Median.To());
}

// --- [TABLES] -------------------------------------------------------------------------------
// Reducers as projections of ONE kernel spread. Each row declares the magnitude count it produces, so a
// five-number reducer feeding a box layer and a scalar reducer feeding a line layer are distinguishable at
// declaration.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartReducer {
    public static readonly ChartReducer Count = new("count", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.Spread.Summary.Count]));
    public static readonly ChartReducer Sum = new("sum", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.Sum]));
    public static readonly ChartReducer Mean = new("mean", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.Spread.Summary.Mean]));
    // The one reduction a stream of PRE-REDUCED rows admits without distortion: an unweighted mean of bucket
    // means answers the mean of buckets wherever bucket populations differ.
    public static readonly ChartReducer Weighted = new("weighted", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.WeightedMean]));
    public static readonly ChartReducer Median = new("median", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.Spread.Median.To()]));
    public static readonly ChartReducer Quantile = new("quantile", arity: 1, static (spread, tau) => ChartMagnitude.Of([spread.Percentile(Math.Clamp(tau * 100d, 0d, 100d))]));
    public static readonly ChartReducer Minimum = new("minimum", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.Spread.Summary.Minimum.To()]));
    public static readonly ChartReducer Maximum = new("maximum", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.Spread.Summary.Maximum.To()]));
    public static readonly ChartReducer Deviation = new("deviation", arity: 1, static (spread, _) => ChartMagnitude.Of([spread.Spread.Summary.Deviation(MomentNormalizer.Population)]));
    // Five magnitudes in the order the box coordinate takes them: maximum, upper quartile, lower quartile,
    // minimum, median — the projection reorders once here rather than at each box layer.
    public static readonly ChartReducer FiveNumber = new("five-number", arity: 5, static (spread, _) => ChartMagnitude.Of([
        spread.Spread.Summary.Maximum.To(), spread.Percentile(75d), spread.Percentile(25d),
        spread.Spread.Summary.Minimum.To(), spread.Spread.Median.To()]));

    public int Arity { get; }

    [UseDelegateFromConstructor]
    public partial ChartMagnitude Reduce(GroupSpread spread, double tau);
}

// Binning policy as a closed value: a histogram over an auto-ranged extent and one over a declared extent are
// the same fold at two policies, and a comparison across two feeds needs the declared one.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BinPolicy {
    private BinPolicy() { }
    public sealed record Buckets(int Count) : BinPolicy;
    public sealed record Width(double Span) : BinPolicy;
    public sealed record Extent(double Low, double High, int Count) : BinPolicy;
}

// The civil-calendar axis a reshape folds on; every row resolves through the injected zone so a reshape, a
// time brush, and an axis label read one civil calendar.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CalendarAxis {
    public static readonly CalendarAxis HourByDay = new("hour-by-day", matrix: true,
        static local => (Column: local.DayOfYear - 1, Row: local.Hour), static local => $"{local.Date:yyyy-MM-dd}T{local.Hour:00}");
    public static readonly CalendarAxis Month = new("month", matrix: false,
        static local => (Column: local.Month - 1, Row: 0), static local => $"{local.Year:0000}-{local.Month:00}");
    public static readonly CalendarAxis Season = new("season", matrix: false,
        static local => (Column: (local.Month % 12) / 3, Row: 0), static local => $"{local.Year:0000}-s{(local.Month % 12) / 3}");
    public static readonly CalendarAxis Weekday = new("weekday", matrix: false,
        static local => (Column: (int)local.DayOfWeek % 7, Row: 0), static local => $"{(int)local.DayOfWeek % 7}");

    public bool Matrix { get; }

    [UseDelegateFromConstructor]
    public partial (int Column, int Row) Cell(LocalDateTime local);

    [UseDelegateFromConstructor]
    public partial string Group(LocalDateTime local);
}

// The zone and calendar every civil projection resolves through, bound at composition beside the clock — a
// board rendered in a proof lane and one on a workstation must fold the same hours into the same cells.
public sealed record CalendarPolicy(DateTimeZone Zone, CalendarSystem Calendar) {
    public LocalDateTime Civil(Instant at) => at.InZone(Zone).LocalDateTime.WithCalendar(Calendar);
}
```

## [03]-[TRANSFORM_CHAIN]

- Owner: `TransformRow` — one declared reshape between feed and series, each case naming the shape it consumes and produces; `TransformChain` — the declaration-time shape fold and the one evaluator.
- Cases: Bin | Aggregate | Window | Calendar | Cumulative | LoadDuration | Downsample | Shift | Clamp.
- Entry: `TransformChain.Admit(Seq<TransformRow> rows, ChartShape source)` — the declaration-time shape fold; `TransformChain.Run(Seq<TransformRow> rows, Seq<ChartDatum> source, CalendarPolicy calendar)` — the one evaluator on the `Fin` rail, so a group whose whole population the kernel spread refuses names itself instead of rendering as a zero.
- Auto: an hourly carpet matrix, a monthly rollup, a box-plot summary, a load-duration curve, and a downsampled min/max band are five declarations over one evaluator, so no tile reshapes data in view code.
- Packages: Rasm, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new reshape is one `TransformRow` case naming its input and output shape and one evaluator arm; zero new surface.
- Boundary: a transform names its INPUT and OUTPUT shape, so an unsatisfiable chain refuses at declaration — a quantile reducer over a matrix, a calendar reshape over a summary, and a downsample over a span track are chains no evaluator sees, and the terminal shape's arity is what `ChartSpec.Admit` checks the encoding against. The downsampler is a ROW rather than a stream column, so a feed declaring no rows passes untouched and a bucket policy can never sit inert. Every step is a pure rewrite, so the whole chain replays off a captured snapshot for the proof lane with no live feed.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransformRow {
    private TransformRow() { }

    public sealed record Bin(BinPolicy Policy) : TransformRow;
    public sealed record Aggregate(ChartReducer Reducer, double Tau) : TransformRow;
    public sealed record Window(int Span, ChartReducer Reducer, double Tau) : TransformRow;
    public sealed record Calendar(CalendarAxis Axis, ChartReducer Reducer, double Tau) : TransformRow;
    public sealed record Cumulative() : TransformRow;
    public sealed record LoadDuration() : TransformRow;
    public sealed record Downsample(int Buckets) : TransformRow;
    public sealed record Shift(Duration Stamp, int Ordinal) : TransformRow;
    public sealed record Clamp(double Low, double High) : TransformRow;

    // Declared shapes as row data: binning turns an ordered series into keyed buckets, aggregation collapses
    // a keyed set at the reducer's arity, the calendar row answers a matrix or a keyed rollup by its axis, the
    // downsampler preserves shape because it selects points rather than deriving them.
    public (ChartShape In, ChartShape Out) Shapes => Switch(
        bin: static _ => (ChartShape.Series, ChartShape.Grouped),
        aggregate: static row => (ChartShape.Grouped, row.Reducer.Arity >= ChartShape.Summary.Arity ? ChartShape.Summary : ChartShape.Series),
        window: static _ => (ChartShape.Series, ChartShape.Series),
        calendar: static row => (ChartShape.Series, row.Axis.Matrix ? ChartShape.Matrix : ChartShape.Grouped),
        cumulative: static _ => (ChartShape.Series, ChartShape.Series),
        loadDuration: static _ => (ChartShape.Series, ChartShape.Series),
        downsample: static _ => (ChartShape.Series, ChartShape.Series),
        shift: static _ => (ChartShape.Series, ChartShape.Series),
        clamp: static _ => (ChartShape.Series, ChartShape.Series));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class TransformChain {
    public static Fin<ChartShape> Admit(Seq<TransformRow> rows, ChartShape source) =>
        rows.Fold(Fin.Succ(source), static (rail, row) => rail.Bind(held =>
            row.Shapes switch {
                var (want, next) when want == held || (want == ChartShape.Grouped && held.Keyed) => Fin.Succ(next),
                var (want, _) => Fin.Fail<ChartShape>(new ChartFault.TransformRejected($"{row.GetType().Name} consumes {want.Key}, chain carries {held.Key}")),
            }));

    // Evaluation walks the same rows the admission walked, so the step set a board declares and the step set
    // it runs are one sequence. The rail carries the kernel spread's refusal — a group with no admissible
    // population names itself rather than reducing to a fabricated zero.
    public static Fin<Seq<ChartDatum>> Run(Seq<TransformRow> rows, Seq<ChartDatum> source, CalendarPolicy calendar) =>
        rows.Fold(Fin.Succ(source), (rail, row) => rail.Bind(held => row.Switch(
            state: (Held: held, Calendar: calendar),
            bin: static (s, r) => Fin.Succ(Binned(s.Held, r.Policy)),
            aggregate: static (s, r) => Reduced(s.Held, r.Reducer, r.Tau),
            window: static (s, r) => Rolled(s.Held, r.Span, r.Reducer, r.Tau),
            calendar: static (s, r) => Folded(s.Held, r.Axis, r.Reducer, r.Tau, s.Calendar),
            cumulative: static (s, _) => Fin.Succ(Accumulated(s.Held)),
            loadDuration: static (s, _) => Fin.Succ(Ranked(s.Held)),
            downsample: static (s, r) => Fin.Succ(ChartFolds.Lttb(s.Held, r.Buckets, static datum => (datum.X, datum.Value.A))),
            shift: static (s, r) => Fin.Succ(Shifted(s.Held, r.Stamp, r.Ordinal)),
            clamp: static (s, r) => Fin.Succ(Clamped(s.Held, r.Low, r.High)))));

    // The comparison alignment: a stamp shift moves the instant AND the axis ordinal forward together, so a
    // time brush and a tooltip read the same row — a shift that moved only `X` would leave every ghost point
    // brushing at its original instant. An ordinal shift moves the index alone.
    static Seq<ChartDatum> Shifted(Seq<ChartDatum> rows, Duration stamp, int ordinal) =>
        stamp == Duration.Zero && ordinal == 0
            ? rows
            : rows.Map((datum, index) => datum with {
                X = stamp == Duration.Zero ? index + ordinal : datum.X + stamp.BclCompatibleTicks,
                Stamp = datum.Stamp.Map(at => at + stamp),
            });

    // The ramp clamp lands on the DATA because the package's heat legend reads the series' own measured
    // bounds: a clamp declared on the legend alone would caption a range the ramp does not paint. The
    // POPULATION stays whole — clamping how much a bucket stands for would silently reweight every mean.
    static Seq<ChartDatum> Clamped(Seq<ChartDatum> rows, double low, double high) =>
        high <= low ? rows : rows.Map(datum => datum with {
            Value = datum.Value with {
                A = Math.Clamp(datum.Value.A, low, high),
                B = datum.Arity > 1 ? Math.Clamp(datum.Value.B, low, high) : datum.Value.B,
            },
        });

    // Binning writes the bucket ORDINAL as `X` and the bucket as the group label, so a histogram reads as a
    // categorical bar without a second axis vocabulary and a rebinned comparison aligns by ordinal.
    static Seq<ChartDatum> Binned(Seq<ChartDatum> rows, BinPolicy policy) =>
        rows.IsEmpty ? rows : Bounds(rows, policy) switch {
            var (low, high, count) when count > 0 && high > low => rows.Map(datum =>
                datum with {
                    Group = Bucket(datum.Value.A, low, high, count).ToString(CultureInfo.InvariantCulture),
                    X = Bucket(datum.Value.A, low, high, count),
                }),
            _ => rows,
        };

    static (double Low, double High, int Count) Bounds(Seq<ChartDatum> rows, BinPolicy policy) =>
        (rows.Min(static datum => datum.Value.A), rows.Max(static datum => datum.Value.A)) switch {
            var (low, high) => policy.Switch(
                state: (Low: low, High: high),
                buckets: static (s, p) => (s.Low, s.High, p.Count),
                width: static (s, p) => (s.Low, s.High, p.Span > 0d ? (int)Math.Ceiling((s.High - s.Low) / p.Span) : 0),
                extent: static (_, p) => (p.Low, p.High, p.Count)),
        };

    static int Bucket(double value, double low, double high, int count) =>
        Math.Clamp((int)((value - low) / (high - low) * count), 0, count - 1);

    // One kernel spread per group feeds every statistic the reducer reads.
    static Fin<Seq<ChartDatum>> Reduced(Seq<ChartDatum> rows, ChartReducer reducer, double tau) =>
        toSeq(rows.GroupBy(static datum => datum.Group, StringComparer.Ordinal))
            .Map(static (group, index) => (Index: index, Key: group.Key, Rows: toSeq(group)))
            .TraverseM(cell => GroupSpread.Of(cell.Rows, Seq(tau)).Map(spread => ChartDatum.Of(
                x: cell.Rows.Head.X is var first && double.IsFinite(first) ? first : cell.Index,
                value: reducer.Reduce(spread, tau),
                arity: reducer.Arity,
                weight: spread.Mass,
                group: cell.Key,
                stamp: cell.Rows.Head.Stamp)))
            .As();

    // The rolling fold reduces the trailing `span` rows at each position, so a moving median and a moving p95
    // are one declaration at two reducer rows. Exactness is the law here too, so each window folds the same
    // kernel spread — the streaming sketch answers an approximation a viewer would read beside exact tiles.
    static Fin<Seq<ChartDatum>> Rolled(Seq<ChartDatum> rows, int span, ChartReducer reducer, double tau) =>
        span <= 1 ? Fin.Succ(rows) : rows
            .Map((datum, index) => (Index: index, Datum: datum))
            .TraverseM(cell => GroupSpread.Of(
                    rows.Skip(int.Max(0, cell.Index - span + 1)).Take(int.Min(span, cell.Index + 1)), tau)
                .Map(spread => cell.Datum with { Value = reducer.Reduce(spread, tau), Arity = reducer.Arity }))
            .As();

    // The calendar fold is the carpet and rollup owner: a matrix axis writes the cell column as `X` and the
    // cell row as the second magnitude — exactly the weighted coordinate a heat layer reads — so an
    // hour-by-day carpet is one transform row and one heat layer rather than a bespoke visual.
    static Fin<Seq<ChartDatum>> Folded(Seq<ChartDatum> rows, CalendarAxis axis, ChartReducer reducer, double tau, CalendarPolicy calendar) =>
        toSeq(rows.Choose(datum => datum.Stamp.Map(stamp => (Datum: datum, Civil: calendar.Civil(stamp))))
            .GroupBy(row => axis.Group(row.Civil), StringComparer.Ordinal))
            .Map(static group => (Key: group.Key, Rows: toSeq(group)))
            .TraverseM(cell => GroupSpread.Of(cell.Rows.Map(static row => row.Datum), Seq(tau))
                .Map(spread => axis.Cell(cell.Rows.Head.Civil) switch {
                    var at => ChartDatum.Of(
                        x: at.Column,
                        value: axis.Matrix
                            ? ChartMagnitude.Of([at.Row, reducer.Reduce(spread, tau).A])
                            : reducer.Reduce(spread, tau),
                        arity: axis.Matrix ? 2 : reducer.Arity,
                        weight: spread.Mass,
                        group: cell.Key,
                        stamp: cell.Rows.Head.Datum.Stamp),
                }))
            .As();

    static Seq<ChartDatum> Accumulated(Seq<ChartDatum> rows) =>
        rows.Fold((Running: 0d, Acc: Seq<ChartDatum>()), static (state, datum) =>
            (state.Running + datum.Value.A) switch {
                var running => (Running: running, Acc: state.Acc.Add(datum with { Value = ChartMagnitude.Of([running]), Arity = 1 })),
            }).Acc;

    // A load-duration curve is the same population re-indexed: values descend and `X` becomes the fraction of
    // the period at or above that value, so a duration read is a coordinate read rather than a chart type.
    static Seq<ChartDatum> Ranked(Seq<ChartDatum> rows) =>
        rows.Count == 0 ? rows : toSeq(rows.OrderByDescending(static datum => datum.Value.A))
            .Map((datum, index) => datum with { X = (double)index / rows.Count });
}
```

## [04]-[FEED_ROSTER]

- Owner: `ChartStream` — the typed feed roster: window, bound, cadence, and shape are ROW columns and the named rows are the roster; `ChartFolds` — the retention and cadence folds and the pure downsampler.
- Cases: `ComputeReceipts`, `Analytical`, `HostEvents`, `Scripted`, `ReceiptTimeline` — each row binds one `DataSource` case key with its retention posture.
- Entry: `ChartFolds.Shape(stream, source)` — retention onto `ExpireAfter`/`LimitSizeTo`; `ChartFolds.Snapshots(stream, shaped, layer, calendar)` — cadence over the materialized state stream, then the declared rows through the one evaluator; `ChartFolds.Lttb(points, buckets, project)` — the pure largest-triangle-three-buckets fold the downsample row composes.
- Packages: DynamicData, NodaTime, LanguageExt.Core
- Growth: a new feed class is one static `ChartStream` row; a new bound is one column value on its row; zero new surface.
- Boundary: the roster is TYPED rows, so window, bound, cadence, and shape live on the row and nowhere else — the markdown mirror the fence rosters once trailed is gone. `Analytical` carries no retention because the analytical lane is a SNAPSHOT source: each refresh replaces the whole keyed set, so retention is one query answer and expiry is the next refresh — a window would truncate an answer the store already bounded and a size limit would evict rows of the answer currently displayed. Rows sharing one source case are one source read under two retention postures, never separate sources: `ReceiptTimeline` holds a longer correlation horizon than `ComputeReceipts` and declares no downsample row because `Lttb` folds an `(x, y)` point series and a SPAN track has no such point to keep. `Scripted` is the proof lane's deterministic feed — its script seeds derive at the `DataSource.FakeDeterministic` owner from the kernel `Deterministic` lanes, so a replay renders the same board twice. `ToCollection` precedes `Sample`, so cadence samples state rather than dropping deltas, and the chart lock owns the terminal series swap.

```csharp signature
// --- [TABLES] -------------------------------------------------------------------------------
// The typed feed roster. `SourceKey` selects the `DataSource` case at the livedata seam; `Window` and `Bound`
// are consumed by `ChartFolds.Shape` and `Cadence` by `Snapshots`, so a policy column can never sit inert
// beside a fold that ignores it.
public sealed record ChartStream(
    string Key,
    string SourceKey,
    Option<Duration> Window,
    Option<int> Bound,
    Option<Duration> Cadence,
    Seq<TransformRow> Shape) {
    public static readonly ChartStream ComputeReceipts = new(
        "compute-receipt-stream", nameof(DataSource<ChartDatum, string>.ReceiptStream),
        Some(Duration.FromSeconds(120)), Some(8192), Some(Duration.FromMilliseconds(250)),
        Seq<TransformRow>(new TransformRow.Downsample(512)));
    public static readonly ChartStream Analytical = new(
        "persistence-analytical", nameof(DataSource<ChartDatum, string>.PersistenceQuery),
        None, None, Some(Duration.FromSeconds(1)), Seq<TransformRow>());
    public static readonly ChartStream HostEvents = new(
        "host-document-events", nameof(DataSource<ChartDatum, string>.HostDocumentEvents),
        Some(Duration.FromSeconds(300)), Some(4096), Some(Duration.FromMilliseconds(500)),
        Seq<TransformRow>(new TransformRow.Downsample(256)));
    public static readonly ChartStream Scripted = new(
        "fake-deterministic", nameof(DataSource<ChartDatum, string>.FakeDeterministic),
        None, None, None, Seq<TransformRow>());
    public static readonly ChartStream ReceiptTimeline = new(
        "receipt-timeline", nameof(DataSource<ChartDatum, string>.ReceiptStream),
        Some(Duration.FromSeconds(300)), Some(4096), Some(Duration.FromMilliseconds(500)), Seq<TransformRow>());
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ChartFolds {
    // Every retention axis lands on a composed operator: Window -> ExpireAfter, Bound -> LimitSizeTo.
    public static IObservable<IChangeSet<T, TKey>> Shape<T, TKey>(ChartStream stream, IObservable<IChangeSet<T, TKey>> source) where TKey : notnull =>
        stream.Bound
            .Map(bound => stream.Window
                .Map(window => source.ExpireAfter(_ => window.ToTimeSpan()).LimitSizeTo(bound))
                .IfNone(source.LimitSizeTo(bound)))
            .IfNone(stream.Window
                .Map(window => source.ExpireAfter(_ => window.ToTimeSpan()))
                .IfNone(source));

    // Cadence gates the materialized STATE stream, never the delta stream — Sample over ToCollection drops no
    // cache delta — and the declared rows then run over that state, so the reshape evaluates once per rendered
    // frame rather than once per delta the frame would have coalesced anyway. The element is the evaluator's
    // rail: a refused reduction names itself on the tile instead of rendering an empty chart.
    public static IObservable<Fin<Seq<ChartDatum>>> Snapshots(
        ChartStream stream, IObservable<IChangeSet<ChartDatum, string>> shaped, Seq<TransformRow> layer, CalendarPolicy calendar) =>
        stream.Cadence
            .Map(every => shaped.ToCollection().Sample(every.ToTimeSpan()))
            .IfNone(shaped.ToCollection())
            .Select(state => TransformChain.Run(stream.Shape + layer, toSeq(state), calendar));

    // Largest-triangle-three-buckets: each interior bucket keeps the point forming the largest triangle with
    // the previously kept anchor and the NEXT bucket's mean, so the envelope survives downsampling where a
    // mean or a stride would flatten it; the ends are pinned and the anchor threads the fold.
    public static Seq<T> Lttb<T>(Seq<T> points, int buckets, Func<T, (double X, double Y)> project) =>
        buckets < 3 || points.Count <= buckets
            ? points
            : Range(1, buckets - 2)
                .Fold(
                    (Acc: Seq<T>().Add(points[0]), Anchor: project(points[0])),
                    (state, bucket) => Peak(points, project, Window(points.Count, buckets, bucket), state.Anchor) switch {
                        var pick => (Acc: state.Acc.Add(pick), Anchor: project(pick)),
                    })
                .Acc
                .Add(points[^1]);

    // Bucket bounds over the interior: `Lo..Hi` is the span this step picks from, `Hi..End` the next span the
    // target mean averages, clamped so the last interior bucket never reads past the pinned tail.
    static (int Lo, int Hi, int End) Window(int count, int buckets, int bucket) => (
        Lo: 1 + (((bucket - 1) * (count - 2)) / (buckets - 2)),
        Hi: 1 + ((bucket * (count - 2)) / (buckets - 2)),
        End: Math.Min(1 + (((bucket + 1) * (count - 2)) / (buckets - 2)), count - 1));

    // Each candidate's triangle area computes ONCE and rides the comparison.
    static T Peak<T>(Seq<T> points, Func<T, (double X, double Y)> project, (int Lo, int Hi, int End) window, (double X, double Y) anchor) =>
        Mean(points, project, window) switch {
            var target => points.Skip(window.Lo).Take(window.Hi - window.Lo)
                .Fold(
                    (Best: -1d, Pick: points[window.Lo]),
                    (best, candidate) => Area(anchor, project(candidate), target) switch {
                        var area => area > best.Best ? (Best: area, Pick: candidate) : best,
                    })
                .Pick,
        };

    // An empty next bucket takes the pinned tail as its target rather than dividing by nothing.
    static (double X, double Y) Mean<T>(Seq<T> points, Func<T, (double X, double Y)> project, (int Lo, int Hi, int End) window) =>
        points.Skip(window.Hi).Take(window.End - window.Hi)
            .Fold((X: 0d, Y: 0d, N: 0d), (sum, point) => project(point) switch {
                var at => (X: sum.X + at.X, Y: sum.Y + at.Y, N: sum.N + 1d),
            }) switch {
                { N: 0d } => project(points[^1]),
                var sum => (X: sum.X / sum.N, Y: sum.Y / sum.N),
            };

    internal static double Area((double X, double Y) a, (double X, double Y) b, (double X, double Y) c) =>
        Math.Abs(((a.X - c.X) * (b.Y - a.Y)) - ((a.X - b.X) * (c.Y - a.Y))) * 0.5;
}
```

## [05]-[PLAN_FEEDS]

- Owner: `PlanFeeds` — the folds projecting the Bim `CostSchedule` and `ScheduleNetwork` planning receipts into chart rows and event marks.
- Entry: `PlanFeeds.Schedule(ScheduleNetwork network, Op key)` — composes the Bim CPM (`ScheduleCpm.Schedule`) and folds each ACTIVITY onto one span row: `X` the early-start ordinal, slot A the scheduled working days, slot B the total float in days — criticality DERIVES as `float <= 0`, so no third slot restates it; `PlanFeeds.Milestones(ScheduleNetwork network, string layer, Op key)` — milestones cross as `ChartAnnotation.Moment` event lines at their early start, because a zero-content event drawn as a zero-length bar is invisible exactly where it matters; `PlanFeeds.Cost(CostSchedule schedule, Op key, Seq<ExchangeRate> fx = default)` — composes the Bim railed `Rollup` and folds its per-category partition onto categorical rows in the schedule currency, so a stacked cost tile sums values one repricing authority already made summable.
- Receipt: the witness board rows are the cost-and-schedule dashboards — a Gantt-class span layer over `Schedule` with `Milestones` on its mark plane, and a stacked-column cost tile over `Cost` — each an ordinary `ChartLayer` naming these folds' output as its pinned rows or its scripted feed.
- Packages: Rasm.Bim, NodaTime, LanguageExt.Core
- Growth: a new planning read is one fold here projecting a receipt column the Bim owner already carries; zero new surface.
- Boundary: receipts are CONSUMED as feed values, never re-solved — the CPM walk, the calendar election (`network.CalendarFor(task)`, never a network-wide calendar parameter), the float derivation, and the currency repricing are the Bim owner's, and this fold reads `TaskGrain`, `CriticalPath`, and `CostRollup` columns whole; a `bool IsMilestone` read and a network-wide `WorkCalendar` argument are the deleted forms the Bim page's own laws name. Cost rows fold the `ByCategory` partition of the RAILED rollup, so a mixed-currency estimate reaches the chart already repriced or refuses by name — summing native amounts across currencies inside a reducer is unspellable because the fold never sees them. Instants cross to chart space as `DateTime.Ticks` exactly as every temporal coordinate on this plane does.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PlanFeeds {
    // One span row per ACTIVITY off the CPM receipt: slot A the scheduled work in days, slot B the total
    // float in days. Critical is `Value.B <= 0` — a derived read, not a third slot.
    public static Fin<Seq<ChartDatum>> Schedule(ScheduleNetwork network, Op key) =>
        network.Schedule(key).Map(paths => network.Tasks
            .Filter(task => task.Grain != TaskGrain.Milestone)
            .Choose(task => paths.Find(task.GlobalId).Map(path => ChartDatum.Of(
                x: path.EarlyStart.ToDateTimeUtc().Ticks,
                value: ChartMagnitude.Of([
                    task.WorkContent(network.CalendarFor(task)).TotalDays,
                    path.TotalFloat.TotalDays]),
                arity: 2,
                weight: 1d,
                group: task.Name,
                stamp: Some(path.EarlyStart)))));

    // Milestones are EVENTS: each crosses as a Moment mark on the named layer at its CPM early start, so the
    // flag renders as a full-height hairline the way a zero-content fact reads, and the mark clusters under
    // the plane's own density fold when a phase gate stacks ten of them.
    public static Fin<Seq<ChartAnnotation>> Milestones(ScheduleNetwork network, string layer, Op key) =>
        network.Schedule(key).Map(paths => network.Tasks
            .Filter(task => task.Grain == TaskGrain.Milestone)
            .Choose(task => paths.Find(task.GlobalId).Map(path =>
                (ChartAnnotation)new ChartAnnotation.Moment(layer, path.EarlyStart, task.Name, Severity.Info))));

    // Categorical cost rows off the RAILED rollup's per-category partition, in the schedule currency the Bim
    // repricing authority already settled. `Map` iterates key-ordered, so the ordinals are deterministic.
    public static Fin<Seq<ChartDatum>> Cost(CostSchedule schedule, Op key, Seq<ExchangeRate> fx = default) =>
        schedule.Rollup(key, fx).Map(rollup => toSeq(rollup.ByCategory)
            .Map((entry, index) => ChartDatum.Of(
                x: index,
                value: ChartMagnitude.Of([(double)entry.Value.Amount]),
                arity: 1,
                weight: 1d,
                group: entry.Key,
                stamp: None)));
}
```

## [06]-[RESEARCH]

(none)
